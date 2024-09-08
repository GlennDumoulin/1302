using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameEnum;
using System.Linq;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class TutorialLevelResources
{
    public string LevelPresetPath = string.Empty;
    public string ScriptPath = string.Empty;
    public string StringTableName = string.Empty;
}

public class InstructionInfo
{
    public Vector2 Pivot = Vector2.zero;
    public Vector3 Offset = Vector3.zero;

    public InstructionInfo(Vector2 pivot, Vector3 offset)
    {
        Pivot = pivot;
        Offset = offset;
    }
}

[RequireComponent(typeof(LevelPresetDataManager))]
public class TutorialScriptHandler : MonoBehaviour, IDataPersistance<TutorialScriptData>
{
    [SerializeField] private TutorialScriptData TutorialScript;
    [SerializeField] private TextMeshProUGUI _text = null;
    [SerializeField] private Image _image = null;

    private readonly Dictionary<int, InstructionInfo> _instructionInfos = new Dictionary<int, InstructionInfo>()
    {
        { 0, new InstructionInfo(new Vector2(0.5f, 0.0f), new Vector3(0.0f, 3.0f, 0.0f)) },
        { 1, new InstructionInfo(new Vector2(0.0f, 0.0f), new Vector3(2.0f, 3.0f, 0.0f)) },
        { 2, new InstructionInfo(new Vector2(0.0f, 0.5f), new Vector3(2.0f, 0.0f, 0.0f)) },
        { 3, new InstructionInfo(new Vector2(0.0f, 1.0f), new Vector3(2.0f, -3.0f, 0.0f)) },
        { 4, new InstructionInfo(new Vector2(0.5f, 1.0f), new Vector3(0.0f, -3.0f, 0.0f)) },
        { 5, new InstructionInfo(new Vector2(1.0f, 1.0f), new Vector3(-2.0f, -3.0f, 0.0f)) },
        { 6, new InstructionInfo(new Vector2(1.0f, 0.5f), new Vector3(-2.0f, 0.0f, 0.0f)) },
        { 7, new InstructionInfo(new Vector2(1.0f, 0.0f), new Vector3(-2.0f, 3.0f, 0.0f)) },
        { 8, new InstructionInfo(new Vector2(0.5f, 0.0f), new Vector3(0.0f, 7.0f, 0.0f)) },
    };

    private GridModel _grid = null;
    private GameLoopManager _gameLoopManager = null;
    private UILocalization _uiLocalization = null;

    private LevelPresetDataManager _levelPresetDataManager = null;
    private TutorialDataManager _tutorialDataManager = null;
    private UIDeckbuildingInfo _campfireInfo = null;

    private TroopAction _action = null;
    public TroopAction Action
    {
        get { return _action; }
    }

    private DeploymentData _deployment = null;

    public DeploymentData Deployment
    {
        get { return _deployment; }
    }

    private HexagonModel _selectedHexagon = null;
    public HexagonModel SelectedHexagon
    {
        get { return _selectedHexagon; }
        set 
        {
            _selectedHexagon = value;
        }
    }

    private TroopModel _selectedTroop = null;
    public TroopModel SelectedTroop
    {
        get { return _selectedTroop; }
        set { _selectedTroop = value; }
    }

    private TroopController _troopTarget = null;
    public TroopController TroopTarget
    {
        get { return _troopTarget; }
    }

    private TroopController _enemyTroop = null;
    public TroopController EnemyTroop
    {
        get { return _enemyTroop; }
        set { _enemyTroop = value; }
    }

    private HexagonModel _targetHexagon = null;
    public HexagonModel TargetHexagon
    {
        get { return _targetHexagon; }
    }

    private TroopCard _targetCard = null;
    public TroopCard TargetCard
    {
        get { return _targetCard; }
        set { _targetCard = value; }
    }

    private TroopCard _selectedCard = null;
    public TroopCard SelectedCard
    {
        get { return _selectedCard; }
        set { _selectedCard = value; }
    }
    
    private TutorialTextManager _tutorialText = null;

    private StringTable _stringTable = null;
    private string _currentKey = string.Empty;

    public static int TutorialLevelIdx = 0;
    [SerializeField] private List<TutorialLevelResources> _tutorialLevels = new List<TutorialLevelResources>();

    private bool _isHandlingSide = false;
    private const float _timeBeforeEndScript = 5.0f;
    private float _startingTime = 0.0f;

    public UnityEvent OnButtonEnable = null;
    public UnityEvent OnButtonDisable = null;

    private void Awake()
    {
        // Make sure we are trying to load a tutorial level that actually exists
        // If not, go back to the main menu
        if (TutorialLevelIdx >= _tutorialLevels.Count)
        {
            AudioManager.instance.PlayMenuMusic();
            SceneTransitionManager.instance.TransitionToMain();
            return;
        }

        _grid = FindObjectOfType<GridModel>();
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _uiLocalization = FindObjectOfType<UILocalization>();
        _campfireInfo = FindObjectOfType<UIDeckbuildingInfo>();

        _levelPresetDataManager = GetComponent<LevelPresetDataManager>();
        _tutorialDataManager = GetComponent<TutorialDataManager>();

        // Get the tutorial text
        _tutorialText = FindAnyObjectByType<TutorialTextManager>();

        // Get the stringTable we use for the localization strings
        UpdateLocale(this, EventArgs.Empty);

        // Reset the tutorial text
        ResetInstructionText();
    }

    private void OnEnable()
    {
        if (_grid) _grid.OnGridCreated += StartData;
        if (_uiLocalization) _uiLocalization.OnLocaleChanged += UpdateLocale;
    }

    private void OnDisable()
    {
        if (_grid) _grid.OnGridCreated -= StartData;
        if (_uiLocalization) _uiLocalization.OnLocaleChanged -= UpdateLocale;
    }

    private void UpdateLocale(object sender, EventArgs e)
    {
        // Get the stringTable we use for the localization strings
        StartCoroutine("GetStringTable");
    }

    private bool IsLocalizationInitDone()
    {
        return LocalizationSettings.InitializationOperation.IsDone;
    }

    private IEnumerator GetStringTable()
    {
        yield return new WaitUntil(IsLocalizationInitDone);

        TutorialLevelResources resources = _tutorialLevels.ElementAt(TutorialLevelIdx);
        if (resources.StringTableName == null) yield break;

        var stringTables = LocalizationSettings.StringDatabase.GetAllTables().WaitForCompletion();
        foreach (var stringTable in stringTables)
        {
            if (stringTable.TableCollectionName == resources.StringTableName)
            {
                _stringTable = stringTable;
                break;
            }
        }

        LocalizeInstructionText();
    }

    public void LoadData(TutorialScriptData data)
    {
        TutorialScript = data;
    }

    public void SaveData(ref TutorialScriptData data)
    {
        data = TutorialScript;
    }

    public void LoadNextLevel()
    {
        TutorialLevelIdx++;

        if (TutorialLevelIdx < _tutorialLevels.Count)
        {
            AudioManager.instance.PlayGameMusic();
            SceneTransitionManager.instance.TransitionToTutorial();
        }
        else
        {

            _campfireInfo.ShowInfoScreen();

            //PlayerPrefs.SetInt("HasPlayedTutorial", 1);
            //AudioManager.instance.PlayMenuMusic();
            //SceneTransitionManager.instance.TransitionToMain();
        }
    }

    private void StartData(object sender, EventArgs e)
    {
        TutorialLevelResources resources = _tutorialLevels.ElementAt(TutorialLevelIdx);

        if (resources.LevelPresetPath == string.Empty || resources.ScriptPath == string.Empty)
        {
            AudioManager.instance.PlayMenuMusic();
            SceneTransitionManager.instance.TransitionToMain();
        }

        LoadTutorialScript(resources.ScriptPath);
        LoadLevelPreset(resources.LevelPresetPath);

        StartCoroutine(ExecuteData());
    }

    private void LoadTutorialScript(string scriptPath)
    {
        if (scriptPath != string.Empty && _tutorialDataManager)
            _tutorialDataManager.LoadResource(scriptPath);
    }

    private void LoadLevelPreset(string levelPresetPath)
    {
        if (levelPresetPath != string.Empty && _levelPresetDataManager)
            _levelPresetDataManager.LoadResource(levelPresetPath);
    }

    private bool IsHandlingSide()
    {
        return _isHandlingSide;
    }

    private bool IsEnemyTurn()
    {
        return !_gameLoopManager.CurrentGameSide.Equals(_gameLoopManager.PlayerSide);
    }

    private bool IsDeployPhase()
    {
        return _gameLoopManager.CurrentBattlePhase.Equals(BattlePhase.DeployPhase);
    }

    private bool IsScriptHandled()
    {
        List<TroopController> troops = _gameLoopManager.Troops;

        // Remove all player troops
        troops.RemoveAll(t => t.GetComponent<TroopModel>().TroopData.Side.Equals(_gameLoopManager.PlayerSide));

        return troops.Count <= 0 || Time.time - _startingTime >= _timeBeforeEndScript;
    }

    private IEnumerator ExecuteData()
    {
        if (!_gameLoopManager) yield break;

        yield return new WaitForSeconds(1);

        // Handle every turn
        foreach (TurnData turn in TutorialScript.Turns)
        {
            // Get all the cards
            List<TroopCard> cards = FindObjectsOfType<TroopCard>().ToList();

            // Handle all actions for the flemish side (always player in tutorial)
            StartCoroutine(HandleSide(turn.FlemishSide, true, cards));

            yield return new WaitWhile(IsHandlingSide);

            if (turn.FrenchSide.Actions.Count > 0 || turn.FrenchSide.Deployments.Count > 0)
            {
                yield return new WaitUntil(IsEnemyTurn);

                // Handle all actions for the french side (always enemy in tutorial)
                StartCoroutine(HandleSide(turn.FrenchSide, false, cards));

                yield return new WaitWhile(IsHandlingSide);
            }
        }

        _startingTime = Time.time;
        yield return new WaitUntil(IsScriptHandled);

        // Load the next level if all turns have been finished
        Invoke("HandleScriptEnded", 1.0f);
    }

    private IEnumerator HandleSide(SideData side, bool isFlemish, List<TroopCard> cards)
    {
        OnButtonDisable?.Invoke();

        _isHandlingSide = true;

        List<TroopAction> actions = side.Actions;
        List<DeploymentData> deployments = side.Deployments;

        bool hadActions = actions.Count > 0;

        // Handle move/attack actions
        while (actions.Count > 0)
        {
            // Reset current action
            _action = null;

            //The "new" action
            _action = actions[0];

            if (_action == null) continue;

            TroopController troopTarget = _gameLoopManager.Troops.Where(t => t.GetComponent<TroopModel>().TutorialIdx == _action.TroopIdx).FirstOrDefault();
            if (troopTarget == null) yield break;

            _troopTarget = troopTarget;

            // Handle showing text if the action is instruction only
            if (_action.IsInstructionOnly)
            {
                yield return new WaitForSeconds(0.5f);

                // Show the instruction
                if (SetInstructionText(_action.Instruction, _troopTarget.transform.position, _action.InstructionInfo))
                {
                    yield return new WaitForSeconds(Mathf.Lerp(2.0f, 8.0f, _text.text.Length / 200.0f));

                    // Reset the text
                    ResetInstructionText();
                }

                //Remove the action
                if (actions.Contains(_action))
                    actions.Remove(_action);

                continue;
            }

            //Convert the target list to a vector 2
            Vector2 targetLoc = new Vector2(_action.TargetPos[0], _action.TargetPos[1]);

            //Get the tile with those coordinates
            _targetHexagon = _grid.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(targetLoc));

            //Check if it needs to happen automatically 
            if (!_action.IsPlayerAction)
            {
                //Show the instruction
                if (SetInstructionText(_action.Instruction, _troopTarget.transform.position, _action.InstructionInfo))
                {
                    yield return new WaitForSeconds(1.0f);
                }

                _troopTarget.TryToActWithTroop(_targetHexagon, _targetHexagon.Troop, true);

                //Remove the action
                if (actions.Contains(_action))
                    actions.Remove(_action);

                yield return new WaitForSeconds(0.75f);
            }
            else
            {
                var troopModel = _troopTarget.GetComponent<TroopModel>();

                //Check if the troop is selected
                if (_selectedTroop && _selectedTroop == troopModel)
                {
                    //Show the instruction
                    SetInstructionText(_action.Instruction, _targetHexagon.transform.position, _action.InstructionInfo);

                    //Show the tutorial attack tile when there is an enemy on there
                    if (_targetHexagon.Troop && !_targetHexagon.Troop.TroopData.Side.Equals(_selectedTroop.GetComponent<TroopModel>().TroopData.Side))
                        _targetHexagon.GetComponent<HexagonView>().HighlightTutorialAttackTile();

                    //Show the campfire tutorial highlight when there is a campfire on there
                    else if (_targetHexagon.Campfire) _targetHexagon.GetComponent<HexagonView>().HighlightTutorialcampfireTile();

                    //Highlight the tile to move to
                    else _targetHexagon.GetComponent<HexagonView>().HighlightTutorialTile();

                    //UnHighlight the previous tile 
                    troopModel.CurrentHexagonModel.GetComponent<HexagonView>().UnHighlightTile();

                    //Check if the player clicked the right tile
                    if (_selectedHexagon == _targetHexagon || (_targetHexagon.Troop && _enemyTroop && _enemyTroop.GetComponent<TroopModel>() == _targetHexagon.Troop))
                    {
                        _troopTarget.TryToActWithTroop(_targetHexagon, _targetHexagon.Troop, true);

                        //Unghighlight the attack tile
                        _selectedTroop.CurrentHexagonModel.GetComponent<HexagonView>().UnHighlightTile();

                        if (_enemyTroop)
                        {
                            //Unselect the troop
                            _enemyTroop.GetComponent<TroopController>().UnselectTroop();
                            _enemyTroop = null;
                        }

                        //Reset the selected things
                        _selectedHexagon = null;
                        _selectedTroop = null;

                        //Reset the text
                        ResetInstructionText();

                        //Unhighlight tile
                        _targetHexagon.GetComponent<HexagonView>().UnHighlightTile();

                        //Remove the action
                        if (actions.Contains(_action))
                            actions.Remove(_action);

                        _action = null;

                        yield return null;
                    }
                    else yield return null;
                }
                else
                {
                    //Highlight the troops tile to select
                    troopModel.CurrentHexagonModel.GetComponent<HexagonView>().HighlightTutorialTile();

                    yield return null;
                }
            }
        }

        // Highlight the move to deploy button (player)
        if (_tutorialText && hadActions && deployments.Count > 0)
        {
            if (isFlemish)
            {
                OnButtonEnable?.Invoke();

                _tutorialText.DisplayMoveToDeploy();

                // Wait until we are in the deploy phase
                yield return new WaitUntil(IsDeployPhase);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        OnButtonDisable?.Invoke();

        // Handle deployment actions
        while (deployments.Count > 0)
        {
            //Reset the deployment
            _deployment = null;

            //The "new" deployment
            _deployment = deployments[0];

            if (_deployment == null) continue;

            //Get the right card 
            TroopCard troopCard = cards.Where(t => t.TutorialIdx == _deployment.CardIdx).FirstOrDefault();
            if (troopCard == null) yield break;

            _targetCard = troopCard;

            // Hanlde showing text if the deployment is instruction only
            if (_deployment.IsInstructionOnly)
            {
                yield return new WaitForSeconds(0.5f);

                // Show the instruction
                if (SetInstructionText(_deployment.Instruction, _targetCard.transform.position))
                {
                    yield return new WaitForSeconds(Mathf.Lerp(2.0f, 6.0f, _text.text.Length / 150.0f));

                    // Reset the text
                    ResetInstructionText();
                }

                //Remove the deployment
                if (deployments.Contains(_deployment))
                    deployments.Remove(_deployment);

                continue;
            }

            //Convert the target list to a vector 2
            Vector2 targetLoc = new Vector2(_deployment.TargetPos[0], _deployment.TargetPos[1]);

            //Get the tile with those coordinates
            _targetHexagon = _grid.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(targetLoc));

            //If it is not a player action (enemy)
            if (!_deployment.IsPlayerAction)
            {
                //Show the instruction
                if (SetInstructionText(_deployment.Instruction, _targetHexagon.transform.position))
                {
                    yield return new WaitForSeconds(1.0f);
                }

                //Spawn the troop
                if (_targetCard.TrySpawnTroop(_targetHexagon, true))
                {
                    _gameLoopManager.UpdateDeployCount();
                    _gameLoopManager.RemoveEnemyCard(_targetCard);
                }

                //Remove the deployment
                if (deployments.Contains(_deployment))
                    deployments.Remove(_deployment);

                yield return new WaitForSeconds(0.5f);
            }

            //If it as a player action
            else
            {
                //Check if the right card is selected
                if (_selectedCard)
                {
                    if (_selectedCard == _targetCard)
                    {
                        //Reset the outine
                        _targetCard.SetOutline(false);

                        //Highlight the tile to move to
                        _targetHexagon.GetComponent<HexagonView>().HighlightTutorialTile();

                        //Show the instruction
                        SetInstructionText(_deployment.Instruction, _targetHexagon.transform.position);

                        //Check if the player clicked the right tile
                        if (_selectedHexagon == _targetHexagon)
                        {
                            //Spawn the card to the tile
                            _selectedCard.TrySpawnTroop(_targetHexagon, true);

                            //Remove the deployment
                            if (deployments.Contains(_deployment))
                                deployments.Remove(_deployment);

                            //Reset the text
                            ResetInstructionText();

                            _targetHexagon.GetComponent<HexagonView>().UnHighlightTile();
                        }
                    }
                }
                else _targetCard.SetOutline(true); //Set the outline of the card

                yield return null;
            }
        }

        // Highlight the end turn button (player) / End turn automatically (enemy)
        if (_tutorialText && isFlemish)
        {
            OnButtonEnable?.Invoke();

            _tutorialText.DisplayEndTurnText(this, EventArgs.Empty);
        }
        else if (_gameLoopManager && !isFlemish)
        {
            yield return new WaitForSeconds(0.5f);

            _gameLoopManager.TurnEnd();
        }

        _isHandlingSide = false;
    }

    private void HandleScriptEnded()
    {
        if (_gameLoopManager)
            _gameLoopManager.EndGame(GameSides.Flemish);
    }

    private bool SetInstructionText(string key, Vector3 targetPosition, int infoIdx = 8)
    {
        if (!_stringTable || !_text || !_image || key == string.Empty) return false;

        bool isValidIdx = (infoIdx >= 0 && infoIdx <= 8);

        // If the index is invalid, we default to displaying above
        InstructionInfo info = isValidIdx ? _instructionInfos[infoIdx] : _instructionInfos[8];

        // Position the text above the player
        _image.enabled = true;
        _image.rectTransform.pivot = info.Pivot;
        _image.rectTransform.position = Vector3.zero;
        _image.transform.position = Camera.main.WorldToScreenPoint(targetPosition + info.Offset);

        // Set the localized text
        _currentKey = key;
        LocalizeInstructionText();

        return true;
    }

    private void LocalizeInstructionText()
    {
        if (!_stringTable || !_text || _currentKey == string.Empty) return;

        _text.text = _stringTable[_currentKey].Value;
    }

    private void ResetInstructionText()
    {
        if (_text) _text.text = string.Empty;
        if (_image) _image.enabled = false;
        _currentKey = string.Empty;
    }
}
