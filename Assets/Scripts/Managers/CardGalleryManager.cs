using DanielLochner.Assets.SimpleScrollSnap;
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGalleryManager : MonoBehaviour
{
    private UICardGallery _currentlySelectedCard;

    [SerializeField]
    private SimpleScrollSnap _scrollArea;

    [Header("Card Info")]
    [SerializeField]
    private List<UICardGallery> _troopCards;

    [Header("Flemish Info")]
    [SerializeField]
    private List<GameObject> _flemishTroopNames;
    [SerializeField]
    private List<GameObject> _flemishTroopInfo;

    [Header("French Info")]
    [SerializeField]
    private List<GameObject> _frenchTroopNames;
    [SerializeField]
    private List<GameObject> _frenchTroopInfo;


    [Header("Side Buttons")]
    [SerializeField]
    private GameObject _flemishButton = null;
    [SerializeField]
    private GameObject _frenchButton = null;
    [SerializeField]
    private GameObject _flemishHiddenPosition = null;
    [SerializeField]
    private GameObject _frenchHiddenPosition = null;
    [SerializeField]
    private GameObject _flemishVisiblePosition = null;
    [SerializeField]
    private GameObject _frenchVisiblePosition = null;
    [SerializeField]
    private float _tweenDuration = 0.15f;
    [SerializeField]
    private LeanTweenType _easeType;

    private Vector3 _flemishDefaultPosition = Vector3.zero;
    private Vector3 _frenchDefaultPosition = Vector3.zero;

    private GameSides _activeSide = GameSides.Flemish;

    private int _currentIndex;

    private List<GameObject> _currentNameList = new List<GameObject>();
    private List<GameObject> _currentDescriptionList = new List<GameObject>();

    private void Awake()
    {
        _flemishDefaultPosition = _flemishVisiblePosition.transform.localPosition;
        _frenchDefaultPosition = _frenchVisiblePosition.transform.localPosition;

        _frenchButton.transform.localPosition = _frenchHiddenPosition.transform.localPosition;
    }

    private void Start()
    {
        _currentNameList = _flemishTroopNames;
        _currentDescriptionList = _flemishTroopInfo;

        _currentlySelectedCard = _troopCards[_scrollArea.StartingPanel];
        _currentIndex = _scrollArea.StartingPanel;
    }
    public void GetCurrentObject()
    {
        if (_currentlySelectedCard != null)
            DisableCardInfo(_currentIndex);

        _currentlySelectedCard = _troopCards[_scrollArea.CenteredPanel];
        _currentIndex = _scrollArea.CenteredPanel;
        ShowCardInfo(_currentIndex);
    }

    public void ChangeCurrentToFrench()
    {
        if (_currentlySelectedCard == null)
            return;

        _activeSide = GameSides.French;

        DisableCardInfo(_currentIndex);

        _currentNameList = _frenchTroopNames;
        _currentDescriptionList = _frenchTroopInfo;

        ShowCardInfo(_currentIndex);

        foreach (UICardGallery card in _troopCards)
            card.SetToFrenchImage();

        SetButtonPositions();
    }

    public void ChangeCurrentToFlemish()
    {
        if (_currentlySelectedCard == null)
            return;

        _activeSide = GameSides.Flemish;

        DisableCardInfo(_currentIndex);

        _currentNameList = _flemishTroopNames;
        _currentDescriptionList = _flemishTroopInfo;

        ShowCardInfo(_currentIndex);

        foreach (UICardGallery card in _troopCards)
            card.SetToFlemishImage();

        SetButtonPositions();
    }

    private void ShowCardInfo(int index)
    {
        _currentNameList[index].SetActive(true);
        _currentDescriptionList[index].SetActive(true);
    }

    private void DisableCardInfo(int index)
    {
        _currentNameList[index].SetActive(false);
        _currentDescriptionList[index].SetActive(false);
    }

    private void SetButtonPositions()
    {
        if (_activeSide == GameSides.Flemish)
        {
            LeanTween.moveLocalX(_flemishButton, _flemishVisiblePosition.transform.localPosition.x, _tweenDuration).setEase(_easeType);
            LeanTween.moveLocalX(_frenchButton, _frenchHiddenPosition.transform.localPosition.x, _tweenDuration).setEase(_easeType);
        } else
        {
            LeanTween.moveLocalX(_flemishButton, _flemishHiddenPosition.transform.localPosition.x, _tweenDuration).setEase(_easeType);
            LeanTween.moveLocalX(_frenchButton, _flemishVisiblePosition.transform.localPosition.x, _tweenDuration).setEase(_easeType);
        }
    }

}
