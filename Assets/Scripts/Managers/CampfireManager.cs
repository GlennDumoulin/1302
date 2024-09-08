using GameEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IntEventArgs : EventArgs
{
    public int Num { get; }

    public IntEventArgs(int num)
    {
        Num = num;
    }
}

public class ListImageEventArgs : EventArgs
{
    public List<Image> Images { get; }

    public ListImageEventArgs(List<Image> images)
    {
        Images = images;
    }
}
public class CampfireManager : MonoBehaviour, IDataPersistance<LevelPresetData>
{
    [SerializeField] private GameObject _campfire = null;
    [SerializeField] private GameObject _campfireParent = null;
    [SerializeField] private GameObject _flameSymbol = null;
    [SerializeField] private GameObject _radialCampfire = null;
    [SerializeField] private int _campfireLimit = 3;
    [SerializeField] private int _maxXRange = 10;
    [SerializeField] private int _maxYRange = 1;
    [SerializeField] private int _turnsTillVictory = 2;
    [SerializeField] private int _turnsTillCampfireDies = 4;

    private GridModel _gridModel = null;
    private GameLoopManager _gameLoopManager = null;
    private UICampfireCounter _campfireIcon;
    private GameSides _sideThatCaptured;
    private int _countdownCounter = 0;
    public int CountdownCounter { get { return _countdownCounter; } }
    private int _turnCounter = 0;
    public int TurnCounter { get { return _turnCounter; } }
    private int _switchCounter = 0;
    private int _campIndexToBeDestroyed = -1;
    private int _campfiresCappedByPlayer = 0;
    public int CampfiresCappedByPlayer { get { return _campfiresCappedByPlayer; } }
    private bool _startCounter = false;
    private bool _waitTurnAfterDestroy = false;

    private int _startingCooldown = 2;

    private List<CampfireModel> _campfireModels = new List<CampfireModel>();
    public List<CampfireModel> CampfireModel {get { return _campfireModels; } }

    public event EventHandler<ListImageEventArgs> OnCampfireCountUpdated;
    public event EventHandler OnCampfireDefendCounterUpdated;
    public event EventHandler OnCampfireWarningUpdate;

    private void Awake()
    {
        _gridModel = FindObjectOfType<GridModel>();
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _campfireIcon = FindObjectOfType<UICampfireCounter>();
        _startingCooldown *= 2;
    }


    private void OnEnable()
    {
        if (_gameLoopManager) _gameLoopManager.OnSwitchSide += IncrementCounter;
    }

    private void OnDisable()
    {
        if (_gameLoopManager) _gameLoopManager.OnSwitchSide -= IncrementCounter;
    }

    public void CreateCampfires()
    {
        int counter = 0;
        bool stopCreation = false;
        while (counter < _campfireLimit)
        {
            //Generate a Random Coordinate that is in the middle three rows
            int randX = UnityEngine.Random.Range(-(_maxXRange), _maxXRange + 1);
            int randY = UnityEngine.Random.Range(-(_maxYRange), _maxYRange + 1);

            //Check if the Random Coordinate is a Valid Tile;
            if ((randX + randY) % 2 != 0)
                continue;

            //Check if the Campfire is more than one tile away from any other Campfires
            foreach(CampfireModel model in _campfireModels)
            {
                if (model == null)
                    continue;

                Vector2 modelCoordinates = model.CurrentHexagonModel.DoubleCoordinates;

                int xDif = ((int) Mathf.Abs(modelCoordinates.x) - (int) Mathf.Abs(randX));
                int yDif = ((int)Mathf.Abs(modelCoordinates.y) - (int)Mathf.Abs(randY));

                if ((Mathf.Abs(xDif) + Mathf.Abs(yDif)) <= 2)
                    stopCreation = true;
            }

            if (stopCreation)
            {
                stopCreation = false;
                continue;
            }

            if (InstantiateCampfire(new Vector2(randX, randY)))
                counter++;
        }

        UpdateCampfireCaptureCount();
    }

    private bool InstantiateCampfire(Vector2 doubleCoordinates)
    {
        if (!_gridModel) return false;

        HexagonModel tile = _gridModel.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates));
        if (!tile) return false;

        //Check if the Hexagon Model does not have a Campfire already on it
        if (tile.Campfire != null)
            return false;

        //Instantiate the campfire
        GameObject newCampfire = Instantiate(_campfire, tile.transform.position, Quaternion.identity);

        //Set the campfire to the right parent
        newCampfire.transform.parent = _campfireParent.transform;

        CampfireModel campModel = newCampfire.GetComponent<CampfireModel>();
        campModel.CampfireManager = this;
        campModel.CurrentHexagonModel = tile;
        tile.Campfire = campModel;

        // Check if there was a troop on the tile already
        if (tile.Troop != null)
            campModel.SetCurrentDefender(tile.Troop);

        _campfireModels.Add(campModel);

        return true;
    }

    internal void CheckToStartCountdown()
    {
        //Check to see if the game should start the counter
        int counter = 0;
        foreach (var campfire in _campfireModels)
        {
            //Check if a campfire is defended
            if (!campfire.IsDefended)
            {
                _waitTurnAfterDestroy = false;
                return;
            }

            //If it is the first campfire checked, check the side
            if (counter == 0 && campfire.CurrentDefender != null)
                _sideThatCaptured = campfire.CurrentDefender.TroopData.Side;

            counter++;

            //Check if there is a troop on that campfire
            if (campfire.CurrentDefender == null)
            {
                _waitTurnAfterDestroy = false;
                return;
            }

            //Check if the troop is the same side as the previous troop
            if (campfire.CurrentDefender.TroopData.Side != _sideThatCaptured)
            {
                _waitTurnAfterDestroy = false;
                return;
            }
        }

        _startCounter = true;
    }

    internal void ResetCountdown()
    {
        _startCounter = false;
        _countdownCounter = 0;
        _switchCounter = 0;
        OnCampfireDefendCounterUpdated?.Invoke(this, EventArgs.Empty);
    }

    internal void IncrementCounter(object sender, EventArgs e)
    {
        if (_campfireModels.Count < 1) return;

        //Don't count the two beginning cooldown turns
        _startingCooldown--;

        //Wait an extra side switch before starting the countdown
        if (_startingCooldown == 0)
        {
            PickCampfireToGoOut();
            return;
        }

        if (_startingCooldown > -2)
            return;
       

        //After four turns (or 8 side switches), a random campfire should be destroyed
        _turnCounter++;
        if (_turnCounter < 0)
            return;

        if (_turnCounter % 2 == 1 && _turnCounter != 0)
            OnCampfireWarningUpdate?.Invoke(this, EventArgs.Empty);

        if (_turnCounter >= (_turnsTillCampfireDies * 2) - 1)
            DestroyRandomCampfire();

        if (!_startCounter)
            return;

        if (_waitTurnAfterDestroy)
        {
            _waitTurnAfterDestroy = false;
            return;
        }
        _switchCounter++;



        //The reason the code is like this is because this is to avoid the player that goes second from incrementing the counter too early by ending their turn.
        if (_switchCounter % 2 == 0)
        {
            _countdownCounter++;
            OnCampfireDefendCounterUpdated?.Invoke(this, EventArgs.Empty);
        }

        if (_countdownCounter >= _turnsTillVictory)
            _gameLoopManager.EndGame(_sideThatCaptured);
    }

    private void DestroyRandomCampfire()
    {
        if (_campfireModels.Count <= 1)
            return;

        CampfireModel model = _campfireModels.ElementAt(_campIndexToBeDestroyed);

        //Tell the HexagonModel that there is no longer a Campfire on that tile
        HexagonModel tile = model.CurrentHexagonModel;
        tile.Campfire = null;

        _campfireModels.RemoveAt(_campIndexToBeDestroyed);
        _campfireIcon.RemoveCampfireFromList(_campIndexToBeDestroyed);
        model.SetToBeDestroyed();
        _turnCounter = 0;
        _waitTurnAfterDestroy = true;
        UpdateCampfireCaptureCount();
        CheckToStartCountdown();

        if (_campfireModels.Count > 1)
        {
            PickCampfireToGoOut();
        }
    }

    internal void UpdateCampfireCaptureCount()
    {
        OnCampfireCountUpdated?.Invoke(this, new ListImageEventArgs(_campfireIcon.CampfireIcons));
    }

    internal void SpawnFlameSymbol(Transform pos)
    {
        Instantiate(_flameSymbol, pos.position, Quaternion.identity);
    }

    private void PickCampfireToGoOut()
    {
        _campIndexToBeDestroyed = UnityEngine.Random.Range(0, _campfireModels.Count);
        _campfireModels[_campIndexToBeDestroyed].ShowWarning();
        Instantiate(_radialCampfire, _campfireModels[_campIndexToBeDestroyed].transform.position, _radialCampfire.transform.rotation);
    }

    public void LoadData(LevelPresetData data)
    {
        _turnsTillVictory = data.Campfires.TurnsTillVictory;
        _turnsTillCampfireDies = data.Campfires.TurnsTillCampfireDies;
        _countdownCounter = data.Campfires.CountdownCounter;
        _turnCounter = data.Campfires.TurnCounter;
        _switchCounter = data.Campfires.SwitchCounter;
        _campfiresCappedByPlayer = data.Campfires.CampfiresCappedByPlayer;
        _startCounter = data.Campfires.StartCounter;
        _waitTurnAfterDestroy = data.Campfires.WaitTurnAfterDestroy;

        ResetData();

        LoadCampfires(data.Campfires.CampfirePositions);
    }

    private void ResetData()
    {
        foreach (CampfireModel campfire in _campfireModels)
        {
            campfire.CurrentHexagonModel.Campfire = null;
            campfire.SetCurrentDefender(null);
            Destroy(campfire.gameObject);
        }
        _campfireModels.Clear();
    }

    private void LoadCampfires(List<CampfirePosition> campfirePositions)
    {
        if (!_gridModel || !_campfire) return;

        foreach (CampfirePosition campfirePosition in campfirePositions)
        {
            // Make sure the given position is valid
            if (campfirePosition.Position.Count < 2) continue;

            // Find the tile the campfire has to spawn on
            Vector2 doubleCoordinates = new Vector2(campfirePosition.Position[0], campfirePosition.Position[1]);
            HexagonModel spawnTile = _gridModel.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates));
            if (!spawnTile) continue;

            // Create the campfire at the desired tile
            InstantiateCampfire(doubleCoordinates);
        }
    }

    public void SaveData(ref LevelPresetData data)
    {
        data.Campfires.TurnsTillVictory = _turnsTillVictory;
        data.Campfires.TurnsTillCampfireDies = _turnsTillCampfireDies;
        data.Campfires.CountdownCounter = _countdownCounter;
        data.Campfires.TurnCounter = _turnCounter;
        data.Campfires.SwitchCounter = _switchCounter;
        data.Campfires.CampfiresCappedByPlayer = _campfiresCappedByPlayer;
        data.Campfires.StartCounter = _startCounter;
        data.Campfires.WaitTurnAfterDestroy = _waitTurnAfterDestroy;

        data.Campfires.CampfirePositions.Clear();
        foreach (CampfireModel campfire in CampfireModel)
        {
            CampfirePosition campfirePosition = new CampfirePosition();

            Vector2 campfireCoordinates = campfire.CurrentHexagonModel.DoubleCoordinates;
            campfirePosition.Position = new List<int> { (int)campfireCoordinates.x, (int)campfireCoordinates.y };

            data.Campfires.CampfirePositions.Add(campfirePosition);
        }
    }
}
