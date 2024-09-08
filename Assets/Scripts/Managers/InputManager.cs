using GameEnum;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput = null;

    private InputAction _touchPositionAction = null;
    private InputAction _touchContactAction = null;

    private GameObject _touchObject = null;
    private TroopCard _selectedCard = null;

    private GameLoopManager _gameLoopManager = null;
    private NextGlowHelper _nextGlowHelper = null;

    private Deck _playerDeck = null;

    private bool _canPerform = true;
    private bool _isDragging = false;
    private bool _hasDragged = false;

    private Vector2 _previousSwipePosition = Vector2.zero;

    private TutorialScriptHandler _tutorial = null;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _nextGlowHelper = FindAnyObjectByType<NextGlowHelper>();
        _tutorial = FindObjectOfType<TutorialScriptHandler>();

        _playerDeck = FindObjectOfType<Deck>();

        _touchPositionAction = _playerInput.actions.FindAction("TouchPosition");
        _touchContactAction = _playerInput.actions.FindAction("TouchContact");
    }

    private void OnEnable()
    {
        _touchPositionAction.performed += OnTouchPositionChanged;
        _touchContactAction.performed += OnTouchContactStarted;
        _touchContactAction.canceled += OnTouchContactEnded;

        AIManager aiManager = FindObjectOfType<AIManager>();
        if (aiManager)
        {
            if (aiManager.IsAiEnabled) _gameLoopManager.OnSwitchSide += UpdateSide;
        }

        _gameLoopManager.OnGameEnd += DisableTouchInputs;
    }

    private void OnDisable()
    {
        _touchPositionAction.performed -= OnTouchPositionChanged;
        _touchContactAction.performed -= OnTouchContactStarted;
        _touchContactAction.canceled -= OnTouchContactEnded;

        AIManager aiManager = FindObjectOfType<AIManager>();
        if (aiManager)
        {
            if (aiManager.IsAiEnabled) _gameLoopManager.OnSwitchSide -= UpdateSide;
        }

        _gameLoopManager.OnGameEnd -= DisableTouchInputs;
    }

    public void ResetTouchObjects()
    {
        // Reset any object we might be interacting with
        if (_touchObject)
        {
            TroopController troopController = _touchObject.GetComponent<TroopController>();
            if (troopController)
            {
                troopController.UnselectTroop();
                _touchObject = null;
            }
        }

        // Reset any card we might have selected
        if (_selectedCard)
        {
            _selectedCard.UnselectCard();
            _selectedCard = null;
        }

        // Rotate the deck back to the center
        if (_gameLoopManager && _playerDeck)
        {
            if (_gameLoopManager.CurrentGameSide == _gameLoopManager.PlayerSide)
            {
                _playerDeck.RotateToCenter();
            }
        }
    }

    private void UpdateSide(object sender, EventArgs e)
    {
        _canPerform = (_gameLoopManager.PlayerSide == _gameLoopManager.CurrentGameSide);
    }

    private void EnableTouchInputs(object sender, GameSideEventArgs e)
    {
        _touchPositionAction.performed += OnTouchPositionChanged;
        _touchContactAction.performed += OnTouchContactStarted;
        _touchContactAction.canceled += OnTouchContactEnded;
    }

    private void DisableTouchInputs(object sender, GameSideEventArgs e)
    {
        _touchPositionAction.performed -= OnTouchPositionChanged;
        _touchContactAction.performed -= OnTouchContactStarted;
        _touchContactAction.canceled -= OnTouchContactEnded;
    }

    #region TapFunctionality
    private void OnTouchTapped()
    {
        // Ignore taps if the gameLoopManager doesn't exists, if we aren't dragging or if we can't perform actions
        if (!_gameLoopManager || _isDragging || !_canPerform) return;

        Ray ray = Camera.main.ScreenPointToRay(_touchPositionAction.ReadValue<Vector2>());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) == false) return;

        switch (_gameLoopManager.CurrentBattlePhase)
        {
            // Handle taps during movement/attack phase
            case BattlePhase.MovementAttackPhase:
                {
                    HandleMovementAttackTaps(hit);
                    break;
                }

            // Handle taps during deployment phase
            case BattlePhase.DeployPhase:
                {
                    HandleDeploymentTaps(hit);
                    break;
                }
        }
    }

    private void HandleMovementAttackTaps(RaycastHit hitInfo)
    {
        TroopModel tappedTroop = hitInfo.transform.GetComponent<TroopModel>();
        TroopController tappedTroopCon = hitInfo.transform.GetComponent<TroopController>();
        HexagonModel tappedTile = hitInfo.transform.GetComponent<HexagonModel>();

        // If we tapped a tile, check if there is a troop on that tile
        if (tappedTile)
        {
            if (tappedTile.Troop)
            {
                tappedTroop = tappedTile.Troop;
                tappedTroopCon = tappedTroop.GetComponent<TroopController>();
            }
        }

        if(_tutorial && tappedTile == _tutorial.TargetHexagon && _tutorial.SelectedTroop)
        {
            _tutorial.SelectedHexagon = tappedTile;
            _tutorial.EnemyTroop = tappedTroopCon;
            _touchObject = null;

            _tutorial.SelectedTroop.GetComponent<TroopController>().UnselectTroop();
        }

        if (_touchObject) // Handle checks if we have an object selected
        {
            // Check if the current object is a troop
            TroopModel currentTroop = _touchObject.GetComponent<TroopModel>();
            TroopController troopController = _touchObject.GetComponent<TroopController>();

            if (!tappedTroop)
            {
                //Don't show the health of all the units
                _gameLoopManager.ShowHealthUnits(false);

                //Reset touchobject
                _touchObject = null;
            }

            if (currentTroop && troopController)
            {
                // Unselect the current troop if it's not a tutorial
                if(!_tutorial)
                {
                    troopController.UnselectTroop();
                }
                else
                {
                    if(_tutorial.TargetHexagon == tappedTile)
                    {
                        troopController.UnselectTroop();
                        _touchObject = null;
                    }
                    else if(_tutorial.TargetHexagon.Troop == tappedTroop && _tutorial.SelectedTroop)
                    {
                        _tutorial.EnemyTroop = tappedTroopCon;
                        _touchObject = null;
                        return;
                    }
                }


                // Check if the troop is different from the current one
                if(_tutorial && _tutorial.TroopTarget == tappedTroopCon)
                {
                    if (tappedTroop.TroopData.Side == currentTroop.TroopData.Side)
                        _gameLoopManager.ShowHealthUnits(true);
                    else
                        _gameLoopManager.ShowHealthUnits(false);

                    // Try to select the new troop
                    if (TrySelectTroop(tappedTroop, tappedTroopCon))
                    {
                        return;
                    }
                }
                else if (!_tutorial && tappedTroop && tappedTroop != currentTroop)
                {
                    if (tappedTroop.TroopData.Side == currentTroop.TroopData.Side)
                        _gameLoopManager.ShowHealthUnits(true);
                    else
                        _gameLoopManager.ShowHealthUnits(false);

                    // Try to select the new troop
                    if (TrySelectTroop(tappedTroop, tappedTroopCon))
                    {
                        return;
                    }
                }
                //If we tapped the same troop
                else if(tappedTroop) 
                {
                    //Don't show the health of all the units
                    _gameLoopManager.ShowHealthUnits(false);

                    _touchObject = null;
                }

                // Try to act with the selected troop --> If we have a tappedtroop and that troop is one of your troops 
                if (troopController && troopController.GetComponent<TroopModel>().TroopData.Side.Equals(_gameLoopManager.PlayerSide) && troopController.TryToActWithTroop(tappedTile, tappedTroop))
                {
                    _gameLoopManager.ShowHealthUnits(false);

                    if (_nextGlowHelper)
                        _nextGlowHelper.CheckToHighlightMoveToDeploy();
                }    

                return;
            }

        }
        else // Handle checks if we don't have an object selected
        {
            // Check if we tapped a troop
            if (tappedTroop)
            {
                //If the troop hasn't acted yet
                if (!tappedTroop.HasActed && tappedTroop.TroopData.Side.Equals(_gameLoopManager.PlayerSide))
                {
                    //Show the health of all the units
                    _gameLoopManager.ShowHealthUnits(true);
                }

                if(_tutorial)
                {
                    //Let the tutorial know
                    if (tappedTroop.TroopData.Side.Equals(_gameLoopManager.PlayerSide) && !_tutorial.SelectedTroop && _tutorial.TroopTarget == tappedTroopCon && _tutorial.Action != null && !_tutorial.Action.IsInstructionOnly)
                        _tutorial.SelectedTroop = tappedTroop;
                    else if(_tutorial.SelectedTroop)
                    {
                        _tutorial.EnemyTroop = tappedTroop.GetComponent<TroopController>();
                        _touchObject = null;
                    }

                    if (_tutorial.TroopTarget == tappedTroopCon && tappedTroop.TroopData.Side.Equals(_gameLoopManager.PlayerSide) && _tutorial.Action != null && !_tutorial.Action.IsInstructionOnly)
                        if (TrySelectTroop(tappedTroop, tappedTroopCon)) return;
                }
                else
                {
                    TrySelectTroop(tappedTroop, tappedTroopCon);
                }
            }
            else if(tappedTile && _tutorial)
            {
                if (!_tutorial.SelectedTroop && tappedTile.Troop == _tutorial.TroopTarget)
                {
                    _tutorial.SelectedTroop = tappedTile.Troop;
                    if (TrySelectTroop(tappedTile.Troop, tappedTile.Troop.GetComponent<TroopController>())) return;
                }
                else if(_tutorial.SelectedTroop)
                {
                    _tutorial.EnemyTroop = tappedTroopCon;
                    _touchObject = null;
                }
            }
        }
    }

    private bool TrySelectTroop(TroopModel troop, TroopController troopCon)
    {
        TroopController prevTroop = null;

        if (_touchObject)
        {
            prevTroop = _touchObject.GetComponent<TroopController>();
            prevTroop.UnselectTroop();
        }

        //Set the new touchobject
        _touchObject = troop.gameObject;

        // Check if the troop is on the correct side and hasn't acted yet
        if (troop.TroopData.Side == _gameLoopManager.CurrentGameSide)
        {
            troopCon.OnTroopSelected();

            return true;
        }

        if(prevTroop && !prevTroop.CanAttackEnemy(prevTroop.GetComponent<TroopModel>(), troop))
        {
            //clicked an enemy
            troopCon.OnEnemyTroopSelected();
        }
        else if(!prevTroop) troopCon.OnEnemyTroopSelected();

        return false;
    }

    private void HandleDeploymentTaps(RaycastHit hitInfo)
    {
        if (_selectedCard) // Handle checks if we have an object selected
        {
            if (!_selectedCard.IsSelected) return;

            // Check if we tapped a tile
            HexagonModel tappedTile = hitInfo.transform.GetComponent<HexagonModel>();
            if (tappedTile)
            {
                if(_tutorial)
                {
                    if(_tutorial.TargetHexagon == tappedTile)
                    {
                        _tutorial.SelectedHexagon = tappedTile;
                        _selectedCard.UnselectCard();
                        _selectedCard = null;
                    }
                }
                else
                {
                    // Try to spawn the troop on the tapped tile
                    if (_selectedCard.TrySpawnTroop(tappedTile, true))
                        return;
                }
            }

            // Unselect the current card if we didn't tap a tile
            if(!_tutorial)
            {
                _selectedCard.UnselectCard();
                _selectedCard = null;
            }

            return;
        }
        else // Handle checks if we don't have an object selected
        {
            // Check if we tapped a card
            TroopCard tappedCard = hitInfo.transform.GetComponent<TroopCard>();
            if (tappedCard)
            {
                //If it's a tutorial
                if(_tutorial)
                {
                    //If it is the right card
                    if(_tutorial.TargetCard == tappedCard && _tutorial.Deployment != null && !_tutorial.Deployment.IsInstructionOnly)
                    {
                        //You can select the card
                        if (TrySelectCard(tappedCard))
                        {
                            _tutorial.SelectedCard = tappedCard;
                            return;
                        }


                    }
                }
                //Else you can always select a card
                else
                    if (TrySelectCard(tappedCard)) return;
            }
        }
    }

    public bool TrySelectCard(TroopCard card, bool alwaysSelect = false)
    {
        // Check if we can select the card
        if (card.IsSelectable() || alwaysSelect)
        {
            StartCoroutine(card.SelectCard());
            _selectedCard = card;

            return true;
        }

        return false;
    }
    #endregion

    #region DragStartFunctionality
    private void OnTouchContactStarted(InputAction.CallbackContext context)
    {
        // Ignore presses if we already are pressing an object or if we can't perform actions
        if (_touchObject || !_canPerform) return;

        Vector2 touchPosition = _touchPositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) == false) return;

        // Check if we clicked a card
        TroopCard card = hit.transform?.GetComponent<TroopCard>();
        if (card)
        {
            // Check that we don't have a card selected
            if (!_selectedCard)
            {
                // Start swiping the deck
                HandleBeginSwiping(touchPosition);
            }
        }
    }

    private void HandleBeginSwiping(Vector3 touchStartPosition)
    {
        if (!_gameLoopManager || !_playerDeck) return;

        if (_gameLoopManager.CurrentGameSide == _gameLoopManager.PlayerSide)
        {
            _previousSwipePosition = touchStartPosition;
            _touchObject = _playerDeck.gameObject;
            _isDragging = true;
            _hasDragged = false;
        }
    }
    #endregion

    #region DragUpdateFunctionality
    private void OnTouchPositionChanged(InputAction.CallbackContext context)
    {
        // Ignore position changes if we aren't pressing an object, if we aren't dragging or if we can't perform actions
        if (!_touchObject || !_isDragging || !_canPerform) return;

        // Check if we are swiping the deck
        Deck deck = _touchObject.GetComponent<Deck>();
        if (deck)
        {
            HandleDeckSwiping(deck);
            return;
        }
    }

    private void HandleDeckSwiping(Deck deck)
    {
        // Calculate the position change compared to previous time
        Vector2 touchPosition = _touchPositionAction.ReadValue<Vector2>();
        Vector2 swipeDelta = touchPosition - _previousSwipePosition;

        // Handle the swiping
        if (deck.HandleSwiping(swipeDelta))
        {
            // Set hasDragged to true if we ever dragged the deck before releasing it
            if (!_hasDragged) _hasDragged = true;
        }

        // Update the previous swipe position
        _previousSwipePosition = touchPosition;
    }
    #endregion

    #region DragEndFunctionality
    private void OnTouchContactEnded(InputAction.CallbackContext context)
    {
        // Ignore releases if we can't perform actions
        if (!_canPerform) return;

        // Handle release as if it were a tap input when we aren't dragging
        if (!_isDragging)
        {
            OnTouchTapped();
            return;
        }

        // Ignore releases if we don't have an object selected
        if (!_touchObject) return;

        // Check if we released the deck
        Deck deck = _touchObject?.GetComponent<Deck>();
        if (deck)
        {
            HandleReleaseDeck(deck);

            // If we haven't dragged the deck, handle the release as a tap as well
            if (!_hasDragged) OnTouchTapped();

            return;
        }
    }

    private void HandleReleaseDeck(Deck deck)
    {
        // End the swiping on the deck
        deck.EndSwiping();

        // Reset current touch object
        _touchObject = null;
        _isDragging = false;
    }
    #endregion
}
