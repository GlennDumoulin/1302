using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using GameEnum;
using System;
using UnityEngine.Events;

public class TroopController : MonoBehaviour
{
    public event EventHandler<IntEventArgs> OnUpdateCrossbowUI = null;
    private bool _hasMoved = false;

    [SerializeField] private TroopView _troopView = null;
    [SerializeField] private TroopModel _troopModel = null;

    private GameLoopManager _gameLoopManager = null;

    private GridController _gridController = null;
    private GridModel _gridModel = null;

    private HexagonModel _tileAfterAttack = null;
    public HexagonModel TileAfterAttack
    {
        get { return _tileAfterAttack; }
        set { _tileAfterAttack = value; }
    }

    private Vector3 _targetAfterAttack = Vector3.zero;  
    public Vector3 TargetAfterAttack
    {
        get { return _targetAfterAttack; }
        set { _targetAfterAttack = value; }
    }

    private TroopModel _attackingHorse = null;
    private TroopModel _defendingAntiHorse = null;

    private TutorialScriptHandler _tutorial = null;

    private bool _isDefending = false;
    public bool IsDefending
    {
        get { return _isDefending; }
        set { _isDefending = value; }
    }

    public UnityEvent OnDealDamage;
    public UnityEvent OnStuckInMud;
    public UnityEvent OnHorseAntihorse;

    // Start is called before the first frame update
    private void Start()
    {
        //Get the grid
        _gridController = FindObjectOfType<GridController>();
        _gridModel = FindObjectOfType<GridModel>();

        //Get the gamemanager
        _gameLoopManager = FindObjectOfType<GameLoopManager>();

        //Rotate pawn if needed, if the Troop is not of the same side of the player
        if (!_gameLoopManager.PlayerSide.Equals(_troopModel.TroopData.Side))
            _troopView.SetStartRotation(180.0f);

        //Add events
        _troopView.OnChargeArrived += DealDamage;
        _gameLoopManager.OnSwitchSide += OnNextTurn;
        _gameLoopManager.OnSwitchSide += SetTroopActStatus;
        _gameLoopManager.OnMoveToDeployPhase += DesaturateTroopInDeployPhase;
        _troopModel.OnShieldBroken += ShieldBroken;

        //Add it to the gamemanager
        _gameLoopManager.AddTroop(this);

        //Get the tutorial
        _tutorial = FindObjectOfType<TutorialScriptHandler>();
    }

    private void OnDisable()
    {
        _troopView.OnChargeArrived -= DealDamage;
        _gameLoopManager.OnSwitchSide -= OnNextTurn;
        _gameLoopManager.OnSwitchSide -= SetTroopActStatus;
        _gameLoopManager.OnMoveToDeployPhase -= DesaturateTroopInDeployPhase;
        _troopModel.OnShieldBroken -= ShieldBroken;
    }

    public bool Attack(TroopModel attacker, TroopModel enemy)
    {
        //All the tiles it can attack
        var tiles = _gridController.GetAttackTiles(_gridModel.HexagonModels, _troopModel.CurrentHexagonModel, _troopModel.TroopData);

        //Check if we can attack
        if (!enemy || !tiles.Contains(enemy.CurrentHexagonModel))
            return false;

        //Defender look at enemy
        enemy.GetComponent<TroopView>().LookAtEnemy(attacker);

        //Move into location for attack
        if (attacker.TroopData.SpecialCharacteristic == TroopSpecialCharacteristics.Charge)
        {
            //If the Troop is Stuck in Mud, do not charge
            if (!attacker.CanMove)
                return false;

            //Store the startposition of the charge
            _troopModel.StartChargeLocation = _troopModel.CurrentHexagonModel;

            //Move towards the enemy
            Move(enemy.CurrentHexagonModel, true, enemy);
        }

        else
        {
            //If it is reloading can't attack
            if (_troopModel.ReloadThisTurn)
                return false;

            //Look at enemy
            _troopView.LookAtEnemy(enemy);

            //Attack if it is an anti-horse
            if(_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.DamageReflect))
                DealDamage(attacker, enemy);

            //Shoot arrow in straight line when crossbow
            if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
                _troopView.ShootArrow(false, attacker, enemy);

            //Shoot arrow in curve when it is an archer 
            else if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.UseRadius))
                _troopView.ShootArrow(true, attacker, enemy);
        }

        return true;
    }

    public void DealDamage(TroopModel attacker, TroopModel enemy)
    {
        //If the troop is a charging unit and it is stuck in mud, it cannot attack
        if (attacker.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge) && !attacker.CanMove)
            return;

        //Check if a Knight has attacked an Anti-Horse enemy, if so, deal damage to the Knight
        if (attacker.TroopData.IsDamageReflectable && enemy.TroopData.SpecialCharacteristic == TroopSpecialCharacteristics.DamageReflect)
        {
            //Let the animations play of the attacking horse
            OnHorseAntihorse?.Invoke();
            //Let the animations play of the defending anti-horse
            enemy.GetComponent<TroopController>().OnHorseAntihorse?.Invoke();

            //Set the attacking horse and defending anti-horse
            _attackingHorse = attacker;
            _defendingAntiHorse = enemy;

            //Let the enemy know it is defending
            enemy.GetComponent<TroopController>().IsDefending = true;

            return;
        }

        else if (!attacker.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload) && !attacker.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.UseRadius))
        {
            enemy.DealDamage(attacker.TroopData.Damage);  //Deal the damage
            OnDealDamage?.Invoke();
        }
        else       
            enemy.DealDamage(attacker.TroopData.Damage);  //Deal the damage


        //If the enemy isn't dead yet and you charged move back 1 tile
        if (!enemy.IsDead && attacker.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge))
        {
            //Get the tile to move back to after an attack
            _tileAfterAttack = _troopModel.GetClosestTileInDirection(_troopModel.StartChargeLocation, enemy.CurrentHexagonModel);
        }

        if (enemy.IsDead)
        {
            ////Let the game know we killed a troop
            //_gameLoopManager.TroopKilled(enemy.TroopData.Side);
            enemy.CurrentHexagonModel.Troop = null;

            if(_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge))
                _tileAfterAttack = enemy.CurrentHexagonModel; //Get the tile to move back to after an attack
        }

        if (attacker.IsDead)
        {
            ////Let the game know we killed a troop
            //_gameLoopManager.TroopKilled(attacker.TroopData.Side);
            attacker.CurrentHexagonModel.Troop = null;
        }
    }

    public bool Move(HexagonModel hexTile, bool isCharge, TroopModel defender)
    {
        //If the troop is Stuck in Mud, do not move
        if (!_troopModel.CanMove)
            return false;

        //Movement tiles
        var tiles = _gridController.GetMovementTiles(_gridModel.HexagonModels, _troopModel.CurrentHexagonModel, _troopModel.TroopData);

        //Add the attack tiles as possible movementtiles if it is a charge
        if (isCharge)
        {
            tiles.AddRange(_gridController.GetAttackTiles(_gridModel.HexagonModels, _troopModel.CurrentHexagonModel, _troopModel.TroopData));
        }

        //Check if we can move to the tile he wants to move
        if (!tiles.Contains(hexTile))
            return false;

        ////Set new tile if it isn't a charge
        if (!isCharge)
            _troopModel.NewTile(hexTile);
        else
        {
            //If there is a troop on the targeted tile
            if (hexTile.Troop)
            {
                //Predict the new tile
                ////Check if the troop would die
                if (_troopModel.TroopData.Damage < hexTile.Troop.HP)
                {
                    //Get the tile next to it
                    var tile = _troopModel.GetClosestTileInDirection(_troopModel.CurrentHexagonModel, hexTile);

                    //Set it as the new tile
                    _troopModel.NewTile(tile);
                }
                //Set the standard new tile
                else _troopModel.NewTile(hexTile);
            }
        }

        //Call event in troop view
        //OnTroopMoved?.Invoke(this, new TroopMovedEventArgs(hexTile, _troopData, isCharge));
        Vector3 targetLoc = CoordinatesHelper.DoubleCoordinatesToWorld(hexTile.DoubleCoordinates, hexTile.Dimensions);

        //Go close to the unit
        if (isCharge)
        {
            //Set the position it needs to move to after an attack
            _targetAfterAttack = targetLoc;

            //Direction from defender to attacker
            Vector3 toAttacker = targetLoc - gameObject.transform.position;
            toAttacker.Normalize();

            //A little offset to the enemy
            const int attackOffset = 2;
            toAttacker *= attackOffset;

            //Set the new targetLoc
            targetLoc -= toAttacker;
        }

        //Start the movement 
        StartCoroutine(_troopView.MoveToPos(targetLoc, _troopModel.TroopData.MovementSpeed, isCharge, defender, _troopModel));

        //Reset the shield if it is a crossbow
        if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
        {
            _troopView.Shield.SetActive(false);
            _hasMoved = true;
        }

        return true;
    }

    internal bool TryToActWithTroop(HexagonModel selectedHexagon, TroopModel selectedTroop, bool triggeredByTutorial = false)
    {
        if(_tutorial && !triggeredByTutorial)
        {
            return true;
        }

        //If the selectedTroop is this troop or this troop already acted return false
        if (selectedTroop == this || _troopModel.HasActed) return false;

        //If there is a selectedhexa
        if (selectedHexagon && selectedHexagon != _troopModel.CurrentHexagonModel)
        {
            //If there isn't a troop the selected tile
            if (!_troopModel.CheckIfTroopIsOnTile(selectedHexagon))
            {
                //Move to the right tile
                if (Move(selectedHexagon, false, null)) //null --> No attack
                {
                    _troopModel.HasActed = true;
                    return true;
                }
                else return false;
            }

            //if there is a troop on the selected tile
            else
            {
                //Attack that tile
                if (Attack(_troopModel, selectedHexagon.Troop)) //Attack
                {
                    _troopModel.HasActed = true;

                    if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
                    {
                        _troopModel.ShouldReload = true;
                        OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(0));
                    }
                    return true;
                }
                else return false;
            }
        }

        //If there is a selected troop
        if (selectedTroop && selectedTroop != _troopModel)
        {
            if (Attack(_troopModel, selectedTroop))
            {
                //Can't act this turn
                _troopModel.HasActed = true;

                //Let the troop reload when it need to reload
                if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
                {
                    _troopModel.ShouldReload = true;
                    OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(0));
                }

                return true;
            }
            else return false;
        }

        return false;
    }

    public bool CanAttackEnemy(TroopModel attacker, TroopModel enemy)
    {
        //All the tiles it can attack
        var tiles = _gridController.GetAttackTiles(_gridModel.HexagonModels, _troopModel.CurrentHexagonModel, _troopModel.TroopData);

        //Check if we can attack
        if (!enemy || !tiles.Contains(enemy.CurrentHexagonModel))
            return false;
        
        return true;
    }

    public void UnselectTroop()
    {
        _gridModel.HexagonsUnHighlight();
    }

    internal void OnTroopSelected()
    {
        //Invoke the stuck in mude 
        if (!_troopModel.CanMove)
            OnStuckInMud?.Invoke();

        if (!_troopModel.HasActed)
        {
            _gridModel.HexagonsAllAttackHighlight(_troopModel.CurrentHexagonModel, _troopModel.TroopData);

            //If the troop is stuck in mud, do not highlight the Movement Highlights
            if (_troopModel.CanMove)
                _gridModel.HexagonsMovementHighlight(_troopModel.CurrentHexagonModel, _troopModel.TroopData);

            //If the troop is a charging unit, do not highlight the Attack Tiles
            if (!_troopModel.CanMove && _troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.Charge))
                return;

            _gridModel.HexagonsAttackHighlight(_troopModel.CurrentHexagonModel, _troopModel.TroopData);
        }
        else if (_gameLoopManager.PlayerSide.Equals(_troopModel.TroopData.Side))
        {
            //Highlight the grey attack tiles
            _gridModel.HexagonsAllAttackHighlight(_troopModel.CurrentHexagonModel, _troopModel.TroopData);
        }
    }

    public void OnEnemyTroopSelected()
    {
        //Highlight the grey attack tiles
        _gridModel.HexagonsAllAttackHighlight(_troopModel.CurrentHexagonModel, _troopModel.TroopData);
    }

    private void OnNextTurn(object sender, EventArgs e)
    {
        // Only handle this if we are switching to the checked troop's side
        if (_troopModel.TroopData.Side != _gameLoopManager.CurrentGameSide)
            return;

        //Check if it is a crossbow
        if (_troopModel.TroopData.SpecialCharacteristic.Equals(TroopSpecialCharacteristics.CrossbowFireReload))
        {
            //Reset the shield
            if (_troopModel.ReloadThisTurn)
            {
                //Stop the reload
                _troopModel.ReloadThisTurn = false;

                //If the troop has not acted it has reloaded
                if (_hasMoved)
                    _troopModel.ShouldReload = true;
                else
                    OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(2));

                //Reset shield
                _troopView.Shield.SetActive(false);
                _troopModel.ShieldPoints = 0;
            }

            //Show the shield when it reloads
            if (_troopModel.ShouldReload )
            {
                //Reload this turn
                _troopModel.ReloadThisTurn = true;
                _troopModel.ShouldReload = false;

                //Add the shields
                if(!_troopModel.ShieldBroken)
                {
                    _troopModel.ShieldPoints = _troopModel.TroopData.Shield;
                    _troopView.Shield.SetActive(true);
                }

                OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(1));
            }
        }

        _hasMoved = false;
    }

    private void SetTroopActStatus(object sender, EventArgs e)
    {
        //If the sides have switched, check what the current side is and allow all their troops to act
        if (_troopModel.TroopData.Side != _gameLoopManager.CurrentGameSide)
            _troopModel.HasActed = false;
    }

    private void DesaturateTroopInDeployPhase(object sender, EventArgs e)
    {
        //If the game is now in the Deploy Phase, desaturate all troops of that side
        if (_troopModel.TroopData.Side == _gameLoopManager.CurrentGameSide)
            _troopModel.HasActed = true;
    }

    private void ShieldBroken(object sender, EventArgs e)
    {
        //Hide the shield
        _troopView.Shield.SetActive(false);

        //Break the shield
        _troopModel.ShieldBroken = true;

        //Update the Crossbow UI Icon
        OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(1));
    }

    private void OnDestroy()
    {
        _gameLoopManager.RemoveTroop(this);
    }

    public void MoveAfterAttack()
    {
        if(_tileAfterAttack)
        {
            //Set the tile to the player
            _troopModel.NewTile(_tileAfterAttack);

            //Get the worldposition of that tile
            var targetAfterAttack = CoordinatesHelper.DoubleCoordinatesToWorld(_tileAfterAttack.DoubleCoordinates, _tileAfterAttack.Dimensions);

            //Move back to the right pos
            StartCoroutine(_troopView.MoveToPosAfterDelay(targetAfterAttack, _troopModel.TroopData.MovementSpeed / 2, false, null, _troopModel, 0.5f));

            //Reset tile after attack
            _tileAfterAttack = null;
        }
        else
        {
            //If the troop isn't dead and isn't defending rotate back
            if (!_troopModel.IsDead && !_isDefending)
                StartCoroutine(_troopView.DelayedRotateBack(0.5f));//Face the right direction again after the animation

        }
    }

    public void UpdateCrossbowUIOnLoad(int eventArg)
    {
        OnUpdateCrossbowUI?.Invoke(this, new IntEventArgs(eventArg));
    }

    public void HorseAntiHorseFinished()
    {
        //Check if we have the attack horse and defending anti-horse
        if (!_defendingAntiHorse && !_attackingHorse)
        {
            _troopView.RotateBack();

            return;
        }

        //Damage each other
        _attackingHorse.DealDamage(_defendingAntiHorse.TroopData.Damage, false);
        _defendingAntiHorse.DealDamage(_attackingHorse.TroopData.Damage, false);

        var tileToStartFrom = _attackingHorse.GetClosestTileInDirection(_attackingHorse.StartChargeLocation, _defendingAntiHorse.CurrentHexagonModel);
        _attackingHorse.NewTile(tileToStartFrom);

        if (_defendingAntiHorse.IsDead && !_attackingHorse.IsDead)
            _attackingHorse.GetComponent<TroopController>().Move(_defendingAntiHorse.CurrentHexagonModel, false, _defendingAntiHorse);
        else if (!_defendingAntiHorse.IsDead && !_attackingHorse.IsDead)
            StartCoroutine(_attackingHorse.GetComponent<TroopView>().MoveToPos(
                tileToStartFrom.transform.position, _attackingHorse.TroopData.MovementSpeed, false, _defendingAntiHorse, _attackingHorse));

        //Reset the the attacking horse and defending anti-horse
        _defendingAntiHorse = null;
        _attackingHorse = null;
    }
}
