using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameEnum;

public class GridModel : MonoBehaviour
{
    private List<HexagonModel> _hexagonModels = new List<HexagonModel>();
    public List<HexagonModel> HexagonModels
    {
        get { return _hexagonModels; }
        set { _hexagonModels = value; }
    }

    private Dictionary<Vector2, HexagonModel> _doubleCoordinateToHexagon = new Dictionary<Vector2, HexagonModel>();
    public Dictionary<Vector2, HexagonModel> DoubleCoordinateToHexagon
    {
        get { return _doubleCoordinateToHexagon; }
    }

    private Dictionary<HexagonModel, HexagonView> _hexagons = new Dictionary<HexagonModel, HexagonView>();

    private List<HexagonModel> _highlightedTiles = new List<HexagonModel>();

    [SerializeField] private GridController _gridController = null;

    private GameLoopManager _gameManager = null;
    private CampfireManager _campfireManager = null;

    private List<HexagonModel> _playerStartTiles = new List<HexagonModel>();
    private List<HexagonModel> _aiStartTiles = new List<HexagonModel>();

    public event EventHandler OnGridCreated;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameLoopManager>();
        _campfireManager = FindObjectOfType<CampfireManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get all the hexagonmodel and views
        _hexagonModels = FindObjectsOfType<HexagonModel>().ToList();
        var views = FindObjectsOfType<HexagonView>().ToList();

        //Link the model and view with each other
        for (int idx = 0; idx < _hexagonModels.Count; idx++)
        {
            HexagonModel model = _hexagonModels[idx];

            _hexagons.Add(model, views[idx]);

            //Link the model's coordinates to the model
            _doubleCoordinateToHexagon.Add(model.DoubleCoordinates, model);

            // Check if the hexagon model is part of the player's start row
            if (model.IsStartTile(true))
            {
                _playerStartTiles.Add(model);
            }
            // Check if the hexagon model is part of the AI's start row
            else if (model.IsStartTile(false))
            {
                _aiStartTiles.Add(model);
            }
        }

        //Grid created
        OnGridCreated?.Invoke(this, EventArgs.Empty);

        _campfireManager?.CreateCampfires();
    }

    public void HexagonsMovementHighlight(HexagonModel hexagon, TroopScriptableObject troopData)
    {
        //Highlight the movement tiles
        var hexagons = _gridController.GetMovementTiles(_hexagonModels, hexagon, troopData);

        //All the tiles that needs to be highlighted
        foreach (var tile in hexagons)
        {
            if (_hexagons.TryGetValue(tile, out var view))
                view.HighlightMovementTile();
        }

        //Store the highlighted tiles
        _highlightedTiles.AddRange(hexagons);
    }

    public void HexagonsAllAttackHighlight(HexagonModel hexagon, TroopScriptableObject troopData)
    {
        //Highlight all the attack tiles
        var hexagons = _gridController.GetAllAttackTiles(_hexagonModels, hexagon, troopData);

        //Highlight the attack tile invalidly
        foreach (var tile in hexagons)
        {
            if (_hexagons.TryGetValue(tile, out var view))
                view.HighlightInvalidAttackTile();
        }
        
        //Store the highlighted tiles
        _highlightedTiles.AddRange(hexagons);
    }

    public void HexagonsAttackHighlight(HexagonModel hexagon, TroopScriptableObject troopData)
    {
        //Highlight attack tiles
        var hexagons = _gridController.GetAttackTiles(_hexagonModels, hexagon, troopData);

        //All the tiles that needs to be highlighted
        foreach (var tile in hexagons)
        {
            if (_hexagons.TryGetValue(tile, out var view))
                view.HighlightAttackTile();


            //Check if there is a troop on that tile. If not, start the next iteration of the loop
            if (tile.Troop == null)
                continue;

            UITroopHPManager troopHPUI = tile.Troop.gameObject.GetComponentInChildren<UITroopHPManager>();
            if (troopHPUI != null && tile.Troop.TroopData.Side != troopData.Side)
                troopHPUI.LoopTroopHPOpacity(troopData.Damage);
        }

        //Store the highlighted tiles
        _highlightedTiles.AddRange(hexagons);
    }

    public void HexagonsUnHighlight()
    {
        //Unhighlight movement tiles
        foreach (var tile in _highlightedTiles)
        {
            if (_hexagons.TryGetValue(tile, out var view))
                view.UnHighlightTile();

            //Check if there is a troop on that tile. If not, start the next iteration of the loop
            if (tile.Troop == null)
                continue;

            UITroopHPManager troopHPUI = tile.Troop.gameObject.GetComponentInChildren<UITroopHPManager>();
            if (troopHPUI != null)
                troopHPUI.StopLoopTroopHP();

        }
        _highlightedTiles.Clear();
    }

    public List<HexagonModel> GetStartRow(bool isPlayerTurn)
    {
        return isPlayerTurn ? _playerStartTiles : _aiStartTiles;
    }

    public void HighlightStartRow(GameSides cardSide)
    {
        if (!_gameManager) return;

        // Get the start tiles
        bool isPlayerCard = (cardSide == _gameManager.PlayerSide);
        List<HexagonModel> startTiles = GetStartRow(isPlayerCard);

        // Highlight all valid tiles
        foreach (HexagonModel model in startTiles)
        {
            if (model.Troop != null) continue;

            if (_hexagons.TryGetValue(model, out HexagonView view))
            {
                view.HighlightMovementTile();
                _highlightedTiles.Add(model);
            }
        }
    }

    public void UnHighlightStartRow()
    {
        if (!_gameManager) return;

        // UnHighlight all highlighted tiles from the start row
        HexagonsUnHighlight();
    }

    public HexagonModel FindTileAtQrs(Vector3 qrs)
    {
        return _hexagonModels.FirstOrDefault(t => t.Qrs == qrs);
    }
}
