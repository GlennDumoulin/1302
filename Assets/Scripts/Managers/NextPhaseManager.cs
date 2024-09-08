using System.Collections.Generic;
using UnityEngine;
using GameEnum;
using UnityEngine.UI;
using System;
using System.Linq;

public class NextPhaseManager : MonoBehaviour
{
    private GameLoopManager _gameLoopManager;
    private bool _isAiPlaying = false;

    private Button _nextPhaseButton = null;
    private Image _sourceImage = null;

    [Header("Source Images")]
    [SerializeField]
    private Sprite _deployTroopSprite;
    [SerializeField]
    private Sprite _endTurnSprite;
    [SerializeField]
    private Sprite _waitTurnSprite;

    [Header("Deploy Sprites")]
    [SerializeField]
    private SpriteState _deploySpriteState;
    [Header("End Turn Sprites")]
    [SerializeField]
    private SpriteState _endSpriteState;
    [Header("Wait Turn Sprites")]
    [SerializeField]
    private SpriteState _waitSpriteState;

    [Header("Turn Banner Sprites")]
    [SerializeField]
    private Image _inactiveBanner;
    [SerializeField]
    private Image _activeBanner;
    [SerializeField]
    private Sprite _frenchBanner;
    [SerializeField]
    private Sprite _flemishBanner;

    [Header("Tweening")]
    [SerializeField]
    private GameObject _nextTurnParent;
    [SerializeField]
    private float _duration = 1f;
    [SerializeField]
    private LeanTweenType _easeType;
    [SerializeField]
    private Color _inactiveColor = Color.gray;

    private Vector3 _baseScale;

    private Deck _playerDeck = null;
    private Deck _enemyDeck = null;

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();

        _playerDeck = FindObjectOfType<Deck>();

        _nextPhaseButton = GetComponent<Button>();
        _sourceImage = GetComponent<Image>();

        _baseScale = _nextTurnParent.transform.localScale;
    }

    private void Start()
    {
        AIManager aiManager = FindObjectOfType<AIManager>();
        if (aiManager) _isAiPlaying = aiManager.IsAiEnabled;

        UpdateButton(this, new NextBattlePhaseEventArgs(_gameLoopManager.CurrentBattlePhase));
    }

    private void OnEnable()
    {
        if (_gameLoopManager)
        {
            _gameLoopManager.OnMoveToDeployPhase += UpdatePlayerDeck;
            _gameLoopManager.OnSwitchSide += UpdatePlayerDeck;
            _gameLoopManager.OnSwitchSide += ScaleNextPhaseButton;
            _gameLoopManager.OnPhaseAdvanced += UpdateButton;
            _gameLoopManager.OnDataLoaded += UpdateButton;
        }
    }

    private void OnDisable()
    {
        if (_gameLoopManager)
        {
            _gameLoopManager.OnMoveToDeployPhase -= UpdatePlayerDeck;
            _gameLoopManager.OnSwitchSide -= UpdatePlayerDeck;
            _gameLoopManager.OnSwitchSide -= ScaleNextPhaseButton;
            _gameLoopManager.OnPhaseAdvanced -= UpdateButton;
            _gameLoopManager.OnDataLoaded -= UpdateButton;
        }
    }

    private void UpdateButton(BattlePhase battlePhase)
    {
        //Check if the AI is playing. If so, prevent the player from clicking the button while the AI acts
        if (_isAiPlaying && _gameLoopManager.CurrentGameSide != _gameLoopManager.PlayerSide)
        {
            _sourceImage.color = _inactiveColor;
            _nextPhaseButton.interactable = false;
            _sourceImage.sprite = _waitTurnSprite;
            _nextPhaseButton.spriteState = _waitSpriteState;
            SetBanners();
            return;
        }

        //If the Player is playing, set the button back to Interactable
        _nextPhaseButton.interactable = true;
        _sourceImage.color = Color.white;

        //Update the Button Sprite to display what the next phase of the battle will be
        if (battlePhase == BattlePhase.DeployPhase)
        {
            _sourceImage.sprite = _endTurnSprite;
            _nextPhaseButton.spriteState = _endSpriteState;
        }
        else if (battlePhase == BattlePhase.MovementAttackPhase)
        {
            _sourceImage.sprite = _deployTroopSprite;
            _nextPhaseButton.spriteState = _deploySpriteState;
        }

        SetBanners();
    }

    private void UpdateButton(object sender, NextBattlePhaseEventArgs e)
    {
        UpdateButton(e.NextPhase);
    }

    private void UpdateButton(object sender, DataEventArgs e)
    {
        UpdateButton(e.CurrentBattlePhase);
    }

    private void UpdatePlayerDeck(object sender, EventArgs e)
    {
        if (!_gameLoopManager || !_playerDeck || !_enemyDeck) return;

        bool isPlayerTurn = (_gameLoopManager.CurrentGameSide == _gameLoopManager.PlayerSide);

        //Update the Player Deck based on what phase the battle is now in
        if (_gameLoopManager.CurrentBattlePhase == BattlePhase.DeployPhase)
        {
            _playerDeck.SetVisibility(isPlayerTurn ? DeckVisibility.FullVisible : DeckVisibility.Hidden);
            if (!_isAiPlaying) _enemyDeck.SetVisibility(isPlayerTurn ? DeckVisibility.Hidden : DeckVisibility.FullVisible);
        }
        else if (_gameLoopManager.CurrentBattlePhase == BattlePhase.MovementAttackPhase)
        {
            _playerDeck.SetVisibility(DeckVisibility.Hidden);
            if (!_isAiPlaying) _enemyDeck.SetVisibility(DeckVisibility.Hidden);
        }
    }

    private void SetBanners()
    {
        if (_gameLoopManager.CurrentGameSide == GameSides.Flemish)
        {
            _activeBanner.sprite = _flemishBanner;
            _inactiveBanner.sprite = _frenchBanner;
        } else
        {
            _activeBanner.sprite = _frenchBanner;
            _inactiveBanner.sprite = _flemishBanner;
        }
    }

    private void ScaleNextPhaseButton(object sender, EventArgs e)
    {
        LeanTween.cancel(_nextTurnParent);
        LeanTween.scale(_nextTurnParent, _baseScale * 1.25f, _duration / 2f).setEase(_easeType).setOnComplete(ScaleDown);
    }

    private void ScaleDown()
    {
        LeanTween.scale(_nextTurnParent, _baseScale, _duration / 2f).setEase(_easeType);
    }
}
