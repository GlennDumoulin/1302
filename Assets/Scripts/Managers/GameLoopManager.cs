using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NextBattlePhaseEventArgs : EventArgs
{
    public BattlePhase NextPhase { get; }

    public NextBattlePhaseEventArgs(BattlePhase battlePhase)
    {
        NextPhase = battlePhase;
    }
}

public class DataEventArgs : EventArgs
{
    public BattlePhase CurrentBattlePhase { get; }

    public DataEventArgs() : this(BattlePhase.DeployPhase) { }

    public DataEventArgs(BattlePhase battlePhase)
    {
        CurrentBattlePhase = battlePhase;
    }
}

public class GameSideEventArgs : EventArgs
{
    public GameSides Side { get; }

    public GameSideEventArgs(GameSides side)
    {
        Side = side;
    }
}

public class GameLoopManager : MonoBehaviour, IDataPersistance<LevelPresetData>
{
    private bool _hasGameEnded = false;

    private bool _isFirstTurn = true;
    public bool IsFirstTurn
    {
        get { return _isFirstTurn; }
        set { _isFirstTurn = value; }
    }

    private bool _isAiPlaying = false;

    private int _totalTroopsDeployed = 0;
    public int TotalTroopsDeployed
    {
        get { return _totalTroopsDeployed; }
        set { _totalTroopsDeployed = value; }
    }

    [SerializeField] private int _deployTroopLimit = 2;

    private InputManager _inputManager = null;
    private AIManager _aiManager = null;

    private int _playerTroopsOnField = 0;
    private int _enemyTroopsOnField = 0;

    private GameSides _playerSide = GameSides.Flemish;
    public GameSides PlayerSide { get { return _playerSide; } }

    //This field keeps track of who's turn it is, whether its the Flemish or the French's
    private GameSides _currentGameSide = GameSides.Flemish;
    public GameSides CurrentGameSide { get { return _currentGameSide;} }
    private GameSides _startingSide = GameSides.Flemish;

    private BattlePhase _currentBattlePhase = BattlePhase.DeployPhase;
    public BattlePhase CurrentBattlePhase 
    { 
        get { return _currentBattlePhase; } 
        set { _currentBattlePhase = value; }
    }

    private Deck _playerDeck = null;
    private List<TroopCard> _enemyDeck = new List<TroopCard>();

    [SerializeField] private Transform _enemyDeckParentSocket = null;

    public event EventHandler OnMoveToDeployPhase;
    public event EventHandler OnTurnEnd;
    public event EventHandler OnSwitchSide;
    public event EventHandler OnReachDeployLimit;
    public event EventHandler OnGameStart;
    public event EventHandler OnUnitDeployed;
    public event EventHandler OnNextTurn;

    public event EventHandler<GameSideEventArgs> OnGameEnd;

    public event EventHandler<NextBattlePhaseEventArgs> OnPhaseAdvanced;

    public event EventHandler<DataEventArgs> OnDataLoaded;

    private List<TroopController> _troops = new List<TroopController>();   
    public List<TroopController> Troops 
    {
        get { return _troops; }
    }

    private Transform _troopsParentSocket = null;

    private TutorialScriptHandler _tutorial = null;
    private int _currentTutorialIdx = 0;

    private void Awake()
    {
        // Get the parent socket for spawned troops
        GameObject troopSocket = GameObject.Find("Units/Troops");
        if (troopSocket) _troopsParentSocket = troopSocket.transform;

        _playerSide = StaticDataHelper.playerSide;

        _playerDeck = FindObjectOfType<Deck>();

        _inputManager = FindObjectOfType<InputManager>();
        _aiManager = FindObjectOfType<AIManager>();
        _tutorial = FindObjectOfType<TutorialScriptHandler>();

        SetFirstSide();
        _currentBattlePhase = BattlePhase.DeployPhase;
    }

    private void Start()
    {
        // Check if the AI is playing
        if (_aiManager) _isAiPlaying = _aiManager.IsAiEnabled;

        if (CurrentGameSide == PlayerSide && _playerDeck)
            StartCoroutine(ShowDeckAtStart(_playerDeck));

        //Game started
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnable()
    {
        OnDataLoaded += HandlePlayerDeckState;
    }
    private void OnDisable()
    {
        OnDataLoaded -= HandlePlayerDeckState;
    }

    private IEnumerator ShowDeckAtStart(Deck deck)
    {
        // Set visible on next frame, not waiting gave issues on mobile devices
        yield return null;

        SetDeckVisibility(deck, DeckVisibility.FullVisible);
    }

    private void SetDeckVisibility(Deck deck, DeckVisibility visibility)
    {
        if (!deck) return;

        deck.SetVisibility(visibility);
    }

    public void RemoveEnemyCard(TroopCard card)
    {
        if (!_enemyDeck.Contains(card)) return;

        _enemyDeck.Remove(card);
    }

    public void MoveToNextPhase()
    {
        ///
        /// There are five possible outcomes for what phase to switch to:
        /// 1. Start on Movement Attack Phase, go to Deploy Phase
        /// 2. Start on Deploy Phase, Switch Turn, Opponent Starts on Movement Attack Phase
        /// 3. Start on Deploy Phase, Switch Turn, Opponent Starts on Deploy Phase
        /// 4. Start on Movement Attack Phase, Switch Turn, Opponent Starts on Movement Attack Phase
        /// 5. Start on Movement Attack Phase, Switch Turn, Opponent Starts on Deploy Phase
        ///

        // Reset the input manager
        if (_inputManager) _inputManager.ResetTouchObjects();

        //Check if we go to the Deploy Phase. If not, we end our turn.
        if (CurrentBattlePhase == BattlePhase.MovementAttackPhase && GetCurrentSideDeckSize() > 0)
        {
            MoveToDeployPhase();
            OnPhaseAdvanced?.Invoke(this, new NextBattlePhaseEventArgs(BattlePhase.DeployPhase));
            return;
        }

        TurnEnd();
      
    }

    public void MoveToDeployPhase()
    {
        _currentBattlePhase = BattlePhase.DeployPhase;
        OnMoveToDeployPhase?.Invoke(this, EventArgs.Empty);

        //Stop showing health
        ShowHealthUnits(false);
    }

    public void TurnEnd()
    {
        //Switch the side and update the data accordingly
        SwitchSide();

        //Let the tactical view know we are at the start of the turn
        FindAnyObjectByType<TacticalView>().StartTurn();

        //Check if the enemy starts in the Deploy Phase. If not, the enemy starts in the Movement/Attack Phase
        if (GetCurrentSideTroopsOnField() < 1)
        {
            MoveToDeployPhase();
            OnPhaseAdvanced?.Invoke(this, new NextBattlePhaseEventArgs(BattlePhase.DeployPhase));
            return;
        }

        //If the opponent is on the Movement Attack Phase, we have to check whether to set the button to the Deploy Troops button or the End Turn Button
        OnPhaseAdvanced?.Invoke(this, new NextBattlePhaseEventArgs(CheckToSetButtonToEnd()));

        OnNextTurn?.Invoke(this, EventArgs.Empty);
    }

    public bool CanDeployTroop()
    {
        // Check if the max deployment limit has been reached
        if (_totalTroopsDeployed >= _deployTroopLimit || (_isFirstTurn && _totalTroopsDeployed >= _deployTroopLimit - 1))
        {
            OnReachDeployLimit?.Invoke(this, EventArgs.Empty);
            return false;
        }

        return true;
    }

    public void UpdateDeployCount()
    {
        _totalTroopsDeployed++;

        if (CurrentGameSide == PlayerSide)
        {
            _playerTroopsOnField++;
        }
        else
        {
            _enemyTroopsOnField++;
        }

        //Fire deploy event
        OnUnitDeployed?.Invoke(this, EventArgs.Empty);
    }

    private void SetFirstSide()
    {
        int randNum = UnityEngine.Random.Range(0, 100);
        if (randNum < 50)
            _startingSide = GameSides.Flemish;
        else
            _startingSide = GameSides.French;

        _currentGameSide = _startingSide;
    }

    private BattlePhase CheckToSetButtonToEnd()
    {
        if (GetCurrentSideDeckSize() > 0)
            return BattlePhase.MovementAttackPhase;
        else
            return BattlePhase.DeployPhase;
    }

    private int GetCurrentSideTroopsOnField()
    {
        if (CurrentGameSide == PlayerSide)
            return _playerTroopsOnField;
        else
            return _enemyTroopsOnField;
    }

    private int GetCurrentSideDeckSize()
    {
        if (CurrentGameSide == PlayerSide)
        {
            if (!_playerDeck) return -1;
            return _playerDeck.CardsRemaining;
        }
        else
        {
            if (_isAiPlaying) return _aiManager.CardsRemaining;
            else return _enemyDeck.Count;
        }
    }

    public void SwitchSide()
    {
        if (_currentGameSide == _startingSide)
        {
            if (_currentGameSide == GameSides.Flemish)
                _currentGameSide = GameSides.French;
            else
                _currentGameSide = GameSides.Flemish;

            //_currentGameSide = PlayerSide.Equals(GameSides.Flemish) ? GameSides.Flemish : GameSides.French;
        }
        else
        {
            if (_currentGameSide == GameSides.Flemish)
                _currentGameSide = GameSides.French;
            else
                _currentGameSide = GameSides.Flemish;

            //_currentGameSide = PlayerSide.Equals(GameSides.Flemish) ? GameSides.French : GameSides.Flemish;

            if (!_isFirstTurn)
                NextTurn();
        }

        //Go to movement phase
        _currentBattlePhase = BattlePhase.MovementAttackPhase;

        //No longer first turn
        _isFirstTurn = false;

        //Reset the troops deployment 
        _totalTroopsDeployed = 0;

        //Switch side
        OnSwitchSide?.Invoke(this, new EventArgs());
    }

    private void NextTurn()
    {
        OnTurnEnd?.Invoke(this, new EventArgs());
    }

    public void TroopKilled(GameSides troopSide)
    {
        if (troopSide.Equals(PlayerSide))
        {
            --_playerTroopsOnField;
            if (_playerTroopsOnField <= 0 && _playerDeck.CardsRemaining == 0)
                EndGame(PlayerSide.Equals(GameSides.Flemish) ? GameSides.French : GameSides.Flemish);
        }
        else
        {
            int enemyCardsRemaining = (_isAiPlaying ? _aiManager.CardsRemaining : _enemyDeck.Count);

            --_enemyTroopsOnField;
            if (_enemyTroopsOnField <= 0 && enemyCardsRemaining == 0)
                EndGame(PlayerSide.Equals(GameSides.Flemish) ? GameSides.Flemish : GameSides.French);
        }
    }

    public void EndGame(GameSides winningSide)
    {
        if (!_hasGameEnded)
        {
            OnGameEnd?.Invoke(this, new GameSideEventArgs(winningSide));
            _hasGameEnded = true;

            // If this isn't called during the tutorial, set has played game to true
            if (!_tutorial)
                PlayerPrefs.SetInt("HasPlayedGame", 1);
        }
    }

    public void ShowHealthUnits(bool showHealth)
    {
        foreach (var troop in _troops)
        {
            if (troop == null)
                return;

            //Get the hp UI component
            var hp = troop.GetComponentInChildren<UITroopHPManager>();

            if (hp)
            {
                //Show the hp
                if (showHealth)
                    hp.FadeInOnly();
                else hp.FadeOutImmedately();
            }
        }
    }

    public void AddTroop(TroopController troop)
    {
        // Check if the troop isn't in the list already
        if (_troops.Contains(troop)) return;

        _troops.Add(troop);
    }

    public void RemoveTroop(TroopController troop)
    {
        _troops.Remove(troop);
    }

    private void HandlePlayerDeckState(object sender, DataEventArgs e)
    {
        if (!_playerDeck) return;

        // Update deck visibility
        DeckVisibility targetVisibility = DeckVisibility.LowVisible;
        if (CurrentGameSide == PlayerSide && CurrentBattlePhase == BattlePhase.DeployPhase && CanDeployTroop())
        {
            targetVisibility = DeckVisibility.FullVisible;
        }
        SetDeckVisibility(_playerDeck, targetVisibility);

        // Check if there is a card selected
        if (targetVisibility == DeckVisibility.FullVisible)
        {
            _playerDeck.CheckSelectedCard();
        }
    }

    public async void LoadData(LevelPresetData data)
    {
        // Game data
        _isFirstTurn = data.GameLoop.IsFirstTurn;
        _isAiPlaying = data.GameLoop.IsAiPlaying;

        _totalTroopsDeployed = data.GameLoop.TotalTroopsDeployed;
        _deployTroopLimit = data.GameLoop.DeployTroopLimit;

        _playerSide = (GameSides)data.GameLoop.PlayerSide;
        _currentGameSide = (GameSides)data.GameLoop.CurrentGameSide;
        _startingSide = (GameSides)data.GameLoop.StartingSide;

        _currentBattlePhase = (BattlePhase)data.GameLoop.CurrentBattlePhase;

        // Instatiated data
        ResetData();

        await LoadTroops(data.GameLoop.Troops);

        await LoadCards(data.GameLoop.PlayerCards, true);

        await LoadCards(data.GameLoop.EnemyCards, false);

        // Make sure the UI displays the correct info
        OnDataLoaded?.Invoke(this, new DataEventArgs(CurrentBattlePhase));
    }

    private void ResetData()
    {
        // Reset tutorial index
        _currentTutorialIdx = 0;

        // Reset troops data
        foreach (TroopController controller in _troops)
        {
            Destroy(controller.gameObject);
        }
        _troops.Clear();
        _playerTroopsOnField = 0;
        _enemyTroopsOnField = 0;

        // Reset player cards data
        if (_playerDeck) _playerDeck.ClearDeck();

        // Reset ai cards data
        if (_aiManager) _aiManager.ClearDeck();
        
        // Reset enemy cards data
        foreach (TroopCard card in _enemyDeck)
        {
            Destroy(card.gameObject);
        }
        _enemyDeck.RemoveRange(0, _enemyDeck.Count);
        _enemyDeck.Clear();
    }

    private async Task LoadTroops(List<TroopData> troops)
    {
        GridModel gridModel = FindObjectOfType<GridModel>();
        if (!gridModel) return;

        foreach (TroopData troopData in troops)
        {
            // Check if the troopData is valid
            if (troopData.CurrentHexagonCoordinates.Count < 2) continue;

            // Load the troop prefab we need to spawn
            GameObject troopObj = await LoadPrefabAsync(troopData.AssetKey);
            if (!troopObj) continue;

            // Find the tile the troop has to spawn on
            Vector2 doubleCoordinates = new Vector2(troopData.CurrentHexagonCoordinates[0], troopData.CurrentHexagonCoordinates[1]);
            HexagonModel spawnTile = gridModel.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates));
            if (!spawnTile) continue;

            // Spawn the troop and check if it's a valid troop
            GameObject instantiatedTroopObj = Instantiate(troopObj, spawnTile.transform.position, Quaternion.identity);

            TroopController troopController = instantiatedTroopObj.GetComponent<TroopController>();
            TroopModel troopModel = instantiatedTroopObj.GetComponent<TroopModel>();
            if (!troopController || !troopModel)
            {
                Destroy(instantiatedTroopObj);
                continue;
            }

            // Set parent for spawned troop
            instantiatedTroopObj.transform.parent = _troopsParentSocket;

            // Load the spawned troop's data
            troopModel.LoadData(troopData);

            // Set and update the tutorial index
            troopModel.TutorialIdx = _currentTutorialIdx;
            _currentTutorialIdx++;

            // Let troop and tile know of each other
            troopModel.NewTile(spawnTile);

            // Update amount of troops on field
            if (troopModel.TroopData.Side == _playerSide)
            {
                _playerTroopsOnField++;
            }
            else
            {
                _enemyTroopsOnField++;
            }

            // Add the troop to the list
            AddTroop(troopController);
        }
    }

    private async Task LoadCards(List<CardData> cards, bool arePlayerCards)
    {
        foreach (CardData cardData in cards)
        {
            // Load the card prefab we need to spawn
            GameObject cardObj = await LoadPrefabAsync(cardData.AssetKey);
            if (!cardObj) continue;

            // Spawn the card and add it to the correct deck
            GameObject instantiatedCardObj = null;
            if (arePlayerCards)
            {
                if (_playerDeck) instantiatedCardObj = _playerDeck.AddCard(cardObj);
            }
            else
            {
                if (_isAiPlaying)
                {
                    instantiatedCardObj = _aiManager.AddCard(cardObj);
                }
                else
                {
                    instantiatedCardObj = AddEnemyCard(cardObj);
                }
            }

            if (!instantiatedCardObj) continue;
            
            TroopCard card = instantiatedCardObj.GetComponent<TroopCard>();
            if (!card) continue;

            card.LoadData(cardData);

            // Set and update the tutorial index
            card.TutorialIdx = _currentTutorialIdx;
            _currentTutorialIdx++;
        }
    }

    private GameObject AddEnemyCard(GameObject cardToAdd)
    {
        if (!cardToAdd || !_enemyDeckParentSocket) return null;

        GameObject card = Instantiate(cardToAdd);

        // Check if the item we are adding is a card
        TroopCard troopCard = card.GetComponent<TroopCard>();
        if (!troopCard)
        {
            Destroy(card);
            return null;
        }

        // Parent the card to the right position
        troopCard.transform.position = _enemyDeckParentSocket.position;

        // Set parent
        troopCard.transform.parent = _enemyDeckParentSocket;

        // Set the parent socket for the troop that will be spawned
        troopCard.TroopParentSocket = _troopsParentSocket;

        // Add the card to the list
        _enemyDeck.Add(troopCard);

        return card;
    }

    private async Task<GameObject> LoadPrefabAsync(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load prefab at address '{address}': {handle.DebugName}");
            return null;
        }
    }

    public void SaveData(ref LevelPresetData data)
    {
        data.GameLoop.IsFirstTurn = _isFirstTurn;
        data.GameLoop.IsAiPlaying = _isAiPlaying;

        data.GameLoop.TotalTroopsDeployed = _totalTroopsDeployed;
        data.GameLoop.DeployTroopLimit = _deployTroopLimit;

        data.GameLoop.PlayerSide = (int)_playerSide;
        data.GameLoop.CurrentGameSide = (int)_currentGameSide;
        data.GameLoop.StartingSide = (int)_startingSide;

        data.GameLoop.CurrentBattlePhase = (int)_currentBattlePhase;

        data.GameLoop.Troops.Clear();
        List<IDataPersistance<TroopData>> troopObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<TroopData>>().ToList();
        foreach (IDataPersistance<TroopData> troopObj in troopObjects)
        {
            TroopData troopData = new TroopData();
            troopObj.SaveData(ref troopData);

            data.GameLoop.Troops.Add(troopData);
        }

        data.GameLoop.PlayerCards.Clear();
        data.GameLoop.EnemyCards.Clear();
        List<IDataPersistance<CardData>> cardObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<CardData>>().ToList();
        foreach (IDataPersistance<CardData> cardObj in cardObjects)
        {
            CardData cardData = new CardData();
            cardObj.SaveData(ref cardData);

            if ((GameSides)cardData.CardSide == _playerSide)
                data.GameLoop.PlayerCards.Add(cardData);
            else
                data.GameLoop.EnemyCards.Add(cardData);
        }
    }
}
