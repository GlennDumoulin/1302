using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;
using System.Linq;
using Random = UnityEngine.Random;

public class AIManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [SerializeField] private GameObject _frenchDecks = null;
    [SerializeField] private GameObject _flemishDecks = null;
    [SerializeField] private GameObject _deckPosition = null;

    private GameObject _deckPool = null;

    [SerializeField] private Transform _troopParentSocket = null;

    [Header("AI Settings")]
    [SerializeField] private bool _isAiEnabled = true;
    public bool IsAiEnabled { get { return _isAiEnabled; } }
    [SerializeField] private AiSettings _aiSettings = null;

    private bool _canAIAct = true;

    private GameLoopManager _gameLoopManager = null;
    private GridController _gridController = null;
    private GridModel _gridModel = null;
    private RainMudManager _rainMudManager = null;
    private CampfireManager _campfireManager = null;

    private List<TroopCard> _deck = new List<TroopCard>();
    private GameSides _aiSide = GameSides.French;
    public int CardsRemaining
    {
        get { return _deck.Count; }
    }

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _gridController = FindObjectOfType<GridController>();
        _gridModel = FindObjectOfType<GridModel>();
        _rainMudManager = FindAnyObjectByType<RainMudManager>();
        _campfireManager = FindAnyObjectByType<CampfireManager>();

        _aiSide = StaticDataHelper.aiSide;

        var deckObj = StaticDataHelper.aiDeck;
        var cards = deckObj.Deck;

        foreach (GameObject card in cards)
            AddCard(card);
    }

    private void Start()
    {
        if (!_gameLoopManager) return;

        if (_isAiEnabled && _gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
            StartTurn(this, EventArgs.Empty);
    }

    private void OnEnable()
    {
        if (!_gameLoopManager || !_isAiEnabled) return;

        _gameLoopManager.OnPhaseAdvanced += StartTurn;
        _gameLoopManager.OnGameEnd += StopAI;
    }

    private void OnDisable()
    {
        if (!_gameLoopManager || !_isAiEnabled) return;

        _gameLoopManager.OnPhaseAdvanced -= StartTurn;
        _gameLoopManager.OnGameEnd -= StopAI;
    }

    private float GetRandomValue01()
    {
        return Random.Range(0.0f, 1.0f);
    }

    private struct WeightedTile
    {
        public HexagonModel Tile;
        public int Weight;
    }
    private int GetWeightedRandomTileIdx(List<WeightedTile> tiles)
    {
        if (tiles.Count <= 0) return -1;

        // Get the total weight of the given tiles
        int totalWeight = 0;
        foreach (WeightedTile tile in tiles)
        {
            totalWeight += tile.Weight;
        }

        // Get a random value
        int randomValue = Random.Range(0, totalWeight);

        // Get a random tile based on the tile's weight
        for (int i = 0; i < tiles.Count; ++i)
        {
            WeightedTile tile = tiles.ElementAt(i);

            if (randomValue <= tile.Weight) return i;

            randomValue -= tile.Weight;
        }

        // Take first index as a fallback option
        return 0;
    }

    private void StartTurn(object sender, EventArgs e)
    {
        if (!_gameLoopManager) return;
        if (_gameLoopManager.CurrentGameSide == _gameLoopManager.PlayerSide) return;
        if (!_canAIAct) return;

        // Handle the phase we start in
        if (_gameLoopManager.CurrentBattlePhase == BattlePhase.MovementAttackPhase)
        {
            HandleMovementAttackPhase();
        }
        else
        {
            StartCoroutine(HandleDeployPhase());
        }
    }

    private void StopAI(object sender, EventArgs e)
    {
        _canAIAct = false;
    }

    private void SetGameSpeed(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    private void HandleMovementAttackPhase()
    {
        if (!_gameLoopManager || !_gridController || !_gridModel) return;

        // Get all troops on the battlefield
        List<TroopController> troops = _gameLoopManager.Troops;

        // Get all AI troops from the list
        List<TroopController> aiTroops = troops.Where(t =>
        {
            TroopModel model = t.GetComponent<TroopModel>();
            if (!model) return false;

            return model.TroopData.Side != _gameLoopManager.PlayerSide;
        }
        ).ToList();

        // Shuffle the list of troops
        aiTroops.Sort((a, b) => Random.Range(-1, 1));

        // Get all Player troops
        List<TroopController> playerTroops = troops.Where(t => !aiTroops.Contains(t)).ToList();

        // Start handling all troops
        StartCoroutine(MoveAttackTroops(aiTroops, playerTroops));
    }

    private IEnumerator MoveAttackTroops(List<TroopController> aiTroops, List<TroopController> playerTroops, int currentIdx = 0)
    {
        // If the game has ended, stop the AI from acting further
        if (!_canAIAct) yield break;

        // Check if we have handled all troops
        if (currentIdx >= aiTroops.Count)
        {
            // Start the deploy phase
            StartCoroutine(HandleDeployPhase());

            yield break;
        }

        // Wait a bit before handling the first troop
        if (currentIdx == 0) yield return new WaitForSeconds(1.0f);

        int distanceToTarget = MoveAttackTroop(aiTroops.ElementAt(currentIdx), playerTroops);

        // Handle moving the next troop
        if (distanceToTarget != -1)
            yield return new WaitForSeconds(Mathf.Lerp(1.0f, 2.5f, distanceToTarget / 10.0f));
        StartCoroutine(MoveAttackTroops(aiTroops, playerTroops, ++currentIdx));
    }

    private int MoveAttackTroop(TroopController troop, List<TroopController> playerTroops)
    {
        if (!troop) return -1;

        TroopModel troopModel = troop.GetComponent<TroopModel>();
        if (!troopModel) return -1;

        bool isFleeingFromCampfire = false;

        // Check if the troop is on a campfire
        if (troopModel.CurrentHexagonModel.Campfire)
        {
            // Check if we should flee from the campfire
            isFleeingFromCampfire = HandleTroopOnCampfire(troopModel, playerTroops);

            // If not, stay on the campfire
            if (!isFleeingFromCampfire) return -1;
        }

        // Check if the troop has to reload and isn't fleeing a campfire
        if ((troopModel.ShouldReload || troopModel.ReloadThisTurn) && !isFleeingFromCampfire)
        {
            // Consider if we want to reload
            if (ShouldConsiderReloading(troopModel)) return -1;
        }
        else
        {
            // Get attack tiles for the current troop
            List<HexagonModel> attackTiles = _gridController.GetAttackTiles(_gridModel.HexagonModels, troopModel.CurrentHexagonModel, troopModel.TroopData);
            
            // Try avoiding anti-horses if the attacking troop is a knight
            if (troopModel.TroopData.IsDamageReflectable)
            {
                TryAvoidAntiHorses(attackTiles);
            }

            if (attackTiles.Count > 0)
            {
                // Get a random tile
                int tileIdx = Random.Range(0, attackTiles.Count);

                HexagonModel tile = attackTiles.ElementAt(tileIdx);
                if (!tile) return -1;

                // Attack the chosen tile
                if (troop.TryToActWithTroop(null, tile.Troop))
                {
                    bool isRanged = (troopModel.TroopData.SpecialCharacteristic == TroopSpecialCharacteristics.UseRadius || troopModel.TroopData.SpecialCharacteristic == TroopSpecialCharacteristics.CrossbowFireReload);

                    int distanceToTarget = Mathf.RoundToInt(_gridController.Distance(troopModel.CurrentHexagonModel, tile) / (isRanged ? 1.5f : 1.0f));
                    return distanceToTarget;
                }
            }
        }

        // Check if the troop is stuck in mud
        if (!troopModel.CanMove) return -1;

        // Get movement tiles for the current troop
        List<HexagonModel> movementTiles = _gridController.GetMovementTiles(_gridModel.HexagonModels, troopModel.CurrentHexagonModel, troopModel.TroopData);

        // Try avoiding mud if it's raining
        if (_rainMudManager)
        {
            if (_rainMudManager.IsRaining) TryAvoidMud(movementTiles, troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge));
        }

        // Check if we should move towards a campfire
        List<WeightedTile> movingToCampfireTiles = new List<WeightedTile>();
        if (!isFleeingFromCampfire && ShouldMoveToCampfire())
        {
            movingToCampfireTiles = HandleMoveToCampfire(movementTiles, troopModel);
        }

        if (movementTiles.Count > 0)
        {
            int tileIdx = -1;
            if (movingToCampfireTiles.Count > 0)
            {
                // Get a weighted random tile based on distance
                tileIdx = GetWeightedRandomTileIdx(movingToCampfireTiles);
            }
            else
            {
                // Get a random tile
                tileIdx = Random.Range(0, movementTiles.Count);
            }

            if (tileIdx == -1) return -1;

            HexagonModel tile = movementTiles.ElementAt(tileIdx);
            if (!tile) return -1;

            // Move to the chosen tile
            troop.TryToActWithTroop(tile, null);

            return _gridController.Distance(troopModel.CurrentHexagonModel, tile);
        }

        return -1;
    }

    private bool HandleTroopOnCampfire(TroopModel troopModel, List<TroopController> playerTroops)
    {
        if (!_gridController || !_gridModel) return false;

        if (playerTroops.Count <= 0) return false;

        // Check if the troop can get damaged/killed
        int remainingHP = troopModel.HP;

        foreach (TroopController playerTroop in playerTroops)
        {
            // Check if the troop still exists
            if (!playerTroop) continue;

            TroopModel playerModel = playerTroop.GetComponent<TroopModel>();
            if (!playerModel) continue;

            List<HexagonModel> attackTiles = _gridController.GetAttackTiles(_gridModel.HexagonModels, playerModel.CurrentHexagonModel, playerModel.TroopData);
            if (attackTiles.Contains(troopModel.CurrentHexagonModel))
                remainingHP -= playerModel.TroopData.Damage;
        }

        bool shouldFlee = false;
        if (remainingHP <= 0)
        {
            // Troop can get killed
            shouldFlee = ShouldFleeCampfire(true);
        }
        else if (remainingHP < troopModel.HP)
        {
            // Troop can get damaged
            shouldFlee = ShouldFleeCampfire(false);
        }

        return shouldFlee;
    }

    private List<WeightedTile> HandleMoveToCampfire(List<HexagonModel> movementTiles, TroopModel troop)
    {
        List<WeightedTile> weightedTiles = new List<WeightedTile>();

        if (!_campfireManager || !_gameLoopManager) return weightedTiles;

        // Get all campfire tiles
        List<HexagonModel> campfireTiles = new List<HexagonModel>();
        foreach (CampfireModel campfire in _campfireManager.CampfireModel)
        {
            if (!campfire) continue;

            // Check if there is a troop on the campfire
            TroopModel campfireTroop = campfire.CurrentDefender;
            if (campfireTroop)
            {
                // Ignore this campfire if the troop is ours
                if (campfireTroop.TroopData.Side != _gameLoopManager.PlayerSide) continue;
            }

            campfireTiles.Add(campfire.CurrentHexagonModel);
        }

        if (campfireTiles.Count <= 0) return weightedTiles;

        // Check if there is a campfire in the movement tiles
        List<HexagonModel> reachableCampfireTiles = movementTiles.Intersect(campfireTiles).ToList();
        if (reachableCampfireTiles.Count > 0)
        {
            // Set the reachable campfires as the only movement tiles
            movementTiles.Clear();
            movementTiles.AddRange(reachableCampfireTiles);
        }
        else
        {
            // Get the current distance to the closest campfire
            int currentDistanceToCampfire = DistanceToClosestCampfire(troop.CurrentHexagonModel, campfireTiles);

            // Remove all tiles that move away from the campfires
            movementTiles.RemoveAll(t =>
            {
                int distance = DistanceToClosestCampfire(t, campfireTiles);
                const int maxWeight = 10;

                if (distance < currentDistanceToCampfire)
                {
                    WeightedTile weightedTile = new WeightedTile();
                    weightedTile.Tile = t;
                    weightedTile.Weight = Mathf.RoundToInt(Mathf.Lerp(maxWeight, 0, distance / 10.0f));

                    weightedTiles.Add(weightedTile);

                    return false;
                }

                return true;
            }
            );
        }

        return weightedTiles;
    }

    private int DistanceToClosestCampfire(HexagonModel tile, List<HexagonModel> campfireTiles)
    {
        int distanceToClosestCampfire = int.MaxValue;

        if (!_gridController) return distanceToClosestCampfire;

        // Find the distance to the closest campfire
        foreach (HexagonModel campfire in campfireTiles)
        {
            int distanceToCampfire = _gridController.Distance(tile, campfire);

            if (distanceToCampfire < distanceToClosestCampfire)
                distanceToClosestCampfire = distanceToCampfire;
        }

        return distanceToClosestCampfire;
    }

    #region ChanceFunctionality
    private bool ShouldConsiderReloading(TroopModel troopModel)
    {
        if (!_aiSettings) return false;

        if (troopModel.ShouldReload || troopModel.ReloadThisTurn)
            return GetRandomValue01() <= _aiSettings.ReloadChance;

        return false;
    }

    private bool ShouldFleeCampfire(bool canDie)
    {
        if (!_aiSettings) return false;

        float fleeChance = canDie ? _aiSettings.FleeKillableCampfireChance : _aiSettings.FleeDamageableCampfireChance;
        return GetRandomValue01() <= fleeChance;
    }

    private bool ShouldMoveToCampfire()
    {
        if (!_aiSettings) return false;

        return GetRandomValue01() <= _aiSettings.MoveToCampfireChance;
    }

    private void TryAvoidAntiHorses(List<HexagonModel> attackTiles)
    {
        if (!_aiSettings || attackTiles.Count <= 0) return;

        if (GetRandomValue01() <= _aiSettings.AvoidAntiHorseChance)
        {
            // Remove all attack tiles containing an anti-horse
            attackTiles.RemoveAll(tile => tile.Troop.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.DamageReflect));
        }
    }

    private void TryAvoidMud(List<HexagonModel> movementTiles, bool isChargingUnit)
    {
        if (!_aiSettings || movementTiles.Count <= 0) return;

        float avoidChance = isChargingUnit ? _aiSettings.AvoidMudChanceCharging : _aiSettings.AvoidMudChanceDefault;
        if (GetRandomValue01() <= avoidChance)
        {
            // Remove all attack tiles containing an anti-horse
            movementTiles.RemoveAll(tile => tile.IsMud);
        }
    }
    #endregion

    private IEnumerator HandleDeployPhase()
    {
        if (!_gameLoopManager) yield break;

        // If the game has ended, stop the AI from acting further
        if (!_canAIAct) yield break;

        yield return new WaitForSeconds(0.5f);

        CheckDeployTroop(false);
    }

    private void DeployTroop()
    {
        if (_deck.Count <= 0)
        {
            StartCoroutine(EndTurn());
            return;
        }

        // Get a random card from the deck
        int cardIdx = Random.Range(0, _deck.Count);

        TroopCard card = _deck.ElementAt(cardIdx);
        if (!card)
        {
            CheckDeployTroop(false);
            return;
        }

        // Get all valid deployment tiles
        List<HexagonModel> deployTiles = card.GetValidDeployTiles();
        if (deployTiles.Count > 0)
        {
            // Get a random tile from the valid deploy tiles
            int tileIdx = Random.Range(0, deployTiles.Count);

            HexagonModel tile = deployTiles.ElementAt(tileIdx);
            if (!tile)
            {
                CheckDeployTroop(false);
                return;
            }

            // Try to spawn the troop
            if (card.TrySpawnTroop(tile, true))
            {
                _gameLoopManager.UpdateDeployCount();
                _deck.RemoveAt(cardIdx);
            }

            CheckDeployTroop();
        }
    }

    private void CheckDeployTroop(bool spawnedTroop = true)
    {
        // Check if we can spawn another troop
        if (_gameLoopManager.CanDeployTroop() && _deck.Count > 0) Invoke("DeployTroop", spawnedTroop ? 0.5f : 0.0f);
        else StartCoroutine(EndTurn());
    }

    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(0.75f);

        _gameLoopManager.TurnEnd();
    }

    private void PickRandomDeck()
    {
        if (_aiSide == GameSides.French)
            _deckPool = _frenchDecks;
        else
            _deckPool = _flemishDecks;


        //Get the amount of decks
        int nmbrOfDecks = _deckPool.transform.childCount;

        //Get the idx of the random deck
        int idx = Random.Range(0, nmbrOfDecks);

        //Get the transform of the random deck
        var deckTrans = _deckPool.transform.GetChild(idx);

        //Get the gameobject list
        GameObject deck = deckTrans.gameObject;

        //Get the nmbrOfCards
        int nmbrOfCards = deck.transform.childCount;

        //Go over all the cards
        for(int i = 0; i < nmbrOfCards; i++) 
        {
            AddCard(deck.transform.GetChild(i).gameObject);
        }
    }

    public void ClearDeck()
    {
        foreach (TroopCard card in _deck)
        {
            Destroy(card.gameObject);
        }
        _deck.RemoveRange(0, _deck.Count);
        _deck.Clear();
    }

    public GameObject AddCard(GameObject cardToAdd)
    {
        if (!cardToAdd) return null;

        // Create an instance of the card
        GameObject card = Instantiate(cardToAdd);

        // Check if the item we are adding is a card
        TroopCard troopCard = card.GetComponent<TroopCard>();
        if (!troopCard)
        {
            Destroy(card);
            return null;
        }

        // Parent the card to the right position
        card.transform.position = _deckPosition.transform.position;

        // Set parent
        card.transform.parent = _deckPosition.transform;

        // Add the card to the deck
        _deck.Add(troopCard);

        // Set the parent socket for the troop card
        troopCard.TroopParentSocket = _troopParentSocket;

        return card;
    }
}
