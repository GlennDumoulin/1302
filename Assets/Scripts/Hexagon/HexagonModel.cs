using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;

public class HexagonModel : MonoBehaviour
{
    //The width of a tile
    private float _width = 0;

    //The height of a tile
    private float _depth = 0;

    //The dimensions of the tile
    private Vector2 _dimensions = new Vector2();

    private GameLoopManager _gameManager = null;
    private GridView _gridView = null;

    public event EventHandler OnCampfireDefenderEnter;
    public event EventHandler OnCampfireDefenderLeave;

    public float Width
    {
        get { return _width; }
        set { _width = value; }
    }

    public float Depth
    {
        get { return _depth; }
        set { _width = value; }
    }

    public Vector2 Dimensions
    {
        get { return _dimensions; }
        set { _dimensions = value; }
    }

    //QRS
    private Vector3 _qrs = Vector3.zero;

    public Vector3 Qrs
    {
        get { return _qrs; }
        set { _qrs = value; }
    }

    //Double Coordinates
    private Vector2 doubleCoordinates = Vector2.zero;

    public Vector2 DoubleCoordinates
    {
        get { return doubleCoordinates; } 
        set { doubleCoordinates = value; }
    }

    //Troop on tile
    private TroopModel _troop = null;

    public TroopModel Troop
    {
        get { return _troop; }
        set 
        { 
            _troop = value;
            if (Campfire) Campfire.SetCurrentDefender(value);
            if (Campfire && Troop != null && Troop.HP > 0)
                OnCampfireDefenderEnter?.Invoke(this, EventArgs.Empty);
            else if (Campfire && Troop == null)
                OnCampfireDefenderLeave?.Invoke(this, EventArgs.Empty);
        }
    }

    //Campfire on Tile
    private CampfireModel _campfire = null;
    public CampfireModel Campfire
    {
        get { return _campfire; }
        set 
        {  
            _campfire = value;
            if (Campfire == null)
                OnCampfireDefenderLeave?.Invoke(this, EventArgs.Empty);
        }
    }

    //Mud on Tile
    private bool _isMud = false;
    public bool IsMud
    {
        get { return _isMud; }
        set {  _isMud = value; }
    }

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameLoopManager>();
        _gridView = FindObjectOfType<GridView>();
    }

    //Calculate the width and depth 
    public Vector2 CalculateWidthDepth(float size)
    {
        _width = MathF.Sqrt(3) * size;
        _depth = size * 2;

        Dimensions = new Vector2(_width, _depth);

        return Dimensions;
    }

    public bool IsValidStartTile(GameSides cardSide)
    {
        if (!_gameManager) return false;

        return IsStartTile(cardSide == _gameManager.PlayerSide) && Troop == null;
    }

    public bool IsStartTile(bool isPlayerTile)
    {
        if (!_gridView) return false;

        int nrRowsPerSide = _gridView.Rows / 2;
        int startRowIdx = isPlayerTile ? -nrRowsPerSide : nrRowsPerSide;

        return DoubleCoordinates.y == startRowIdx;
    }
}
