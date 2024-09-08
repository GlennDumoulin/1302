using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CounterHelper : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _counterText;
    [SerializeField]
    private int _manpower = 1;
    [SerializeField]
    private GameObject _troopCardPrefab;

    [Header("Tweening")]
    [SerializeField]
    private GameObject _uiCardSpawn;
    [SerializeField]
    private Transform _parentTransform;
    [SerializeField]
    private Transform _centerPos;
    [SerializeField]
    private Transform _offscreenPos;
    [SerializeField]
    private float _duration = 0.5f;
    [SerializeField]
    private LeanTweenType _easeType;

    private DeckBuildingManager _deckbuildingManager;
    private int _counter = 0;

    private ManpowerLimit _manpowerLimit;

    private void Awake()
    {
        _counter = 0;
        _counterText.text = _counter.ToString();

        _manpowerLimit = FindObjectOfType<ManpowerLimit>();
        _deckbuildingManager = FindObjectOfType<DeckBuildingManager>();
    }

    public void IncrementCounter()
    {
        if (!_manpowerLimit.UpdateManpower(_manpower))
            return;

        _deckbuildingManager.AddCard(_troopCardPrefab);
        _counter++;
        _counterText.text = _counter.ToString();
        MoveCardOffScreen();
    }

    public void DecrementCounter() 
    {
        if (!_deckbuildingManager.RemoveCard(_troopCardPrefab))
            return;

        if (!_manpowerLimit.UpdateManpower(-_manpower))
            return;

        _counter--;
        if (_counter < 0)
            _counter = 0;

        _counterText.text = _counter.ToString();

        MoveCardOnScreen();
    }

    private void MoveCardOffScreen()
    {
        var card = Instantiate(_uiCardSpawn, _parentTransform);
        card.transform.localPosition = _centerPos.localPosition;

        LeanTween.moveLocal(card, _offscreenPos.localPosition, _duration).setEase(_easeType).setOnComplete(() => Destroy(card));
    }

    private void MoveCardOnScreen()
    {
        var card = Instantiate(_uiCardSpawn, _parentTransform);
        card.transform.localPosition = _offscreenPos.localPosition;

        LeanTween.moveLocal(card, _centerPos.localPosition, _duration).setEase(_easeType).setOnComplete(() => Destroy(card));
    }

    internal void ResetCounter()
    {
        _counter = 0;
        _counterText.text = _counter.ToString();
    }
}
