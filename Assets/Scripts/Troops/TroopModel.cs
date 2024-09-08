using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TroopMovedEventArgs : EventArgs
{
    public HexagonModel ToPosition { get; }

    public TroopScriptableObject TroopData { get; }

    public bool Charge { get; }

    public TroopModel Defender { get; }

    public TroopModel Attacker { get; }

    public TroopMovedEventArgs(HexagonModel toPosition, TroopScriptableObject troopData, bool charge, TroopModel defender, TroopModel attacker)
    {
        ToPosition = toPosition;
        TroopData = troopData;
        Charge = charge;
        Defender = defender;
        Attacker = attacker;
    }
}

public class TroopModel : MonoBehaviour, IDataPersistance<TroopData>
{
    //Event handlers
    public event EventHandler OnTroopAttacked = null;
    public event EventHandler OnTroopDeath = null;
    public event EventHandler OnShieldBroken = null;
    public event EventHandler OnTroopAct = null;

    //Managers / grid
    private GridModel _gridModel = null;
    private GameLoopManager _gameLoopManager = null;

    //Acted this turn
    private bool _hasActed = false;
    public bool HasActed
    {
        get { return _hasActed; }
        set
        {
            _hasActed = value;
            OnTroopAct?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField] private string _assetKey = string.Empty;

    //The troopdata
    [SerializeField]
    private TroopScriptableObject _troopData = null;
    public TroopScriptableObject TroopData
    {
        get { return _troopData; }
    }

    //The current hexagon the troop is on
    private HexagonModel _currentHexagonModel = null;
    public HexagonModel CurrentHexagonModel
    {
        get { return _currentHexagonModel; }
        set { _currentHexagonModel = value; }
    }

    //All the live variables
    private int _hp = -1;
    public int HP { get { return _hp; } }

    private bool _isDead = false;

    public bool IsDead
    {
        get { return _isDead; }
    }

    //Reloading
    private bool _shouldReload = false;
    public bool ShouldReload
    {
        get { return _shouldReload; }
        set { _shouldReload = value; }
    }

    private bool _reloadThisTurn = false;
    public bool ReloadThisTurn
    {
        get { return _reloadThisTurn; }
        set { _reloadThisTurn = value; }
    }

    private bool _canMove = true;
    public bool CanMove
    {
        get { return _canMove; }
    }

    //The location where the charge started
    private HexagonModel _startChargeLocation = null;
    public HexagonModel StartChargeLocation
    {
        get { return _startChargeLocation; }
        set { _startChargeLocation = value; }
    }

    //Shield
    private int _shieldPoints = 0;
    public int ShieldPoints
    {
        get { return _shieldPoints; }
        set { _shieldPoints = value; }
    }

    private bool _shieldBroken = false;
    public bool ShieldBroken
    {
        get { return _shieldBroken; }
        set { _shieldBroken = value; }
    }

    public UnityEvent OnDamaged;

    private int _tutorialIdx = 0;
    public int TutorialIdx
    {
        get { return _tutorialIdx; }
        set { _tutorialIdx = value; }
    }

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _gridModel = FindObjectOfType<GridModel>();
    }

    private void Start()
    {
        if (_hp == -1) _hp = TroopData.Health;
    }

    private void OnEnable()
    {
        if (_gameLoopManager) _gameLoopManager.OnTurnEnd += CheckIfMoveMud;
    }

    private void OnDisable()
    {
        if (_gameLoopManager) _gameLoopManager.OnTurnEnd -= CheckIfMoveMud;
    }

    internal void DealDamage(int damage, bool playAnimation = true)
    {
        int remainingDmg = damage; //The damage that is left after destroying the shield(if it destroys the shield)

        //If we have a shield
        if (_shieldPoints > 0)
        {
            remainingDmg = damage - _shieldPoints;

            _shieldPoints -= damage;

            if (_shieldPoints <= 0)
            {
                _shieldPoints = 0;
                OnShieldBroken(this, new EventArgs());
            }
        }

        //Apply the remaining damage on the troop's health
        _hp -= remainingDmg;

        //Check if the troop died
        if (_hp <= 0)
        {
            OnTroopDeath?.Invoke(this, EventArgs.Empty);
            _isDead = true;
            _gameLoopManager.TroopKilled(_troopData.Side);
        } else
        {
            //play the animation when needed
            if(playAnimation) OnDamaged?.Invoke();

            OnTroopAttacked?.Invoke(this, EventArgs.Empty);
        }
    }

    public HexagonModel GetClosestTileInDirection(HexagonModel attacker, HexagonModel enemy)
    {
        //If we don't have an attacker or enemy tile return nothing
        if (!attacker || !enemy) return null;

        //Get the qrs of the enemy
        var qrsEnemy = enemy.Qrs;

        //Get the qrs of the attacker
        var qrsAttacker = attacker.Qrs;

        //Get the qrs diff
        var qrsDiff = qrsAttacker - qrsEnemy;

        //Find the common qrs value and set it to 0 (don't need it)
        if (qrsEnemy.x == qrsAttacker.x)
        {
            qrsDiff.x = 0;

            qrsDiff.y /= Math.Abs(qrsDiff.y);
            qrsDiff.z /= Math.Abs(qrsDiff.z);
        }

        else if (qrsEnemy.y == qrsAttacker.y)
        {
            qrsDiff.y = 0;

            qrsDiff.x /= Math.Abs(qrsDiff.x);
            qrsDiff.z /= Math.Abs(qrsDiff.z);
        }

        else if (qrsEnemy.z == qrsAttacker.z)
        {
            qrsDiff.z = 0;

            qrsDiff.x /= Math.Abs(qrsDiff.x);
            qrsDiff.y /= Math.Abs(qrsDiff.y);
        }

        //Get the right qrs tile
        var qrsMoveTile = qrsEnemy + qrsDiff;

        //Find the tile at qrs coordinate
        var tile = _gridModel.FindTileAtQrs(qrsMoveTile);
        return tile;
    }

    internal bool CheckIfTroopIsOnTile(HexagonModel hexagonModel)
    {
        if (hexagonModel.Troop == null)
            return false;
        return true;
    }

    public void NewTile(HexagonModel hexTile)
    {
        if (!hexTile) return;

        if (_currentHexagonModel)
            _currentHexagonModel.Troop = null;

        CurrentHexagonModel = hexTile;
        _currentHexagonModel.Troop = this;
    }

    private void CheckIfMoveMud(object sender, EventArgs e)
    {
        if (!_canMove)
        {
            _canMove = true;
            return;
        }

        if (CurrentHexagonModel.IsMud)
            _canMove = false;
        else
            _canMove = true;
    }

    public void LoadData(TroopData data)
    {
        _hp = data.HP;
        _shieldPoints = data.ShieldPoints;
        _shieldBroken = data.ShieldBroken;

        _shouldReload = data.ShouldReload;
        _reloadThisTurn = data.ReloadThisTurn;
        _canMove = data.CanMove;

        StartCoroutine(LoadHasActed(data.HasActed));

        if (_troopData)
        {
            if (_troopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
            {
                TroopController controller = GetComponent<TroopController>();
                if (controller)
                {
                    int eventArg = 2;

                    if (_shouldReload)
                        eventArg = 0;
                    else if (_reloadThisTurn)
                        eventArg = 1;

                    controller.UpdateCrossbowUIOnLoad(eventArg);
                }
            }
        }
    }

    private IEnumerator LoadHasActed(bool hasActed)
    {
        yield return null;

        HasActed = hasActed;
    }

    public void SaveData(ref TroopData data)
    {
        data.HP = _hp;
        data.ShieldPoints = _shieldPoints;
        data.ShieldBroken = _shieldBroken;

        data.ShouldReload = _shouldReload;
        data.ReloadThisTurn = _reloadThisTurn;
        data.HasActed = _hasActed;
        data.CanMove = _canMove;

        data.AssetKey = _assetKey;
        data.CurrentHexagonCoordinates = new List<int> { (int)CurrentHexagonModel.DoubleCoordinates.x, (int)CurrentHexagonModel.DoubleCoordinates.y };
    }
}
