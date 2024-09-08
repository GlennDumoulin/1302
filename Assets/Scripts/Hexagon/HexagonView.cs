using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonView : MonoBehaviour
{
    private HexagonModel _model = null;

    //The mesh that needs to be highlighted
    private MeshRenderer _meshToHighlight = null;

    //All the materials a tile can have(move, attack, default)
    [Header("Game Materials")]
    [SerializeField] private Material _highlightMovementMaterial = null;
    [SerializeField] private Material _highlightInvalidAttackMaterial = null;
    [SerializeField] private Material _highlightAttackMaterial = null;
    [SerializeField] private Material _highlightCampfireMaterial = null;
    [SerializeField] private Material _highlightMudMaterial = null;

    [Header("Tutorial Materials")]
    [SerializeField] private Material _highlightTutorialTile = null;
    [SerializeField] private Material _highlightTutorialAttack = null;
    [SerializeField] private Material _highlightTutorialCampfire = null;

    private Material _defaultMaterial = null;

    // Start is called before the first frame update
    private void Awake()
    {
        _meshToHighlight = GetComponentInChildren<MeshRenderer>();
        _model = GetComponent<HexagonModel>();
        _defaultMaterial = _meshToHighlight.material;
    }

    private void OnEnable()
    {
        _model.OnCampfireDefenderEnter += ApplyCampfireHighlight;
        _model.OnCampfireDefenderLeave += RemoveCampfireHighlight;
    }

    private void OnDisable()
    {
        _model.OnCampfireDefenderEnter -= ApplyCampfireHighlight;
        _model.OnCampfireDefenderLeave -= RemoveCampfireHighlight;
    }

    public void HighlightMovementTile()
    {
        if (_model.Campfire != null)
            _meshToHighlight.material = _highlightCampfireMaterial;
        else if (_model.IsMud)
            _meshToHighlight.material = _highlightMudMaterial;
        else
            _meshToHighlight.material = _highlightMovementMaterial;
    }

    public void HighlightInvalidAttackTile()
    {
        _meshToHighlight.material = _highlightInvalidAttackMaterial;
    }

    public void HighlightAttackTile() 
    {
        _meshToHighlight.material = _highlightAttackMaterial;
    }

    public void HighlightTutorialTile()
    {
        _meshToHighlight.material = _highlightTutorialTile;
    }

    public void HighlightTutorialAttackTile()
    {
        _meshToHighlight.material = _highlightTutorialAttack;
    }

    public void HighlightTutorialcampfireTile()
    {
        _meshToHighlight.material = _highlightTutorialCampfire;
    }

    public void UnHighlightTile()
    {
        if (_model.Campfire != null && _model.Troop != null)
            _meshToHighlight.material = _highlightCampfireMaterial;
        else
            _meshToHighlight.material = _defaultMaterial;
    }

    private void ApplyCampfireHighlight(object sender, EventArgs e)
    {
        _meshToHighlight.material = _highlightCampfireMaterial;
    }

    private void RemoveCampfireHighlight(object sender, EventArgs e)
    {
        _meshToHighlight.material = _defaultMaterial;
    }
}
