using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TacticalView : MonoBehaviour
{
    [SerializeField] private Deck _playerDeck = null;

    [SerializeField] private GameLoopManager  _gameLoopManager = null;

    [SerializeField] private Sprite _viewDown = null;
    [SerializeField] private Sprite _viewUp = null;

    [SerializeField] private TextMeshProUGUI _deploysLeft = null;
    [SerializeField] private TextMeshProUGUI _deploysMax = null;

    private Image _tacticalViewImage = null;

    private bool _inTacticalView = false;

    private bool _isAIPlaying = false;

    public bool InTacticalView
    {
        get { return _inTacticalView; }
    }

    private int _maxDeploys = 1;

    // Start is called before the first frame update
    private void Start()
    {
        _tacticalViewImage = GetComponent<Image>();

        _gameLoopManager.OnMoveToDeployPhase += StartDeployment;

        var AI = FindObjectOfType<AIManager>();
        if (AI) _isAIPlaying = AI.IsAiEnabled;

        if(_deploysMax)
            _deploysMax.text = _maxDeploys.ToString();

        if (_deploysLeft)
            _deploysLeft.text = _maxDeploys.ToString();
    }

    private void OnEnable()
    {
        _gameLoopManager.OnUnitDeployed += UpdateCounter;
        _gameLoopManager.OnSwitchSide += SwitchedSide;
    }
    private void OnDisable()
    {
        _gameLoopManager.OnUnitDeployed -= UpdateCounter;
        _gameLoopManager.OnSwitchSide -= SwitchedSide;
    }

    private void UpdateCounter(object sender, EventArgs e)
    {
        if(_deploysLeft && _gameLoopManager.CurrentGameSide.Equals(_gameLoopManager.PlayerSide))
            //Update the deploycounter
            _deploysLeft.text = (_maxDeploys - _gameLoopManager.TotalTroopsDeployed).ToString();
    }

    private void SwitchedSide(object sender, EventArgs e)
    {
        _maxDeploys = 2;

        if(_deploysMax)
            _deploysMax.text = _maxDeploys.ToString();

        if (_deploysLeft)
            _deploysLeft.text = _maxDeploys.ToString();
    }

    //Gets called when the tacticalviewbutton is pressed
    public void ToggleTacticalView()
    {
        //Update the inTacticalView
        _inTacticalView = !_inTacticalView;

        //Handle everything that needs to be handled
        HandleTacticalView();
    }

    public void StartDeployment(object sender, EventArgs e)
    {
        if (_gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
            return;

        //Reset tactical view
        _inTacticalView = false;

        //Handle everything that needs to be handled
        HandleTacticalView();
    }

    public void StartTurn()
    {
        //Reset tactical view
        _inTacticalView = true;

        //Handle everything that needs to be handled
        HandleTacticalView();
    }

    public void EndOfDeploment()
    {
        //Go to tactical view
        _inTacticalView = true;

        //Handle everything that needs to be handled
        HandleTacticalView();
    }

    private void HandleTacticalView()
    {
        //Update the UI buttons
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_inTacticalView)
        {
            //Check if we are in battle or deploy phase
            if (_gameLoopManager.CurrentBattlePhase.Equals(GameEnum.BattlePhase.DeployPhase))
            {
                //Show deck partially in deployphase
                _playerDeck.SetVisibility(GameEnum.DeckVisibility.LowVisible);
            }
            else //Show deck fully in deployphase
                _playerDeck.SetVisibility(GameEnum.DeckVisibility.Hidden);

            //Update the sprite
            _tacticalViewImage.sprite = _viewUp;
        } 
        else
        {
            //Check if we are in battle or deploy phase
            if (_gameLoopManager.CurrentBattlePhase.Equals(GameEnum.BattlePhase.DeployPhase))
            {
                //Show deck partially in deployphase
                _playerDeck.SetVisibility(GameEnum.DeckVisibility.FullVisible);
            }
            else //Show deck fully in deployphase
                _playerDeck.SetVisibility(GameEnum.DeckVisibility.LowVisible);

            //Update the sprite
            _tacticalViewImage.sprite = _viewDown;
        }
    }
}
