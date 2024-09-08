using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameEnum;

public class Deck : MonoBehaviour
{
    DeckScriptableObject _deck = null;

    [SerializeField] private Transform _deckSocket = null;

    private TroopCard _currentSelectedTroopCard = null;

    public TroopCard CurrentSelectedTroopCard
    {
        get { return _currentSelectedTroopCard; }
        set { _currentSelectedTroopCard = value; }
    }

    private List<TroopCard> _troopCards = new List<TroopCard>();
    public int CardsRemaining
    {
        get { return _troopCards.Count; }
    }

    private Transform _troopParentSocket = null;
    private CameraRectHelper _cameraRectHelper = null;

    private int _middleIdx = -1;
    private const float _rotationOffset = 5.0f;
    private const float _heightOffset = -0.05f;

    private Vector3 _fullVisiblePosition = Vector3.zero;
    private Vector3 _lowVisiblePosition = Vector3.zero;
    private Vector3 _hiddenPosition = Vector3.zero;

    private Vector3 _targetPosition = Vector3.zero;
    private Quaternion _targetRotation = Quaternion.identity;

    private const float _moveSpeed = 5.0f;
    private const float _rotateSpeed = 5.0f;
    private const float _maxSwipeSpeed = 0.75f;

    private const float _swipeDeadzoneAngle = 50.0f;
    private const float _swipeMaxMagnitude = 20.0f;

    private float _swipeMinYEulers = 0.0f;
    private float _swipeMaxYEulers = 0.0f;

    private float _currentSwipeSpeed = 0.0f;

    private void Awake()
    {
        _cameraRectHelper = FindObjectOfType<CameraRectHelper>();

        // Get the parent socket for spawned troops
        GameObject troopSocket = GameObject.Find("Units/Troops");
        if (troopSocket) _troopParentSocket = troopSocket.transform;

        TutorialScriptHandler tutorialScriptHandler = FindObjectOfType<TutorialScriptHandler>();
        if (!tutorialScriptHandler)
        {
            // Get the Deck
            _deck = StaticDataHelper.playerDeck;
            if (!_deck) return;

            // Add all cards to the deck
            var cards = _deck.Deck;
            foreach (GameObject card in cards)
            {
                AddCard(card);
            }
        }

        // Set visibility positions
        _fullVisiblePosition = transform.position - _cameraRectHelper.GetChangeInCameraPosition();
        _lowVisiblePosition = _fullVisiblePosition + (Vector3.back * 3.0f);
        _hiddenPosition = _fullVisiblePosition + (Vector3.back * 4.5f);

        // Make sure the deck is set to hidden
        transform.position = _hiddenPosition;
    }

    public GameObject AddCard(GameObject cardToAdd)
    {
        if (!cardToAdd) return null;

        // Create an instance of the card
        GameObject card = Instantiate(cardToAdd);

        // Check if the item we are adding is a card and isn't in the list already
        TroopCard troopCard = card.GetComponent<TroopCard>();
        if (!troopCard || _troopCards.Contains(troopCard))
        {
            Destroy(card);
            return null;
        }

        // Set parent socket for spawned troop
        troopCard.TroopParentSocket = _troopParentSocket;

        // Add a card to the deck
        _troopCards.Add(troopCard);

        // Add a socket for the added card
        AddCardSocket(card);

        // Assign this deck to the card
        troopCard.SetDeck(this, _troopCards.Count - 1);

        // Set card transform
        card.transform.localPosition = Vector3.back * _deckSocket.localPosition.z;

        UpdateDeck();

        return card;
    }
    private void AddCardSocket(GameObject card)
    {
        // Create an empty game object as rotation socket
        GameObject parent = new GameObject("CardRotationSocket");

        // Add the card to it's rotation socket
        card.transform.SetParent(parent.transform, true);

        // Add the socket to the deck
        parent.transform.SetParent(_deckSocket, true);
    }

    public void ReAddCard(TroopCard card, int idx)
    {
        if (idx == -1) return;

        // Check if the card isn't in the list already
        if (_troopCards.Contains(card)) return;

        // Check if the given index doesn't go out of range
        if (_troopCards.Capacity > idx)
        {
            // Insert card back into the deck
            _troopCards.Insert(idx, card);
        }
        else
        {
            // Add card to end of the deck
            _troopCards.Add(card);
        }

        // Add a socket for the added card
        AddCardSocket(card.gameObject);

        // Set card transform
        card.transform.localPosition = Vector3.back * _deckSocket.localPosition.z;

        UpdateDeck();
    }

    public void RemoveCard(TroopCard cardToRemove)
    {
        // Check if the card is in the list already
        if (!_troopCards.Contains(cardToRemove)) return;

        // Remove a card from the deck
        _troopCards.Remove(cardToRemove);

        // Detach card from parent
        GameObject parentObject = cardToRemove.transform.parent.gameObject;
        cardToRemove.transform.parent = null;
        Destroy(parentObject);

        // Set local position to world position and reset rotation
        cardToRemove.transform.localPosition = cardToRemove.transform.position;
        cardToRemove.transform.localEulerAngles = cardToRemove.transform.eulerAngles;

        UpdateDeck();
    }

    private void DeleteCard(TroopCard cardToDestroy, bool removeFromList = true)
    {
        // Check if the card is actually in the list
        if (!_troopCards.Contains(cardToDestroy)) return;

        if (removeFromList)
        {
            _troopCards.Remove(cardToDestroy);
            UpdateIndexes();
            UpdateDeck();
        }

        Destroy(cardToDestroy.transform.parent.gameObject);
        Destroy(cardToDestroy.gameObject);
    }

    public void ClearDeck()
    {
        // Remove all cards from the deck
        foreach (TroopCard card in _troopCards)
        {
            DeleteCard(card, false);
        }
        _troopCards.RemoveRange(0, _troopCards.Count);
        _troopCards.Clear();

        UpdateDeck();
    }

    public int GetCardIdx(TroopCard card)
    {
        return _troopCards.IndexOf(card);
    }

    private void UpdateDeck(int middleIdx = int.MinValue)
    {
        // Set selected index of the deck
        bool isValidIdx = middleIdx >= 0 && middleIdx < _troopCards.Count;
        if (isValidIdx)
        {
            _middleIdx = middleIdx;
        }
        else
        {
            _middleIdx = _troopCards.Count / 2;
        }

        // Set rotation and height for all cards in the deck
        foreach (TroopCard card in _troopCards)
        {
            int cardIdx = GetCardIdx(card);
            int distanceFromSelectedIdx = cardIdx - _middleIdx;

            // Set parent transform
            Transform parent = card.transform.parent;
            Vector3 cardRotation = Vector3.up * distanceFromSelectedIdx * _rotationOffset;

            parent.localEulerAngles = cardRotation;
            parent.localPosition = Vector3.up * Mathf.Abs(distanceFromSelectedIdx) * _heightOffset;

            // Reset card rotation
            card.transform.localRotation = Quaternion.identity;

            // Set the min and max y rotations
            if (_troopCards.Count > 0)
            {
                if (cardIdx == 0) _swipeMaxYEulers = -(cardRotation.y - (_rotationOffset / 2));
                if (cardIdx == _troopCards.Count - 1) _swipeMinYEulers = -(cardRotation.y + (_rotationOffset / 2));
            }
        }
    }

    public void UpdateIndexes()
    {
        foreach (TroopCard card in _troopCards)
        {
            card.UpdateDeckIdx(GetCardIdx(card));
        }
    }

    public void CheckSelectedCard()
    {
        foreach (TroopCard card in _troopCards)
        {
            if (card.IsSelected)
            {
                InputManager inputManager = FindObjectOfType<InputManager>();
                if (inputManager)
                {
                    inputManager.TrySelectCard(card, true);
                }

                return;
            }
        }
    }

    public void SetVisibility(DeckVisibility visibilityState)
    {
        StopCoroutine("MoveToTarget");

        //Unselect card
        if (CurrentSelectedTroopCard)
        {
            CurrentSelectedTroopCard = null;

            var input = FindObjectOfType<InputManager>();

            input.ResetTouchObjects();
        }


        switch (visibilityState)
        {
            case DeckVisibility.Hidden:
                {
                    _targetPosition = _hiddenPosition;
                    break;
                }
            case DeckVisibility.LowVisible:
                {
                    _targetPosition = _lowVisiblePosition;
                    break;
                }
            case DeckVisibility.FullVisible:
                {
                    _targetPosition = _fullVisiblePosition;
                    break;
                }
            default: return;
        }

        StartCoroutine("MoveToTarget");
    }

    private IEnumerator MoveToTarget()
    {
        while (_targetPosition != transform.position)
        {
            // Lerp towards target position
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void SetTargetRotation(Vector3 targetRotation)
    {
        // Handle setting the target rotation and starting the coroutine
        _targetRotation = Quaternion.Euler(targetRotation);
        StartCoroutine("RotateToTarget");
    }

    public bool HandleSwiping(Vector2 swipeDelta)
    {
        if (!_deckSocket) return false;

        // Get the magnitude of the swipe update
        float swipeMagnitude = swipeDelta.magnitude;

        // Check if we moved a minimum amount since the previous update
        if (swipeMagnitude < float.Epsilon)
        {
            _currentSwipeSpeed = 0.0f;
            return false;
        }

        // Get the direction in which we are swiping
        float swipeDirectionAngle = Vector2.SignedAngle(Vector2.up, swipeDelta.normalized);
        Vector3 swipeDirection = Vector3.zero;

        if (swipeDirectionAngle > _swipeDeadzoneAngle && swipeDirectionAngle < (180.0f - _swipeDeadzoneAngle))
        {
            // Swipe Right
            swipeDirection = Vector3.down;
        }
        else if (swipeDirectionAngle < -_swipeDeadzoneAngle && swipeDirectionAngle > (-180.0f + _swipeDeadzoneAngle))
        {
            // Swipe Left
            swipeDirection = Vector3.up;
        }

        // Check if we aren't swiping in the deadzone
        if (swipeDirection == Vector3.zero)
        {
            _currentSwipeSpeed = 0.0f;
            return false;
        }

        // Calculate the swipe speed based on the swipe magnitude
        float swipeSpeedLerpValue = swipeMagnitude / _swipeMaxMagnitude;
        float swipeSpeed = _maxSwipeSpeed * swipeSpeedLerpValue;

        if (_currentSwipeSpeed != swipeSpeed)
            _currentSwipeSpeed = Mathf.Lerp(_currentSwipeSpeed, swipeSpeed, 0.25f);

        Vector3 targetRotation = _deckSocket.eulerAngles + (swipeDirection * _currentSwipeSpeed);

        // Check if the targetRotation is valid
        float rotationToCheck = (targetRotation.y > 180.0f) ? targetRotation.y - 360.0f : targetRotation.y;
        if (rotationToCheck < _swipeMinYEulers || rotationToCheck > _swipeMaxYEulers) return false;

        SetTargetRotation(targetRotation);

        return true;
    }

    public void EndSwiping()
    {
        StopCoroutine("RotateToTarget");

        // Reset the current swipe speed
        _currentSwipeSpeed = 0.0f;

        // Calculate the new middle index
        int middleIdx = CalculateMiddleIdx();

        // Check if we have selected a new index
        if (middleIdx != _middleIdx)
        {
            int indexesMoved = middleIdx - _middleIdx;
            _deckSocket.Rotate(Vector3.up, indexesMoved * _rotationOffset);

            // Update the deck
            UpdateDeck(middleIdx);
        }

        // Reset the deck's rotation
        SetTargetRotation(Vector3.zero);
    }

    private int CalculateMiddleIdx()
    {
        float yRotation = _deckSocket.eulerAngles.y;
        if (yRotation > 180.0f) yRotation -= 360.0f;

        int middleIdx = _middleIdx - Mathf.RoundToInt(yRotation / _rotationOffset);
        int maxIdx = (_troopCards.Count > 0) ? (_troopCards.Count - 1) : 0;

        return Mathf.Clamp(middleIdx, 0, maxIdx);
    }

    private IEnumerator RotateToTarget()
    {
        if (!_deckSocket) yield break;

        while (_targetRotation.eulerAngles != _deckSocket.eulerAngles)
        {
            // Lerp towards target position
            _deckSocket.rotation = Quaternion.Slerp(_deckSocket.rotation, _targetRotation, _rotateSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public int RotateCardToMiddle(TroopCard card)
    {
        // Check if the given card is in this deck
        int cardIdx = GetCardIdx(card);
        if (cardIdx != -1)
        {
            // Get the distance in indexes to the new card
            int distanceToIdx = cardIdx - _middleIdx;

            // Store card's euler angles
            Vector3 cardEulers = card.transform.eulerAngles;

            // Update the deck positions
            UpdateDeck(cardIdx);

            // Rotate the deck
            _deckSocket.eulerAngles = cardEulers;
            SetTargetRotation(Vector3.zero);

            return Mathf.Abs(distanceToIdx);
        }

        return -1;
    }

    public void RotateToCenter()
    {
        if (CardsRemaining == 0) return;

        // Get the middle index of the deck
        int middleIdx = _troopCards.Count / 2;

        // Get the card at the middle index and rotate the deck to that card
        TroopCard middleCard = _troopCards.ElementAt(middleIdx);
        if (middleCard) RotateCardToMiddle(middleCard);
    }
}
