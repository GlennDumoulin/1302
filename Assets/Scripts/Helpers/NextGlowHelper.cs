using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextGlowHelper : MonoBehaviour
{
    [SerializeField]
    private GameObject _buttonGlow = null;

    private GameLoopManager _gameLoopManager = null;
    private List<TroopModel> _playerTroops = new List<TroopModel>();

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _buttonGlow.SetActive(false);

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

    private void UpdateTutorialText(object sender, NextBattlePhaseEventArgs e)
    {
        _buttonGlow.SetActive(false);

        _playerTroops = GetListOfPlayerTroops();
    }

    private void DisplayEndTurnText(object sender, EventArgs e)
    {
        _buttonGlow.SetActive(false);

        if (_gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
            return;

        //If the player cannot deploy any more troops, show the button glow
        _buttonGlow.SetActive(true);
    }

    internal void CheckToHighlightMoveToDeploy()
    {
        if (CheckIfPlayerHasMovedAllTroops())
            _buttonGlow.SetActive(true);
    }

    private List<TroopModel> GetListOfPlayerTroops()
    {
        // Get all troops on the battlefield
        List<TroopController> troops = _gameLoopManager.Troops;

        _playerTroops.Clear();
        List<TroopModel> troopModels = new List<TroopModel>();

        foreach (TroopController controller in troops)
        {
            if (!controller) continue;

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
