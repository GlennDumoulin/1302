using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTextManager : MonoBehaviour
{
    [Header("Button Glows")]
    [SerializeField]
    private GameObject _buttonGlow = null;
    [SerializeField]
    private GameObject _campfireGlow = null;
    [SerializeField]
    private GameObject _tacticalGlow = null;

    private GameLoopManager _gameLoopManager = null;

    private List<TroopModel> _playerTroops = new List<TroopModel>();

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
    }

    private void OnEnable()
    {
        _gameLoopManager.OnPhaseAdvanced += UpdateTutorialText;
        _gameLoopManager.OnReachDeployLimit += DisplayEndTurnText;
    }

    private void OnDisable()
    {
        _gameLoopManager.OnPhaseAdvanced -= UpdateTutorialText;
        _gameLoopManager.OnReachDeployLimit -= DisplayEndTurnText;
    }

    private void Start()
    {
        SetAllToInactive();
    }

    private void UpdateTutorialText(object sender, NextBattlePhaseEventArgs e)
    {
        SetAllToInactive();

        _playerTroops = GetListOfPlayerTroops();
    }

    public void DisplayEndTurnText(object sender, EventArgs e)
    {
        SetAllToInactive();

        if (_gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
            return;

        //If the player cannot deploy any more troops, show the button glow
        _buttonGlow.SetActive(true);
    }

    public void DisplayMoveToDeploy()
    {
        SetAllToInactive();

        if (_gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
            return;

        //If the player has moved all troops, enable the button glow
        if (CheckIfPlayerHasMovedAllTroops())
            _buttonGlow.SetActive(true);
    }

    private void SetAllToInactive()
    {
        _buttonGlow.SetActive(false);
        _campfireGlow.SetActive(false);
        _tacticalGlow.SetActive(false);
    }

    private List<TroopModel> GetListOfPlayerTroops()
    {
        // Get all troops on the battlefield
        List<TroopController> troops = _gameLoopManager.Troops;

        _playerTroops.Clear();
        List<TroopModel> troopModels = new List<TroopModel>();

        foreach (TroopController controller in troops)
        {
            if(!controller ) continue;

            TroopModel model = controller.GetComponent<TroopModel>();
            if (model && model.TroopData.Side == _gameLoopManager.PlayerSide)
                troopModels.Add(model);
        }

        return troopModels;
    }

    private bool CheckIfPlayerHasMovedAllTroops()
    {
        //Check each Player troop, if they have not acted, return false
        foreach (var model in _playerTroops)
            if (!model.HasActed)
                return false;

        return true;
    }

}
