using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RainMudManager : MonoBehaviour, IDataPersistance<LevelPresetData>
{
    [Header("Game Objects")]
    [SerializeField]
    private GameObject _mudTile = null;
    [SerializeField]
    private GameObject _mudTileParent = null;
    [SerializeField]
    private bool _isActive = true;

    [Header("Unity Events")]
    public UnityEvent OnRainStart;
    public UnityEvent OnRainEnd;

    private GridModel _gridModel = null;
    private GameLoopManager _gameLoop = null;

    private List<int> _rainChances = new List<int> { 25, 45, 70, 100 };
    private List<HexagonModel> _mudTiles = new List<HexagonModel>();
    private List<GameObject> _mudTilesObj = new List<GameObject>();

    private const int _maxXRange = 10;
    private const int _maxYRange = 4;

    private int _turnsWithoutRain = 0;
    private int _turnsWithMud = 0;
    private int _maxMudTurns = 3;
    private int _cooldownCounter = 0;
    private int _cooldownLimit = 1;

    private bool _onCooldown = false;

    private ParticleSystem _rainParticle = null;

    private bool _isRaining = false;
    public bool IsRaining { get { return _isRaining; } }

    private void Awake()
    {
        _gridModel = FindObjectOfType<GridModel>();
        _gameLoop = FindObjectOfType<GameLoopManager>();

        if (_gameLoop) _gameLoop.OnTurnEnd += CalculateRainMud;
    }

    private void Start()
    {
        //Get the rainparticle
        _rainParticle = GetComponentInChildren<ParticleSystem>();

        //Disable the particle at start
        if (_rainParticle) _rainParticle.Stop();
    }

    private void OnDisable()
    {
        if (_gameLoop) _gameLoop.OnTurnEnd -= CalculateRainMud;
    }

    private void CalculateRainMud(object sender, EventArgs e)
    {
        if (!_isActive) return;

        if (!_isRaining && !_onCooldown)
        {
            int randNum = UnityEngine.Random.Range(0, 100);

            if (randNum < _rainChances[_turnsWithoutRain])
            {
                //Spawn the mud
                SpawnMud(3);
                SpawnMud(3);
                SpawnMud(2);

                //Set isRaining to True
                _isRaining = true;

                //reset the counter
                _turnsWithoutRain = 0;

                //Play the particle
                if (_rainParticle)
                {
                    _rainParticle.Play();
                    OnRainStart?.Invoke();
                }
            } else
                _turnsWithoutRain++;

        } else if (_isRaining && !_onCooldown)
        {
            if (_turnsWithMud < _maxMudTurns)
                _turnsWithMud++;
            else
            {
                //Remove the mud tiles
                RemoveMud();

                //Set isRaining to False
                _isRaining = false;

                //Reset counter
                _turnsWithMud = 0;

                //Stop the rain
                if (_rainParticle)
                {
                    _rainParticle.Stop();
                    OnRainEnd?.Invoke();
                }
            }
        } else
        {
            if (_cooldownCounter < _cooldownLimit) 
            {
                _cooldownCounter++;
                if (_cooldownCounter >= _cooldownLimit)
                {
                    _onCooldown = false;
                    _cooldownCounter = 0;
                }
            }
        }
    }

    private void SpawnMud(int numOfMud)
    {
        //Spawn a specific amount of mud tiles
        int randMudTileCount = numOfMud;
        int counter = 0;

        bool stopCreation = false;

        List<Vector2> currentMudCoodrdinates = new List<Vector2>();

        while (counter < randMudTileCount)
        {
            stopCreation = false;

            //Generate a Random Coordinate on the board
            int randX = UnityEngine.Random.Range(-(_maxXRange), _maxXRange + 1);
            int randY = UnityEngine.Random.Range(-(_maxYRange), _maxYRange + 1);

            //Check if the Random Coordinate is a Valid Tile
            if ((randX + randY) % 2 != 0)
                continue;

            //Check if the coordinate isn't on the first row of either side
            if (randY == -_maxYRange || randY == _maxYRange)
                continue;

            //If you are starting a new cluster, check to make sure the cluster is atleast 3 tiles away from another cluster
            if (currentMudCoodrdinates.Count == 0)
            {
                foreach(var mud in _mudTiles)
                {
                    if (mud == null)
                        continue;

                    Vector2 modelCoordinates = mud.DoubleCoordinates;

                    int xDif = ((int)modelCoordinates.x - randX);
                    int yDif = ((int)modelCoordinates.y - randY);

                    if ((Mathf.Abs(xDif) + Mathf.Abs(yDif)) <= 6)
                        stopCreation = true;
                }
            }

            //Check to make sure that the mud tile is atleast 1 away from both mud tiles
            foreach (var coord in currentMudCoodrdinates)
            {
                if (coord == null)
                    continue;

                int xDif = ((int)coord.x - randX);
                int yDif = ((int)coord.y - randY);

                if ((Mathf.Abs(xDif) + Mathf.Abs(yDif)) > 2)
                    stopCreation = true;
            }

            if (stopCreation)
            {
                stopCreation = false;
                continue;
            }

            //If that tile can be muddied, increase the counter
            Vector2 doubleCoordinates = new Vector2(randX, randY);

            if (InstantiateMud(doubleCoordinates))
            {
                currentMudCoodrdinates.Add(doubleCoordinates);
                counter++;
            }
        }
    }

    private bool InstantiateMud(Vector2 doubleCoordinates, bool forceInstantiate = false)
    {
        if (!_gridModel) return false;

        //Check if there is a valid Hexagon Model at the coordinate
        HexagonModel tile = _gridModel.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates));
        if (!tile) return false;

        if (!forceInstantiate)
        {
            //Check if the tile hasn't already been muddied
            if (tile.IsMud)
                return false;

            //Check if there isn't a campfire on the tile
            if (tile.Campfire != null)
                return false;
        }

        //Instantiate the campfire
        GameObject newMudTile = Instantiate(_mudTile, tile.transform.position - new Vector3(0.0f, 0.15f, 0.0f), Quaternion.identity);

        //Set the campfire to the right parent
        newMudTile.transform.parent = _mudTileParent.transform;

        // Get a random rotation for the tile so there is at least some variation in the mud
        float angle = (UnityEngine.Random.Range(0, 6) * 60.0f) + 30.0f;
        newMudTile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        _mudTilesObj.Add(newMudTile);

        tile.IsMud = true;
        _mudTiles.Add(tile);
        return true;
    }

    private void RemoveMud()
    {
        foreach (HexagonModel tile in _mudTiles)
        {
            tile.IsMud = false;
        }
        _mudTiles.Clear();

        foreach (GameObject obj in _mudTilesObj)
        {
            obj.GetComponent<MudTileTweenHelper>().SetToDestroy();
        }
        _mudTilesObj.Clear();

        _onCooldown = true;
    }

    public void LoadData(LevelPresetData data)
    {
        ResetData();

        _isActive = data.RainMud.IsActive;
        _turnsWithoutRain = data.RainMud.TurnsWithoutRain;
        _turnsWithMud = data.RainMud.TurnsWithMud;
        _maxMudTurns = data.RainMud.MaxMudTurns;
        _cooldownCounter = data.RainMud.CooldownCounter;
        _cooldownLimit = data.RainMud.CooldownLimit;
        _onCooldown = data.RainMud.OnCooldown;
        _isRaining = data.RainMud.IsRaining;

        if (_isRaining && _rainParticle)
        {
            _rainParticle.Play();
            OnRainStart?.Invoke();
        }

        LoadMudTiles(data.RainMud.MudPositions);
    }

    private void ResetData()
    {
        RemoveMud();
    }

    private void LoadMudTiles(List<MudPosition> mudPositions)
    {
        if (!_gridModel || !_mudTile) return;

        foreach (MudPosition mudPosition in mudPositions)
        {
            // Make sure the given position is valid
            if (mudPosition.Position.Count < 2) continue;

            // Find the tile the mud has to spawn on
            Vector2 doubleCoordinates = new Vector2(mudPosition.Position[0], mudPosition.Position[1]);
            HexagonModel spawnTile = _gridModel.FindTileAtQrs(CoordinatesHelper.DoubleCoordinatesToQrs(doubleCoordinates));
            if (!spawnTile) continue;

            // Create the mud at the desired tile
            InstantiateMud(doubleCoordinates, true);
        }
    }

    public void SaveData(ref LevelPresetData data)
    {
        data.RainMud.IsActive = _isActive;
        data.RainMud.TurnsWithoutRain = _turnsWithoutRain;
        data.RainMud.TurnsWithMud = _turnsWithMud;
        data.RainMud.MaxMudTurns = _maxMudTurns;
        data.RainMud.CooldownCounter = _cooldownCounter;
        data.RainMud.CooldownLimit = _cooldownLimit;
        data.RainMud.OnCooldown = _onCooldown;
        data.RainMud.IsRaining = _isRaining;

        data.RainMud.MudPositions.Clear();
        foreach (HexagonModel mudTile in _mudTiles)
        {
            MudPosition mudPosition = new MudPosition();

            Vector2 mudCoordinates = mudTile.DoubleCoordinates;
            mudPosition.Position = new List<int> { (int)mudCoordinates.x, (int)mudCoordinates.y };

            data.RainMud.MudPositions.Add(mudPosition);
        }
    }
}
