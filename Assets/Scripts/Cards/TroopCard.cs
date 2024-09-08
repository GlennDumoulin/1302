using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;
using TMPro;
using System.Linq;

public class TroopCard : MonoBehaviour, IDataPersistance<CardData>
{
    [SerializeField] private string _assetKey = string.Empty;
    [SerializeField] private GameObject _troopToSpawn = null;
    [SerializeField] private TextMeshPro _healthText = null;
    [SerializeField] private TextMeshPro _damageText = null;
    [SerializeField] private int _manpowerCost = 1;
    [SerializeField] private MeshRenderer _outlineRenderer = null;
    [SerializeField] private Material _outlineMaterial = null;

    private Material _defaultMaterial = null;

    public GameSides CardSide { get; private set; } = GameSides.Flemish;
    public int ManpowerCost { get { return _manpowerCost; } }

    private const float _dragFollowSpeed = 5.0f;
    private const float _rotateSpeed = 5.0f;
    private const float _scaleSpeed = 5.0f;

    private Vector3 _targetPosition = Vector3.zero;
    private Vector3 _targetLocalPosition = Vector3.zero;
    private Quaternion _targetRotation = Quaternion.identity;
    private Vector3 _targetScale = Vector3.one;

    private Vector3 _previousPosition = Vector3.zero;
    private Quaternion _previousRotation = Quaternion.identity;

    private Vector3 _defaultLocalPosition = Vector3.zero;
    private Vector3 _selectedLocalPosition = Vector3.zero;

    private Vector3 _targetedScale = new Vector3(0.7f, 0.7f, 0.7f);
    private Vector3 _selectedScale = new Vector3(1.1f, 1.1f, 1.1f);

    private HexagonModel _selectedTileModel = null;
    private GridModel _gridModel = null;
    private GameLoopManager _gameLoopManager = null;

    private Deck _deck = null;
    private int _deckIdx = -1;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get { return _isSelected; }
    }

    public Transform TroopParentSocket { get; set; } = null;

    private int _tutorialIdx = -1;
    public int TutorialIdx
    {
        get { return _tutorialIdx; }
        set { _tutorialIdx = value; }
    }

    private void Awake()
    {
        _gridModel = FindObjectOfType<GridModel>();
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
    }
    private void Start()
    {
        // Set the data on the card
        if (_troopToSpawn && _healthText && _damageText)
        {
            TroopModel troop = _troopToSpawn.GetComponent<TroopModel>();
            if (troop)
            {
                CardSide = troop.TroopData.Side;
                _healthText.text = troop.TroopData.Health.ToString();
                _damageText.text = troop.TroopData.Damage.ToString();
            }
        }

        // Set the (un)selected position
        _defaultLocalPosition = transform.localPosition;
        _selectedLocalPosition = _defaultLocalPosition + new Vector3(0.0f, 0.5f, 0.75f);

        //Get the default material
        _defaultMaterial = _outlineRenderer.material;
    }

    public void SetDeck(Deck deck, int idx)
    {
        if (_deck != null) return;

        _deck = deck;
        UpdateDeckIdx(idx);
    }
    public void UpdateDeckIdx(int idx)
    {
        _deckIdx = idx;
    }

    public void EnableTargeting(Vector3 targetPosition)
    {
        // Unselect the card
        UnselectCard(false);

        // Handle deck changes
        if (_deck)
        {
            // Remove targeted card from assigned deck
            _deck.RemoveCard(this);
        }

        // Store previous transform values
        _previousPosition = transform.position;
        _previousRotation = transform.rotation;

        // Set target
        SetTargetPosition(targetPosition);
        SetTargetRotation(Quaternion.identity);
        SetTargetScale(_targetedScale);
    }

    public void DisableTargeting()
    {
        StopCoroutine("MoveToTarget");
        StopCoroutine("RotateToTarget");
        StopCoroutine("ScaleToTarget");

        // Remove highlight start row
        _gridModel.UnHighlightStartRow();

        // Check if we released the card on a grid tile
        Ray ray = new Ray(Camera.main.transform.position, _targetPosition - Camera.main.transform.position);
        RaycastHit hit;

        // Create a layermask to only check for grid tiles
        LayerMask layerMask = LayerMask.GetMask("GridTiles");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) == true)
        {
            // Get the tile we released the card on
            HexagonModel tileModel = hit.transform.GetComponent<HexagonModel>();
            if (tileModel != null)
            {
                if (TrySpawnTroop(tileModel)) return;
            }
        }

        // Handle deck changes
        if (_deck)
        {
            // Set card's scale back to previous value
            SetTargetScale(Vector3.one);

            // Add targeted card to assigned deck
            _deck.ReAddCard(this, _deckIdx);

            // Set deck visibility to full visible
            _deck.SetVisibility(DeckVisibility.FullVisible);
        }
        else
        {
            // Set card's transform back to previous values
            SetTargetPosition(_previousPosition);
            SetTargetRotation(_previousRotation);
        }
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        StartCoroutine("MoveToTarget");
    }
    public void SetTargetLocalPosition(Vector3 targetLocalPosition)
    {
        _targetLocalPosition = targetLocalPosition;
        StartCoroutine("MoveLocalToTarget");
    }
    public void SetTargetRotation(Quaternion targetRotation)
    {
        _targetRotation = targetRotation;
        StartCoroutine("RotateToTarget");
    }
    public void SetTargetScale(Vector3 targetScale)
    {
        _targetScale = targetScale;
        StartCoroutine("ScaleToTarget");
    }

    private IEnumerator MoveToTarget()
    {
        while (_targetPosition != transform.position)
        {
            // Lerp towards target position
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _dragFollowSpeed * Time.deltaTime);

            yield return null;
        }
    }
    private IEnumerator MoveLocalToTarget()
    {
        while (_targetLocalPosition != transform.localPosition)
        {
            // Lerp towards target local position
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPosition, _dragFollowSpeed * Time.deltaTime);

            yield return null;
        }
    }
    private IEnumerator RotateToTarget()
    {
        while (_targetRotation.eulerAngles != transform.eulerAngles)
        {
            // Lerp towards target position
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _rotateSpeed * Time.deltaTime);

            yield return null;
        }
    }
    private IEnumerator ScaleToTarget()
    {
        while (_targetScale != transform.localScale)
        {
            // Lerp towards target scale
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, _scaleSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public bool TrySpawnTroop(HexagonModel tileModel, bool isTriggeredByTap = false)
    {
        // Check if we have a troop to spawn
        if (!_troopToSpawn) return false;

        // Check if the tile we released the card on is on the start row and there is no troop on there already
        if (tileModel.IsValidStartTile(CardSide))
        {
            // Check if this got triggered by a tap
            if (isTriggeredByTap) EnableTargeting(tileModel.transform.position);
            else SetTargetPosition(tileModel.transform.position);

            // Remove highlight start row
            _gridModel.UnHighlightStartRow();

            //Set the selected tile model
            _selectedTileModel = tileModel;

            // Spawn the troop
            Invoke("SpawnTroop", isTriggeredByTap ? 0.5f : 0.2f);

            return true;
        }

        return false;
    }

    private void SpawnTroop()
    {
        // Check if we have a tile to spawn the troop on
        if (!_selectedTileModel) return;

        // Spawn the troop at the card's position
        GameObject troop = Instantiate(_troopToSpawn, _targetPosition, Quaternion.identity);
        TroopModel troopModel = troop.GetComponent<TroopModel>();

        // Set troop to tile and tile to troop
        _selectedTileModel.Troop = troopModel;
        troopModel.CurrentHexagonModel = _selectedTileModel;

        // Attach troop to it's parent socket
        if (TroopParentSocket)
            troop.transform.parent = TroopParentSocket;

        // Set the tutorial index
        if (TutorialIdx != -1)
            troopModel.TutorialIdx = TutorialIdx;

        // Handle gameloop & deck changes
        if (_deck && _gameLoopManager)
        {
            //Update how many troops have been deployed
            _gameLoopManager.UpdateDeployCount();

            // Set deck visibility to full visible
            if (_gameLoopManager.CanDeployTroop())
                _deck.SetVisibility(DeckVisibility.FullVisible);
            //Let the tactical view know
            else  FindObjectOfType<TacticalView>().EndOfDeploment();

            // Update deck indexes
            _deck.UpdateIndexes();
        }

        // Destroy the card
        Destroy(gameObject);
    }

    public bool IsSelectable()
    {
        if (!_gameLoopManager || !_deck || _isSelected) return false;

        return CardSide == _gameLoopManager.CurrentGameSide && _gameLoopManager.CanDeployTroop();
    }

    public IEnumerator SelectCard()
    {
        if (!_deck) yield break;

        // Rotate the card's deck to center this card
        int distanceToIdx = _deck.RotateCardToMiddle(this);

        const int maxDistance = 10;
        const float maxTime = 1.0f;
        float timeScaleFactor = (maxTime / maxDistance) * Mathf.Clamp(distanceToIdx, 0, maxDistance);

        // Select the card after a bit
        yield return new WaitForSeconds(maxTime * timeScaleFactor);

        _isSelected = true;

        // Move and scale card to selected values
        SetTargetLocalPosition(_selectedLocalPosition);
        SetTargetScale(_selectedScale);

        // Set deck visibility to low visible
        _deck.SetVisibility(DeckVisibility.LowVisible);

        // Highlight start row
        _gridModel.HighlightStartRow(CardSide);

        //Update the current selected card
        _deck.CurrentSelectedTroopCard = this;
    }

    public void UnselectCard(bool setDeckVisible = true)
    {
        StopCoroutine("SelectCard");
        StopCoroutine("MoveToTarget");
        StopCoroutine("MoveLocalToTarget");
        StopCoroutine("RotateToTarget");
        StopCoroutine("ScaleToTarget");

        _isSelected = false;

        // Reset scale to default
        SetTargetScale(Vector3.one);

        // Ignore deck changes if we don't have a deck
        if (!_deck) return;

        //Update the current selected card
        _deck.CurrentSelectedTroopCard = null;

        if (setDeckVisible)
        {
            // Reset local position to previous value
            SetTargetLocalPosition(_defaultLocalPosition);

            //Get the tactical view 
            var tacView = FindObjectOfType<TacticalView>();

            //Reset the deck
            if (tacView)
            {
                _deck.SetVisibility(tacView.InTacticalView ? DeckVisibility.LowVisible : DeckVisibility.FullVisible);
            }

            // Remove highlight start row
            _gridModel.UnHighlightStartRow();
        }
    }

    public List<HexagonModel> GetValidDeployTiles()
    {
        List<HexagonModel> validDeployTiles = new List<HexagonModel>();

        if (_gridModel && _gameLoopManager)
        {
            bool isPlayerTurn = (_gameLoopManager.CurrentGameSide == _gameLoopManager.PlayerSide);
            validDeployTiles = _gridModel.GetStartRow(isPlayerTurn).Where(hexa => hexa.Troop == null).ToList();
        }

        return validDeployTiles;
    }

    public void LoadData(CardData data)
    {
        CardSide = (GameSides)data.CardSide;

        _isSelected = data.IsSelected;
    }

    public void SaveData(ref CardData data)
    {
        data.CardSide = (int)CardSide;

        data.IsSelected = _isSelected;

        data.AssetKey = _assetKey;
    }

    public void SetOutline(bool outline)
    {
        if (outline)
            _outlineRenderer.material = _outlineMaterial;
        else _outlineRenderer.material = _defaultMaterial;
    }
}
