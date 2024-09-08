using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UICampfireCounter : MonoBehaviour
{
    [Header("Big Flame Sprites")]
    [SerializeField]
    private Image _currentImage = null;
    [SerializeField]
    private Sprite _emptySprite = null;
    [SerializeField]
    private Sprite _halfSprite = null;
    [SerializeField]
    private Sprite _fullSprite = null;

    [Header("Small Flame Sprites")]
    [SerializeField]
    private List<Image> _campfireIcons = new List<Image>();
    public List<Image> CampfireIcons { get { return _campfireIcons; } }
    [SerializeField]
    private Sprite _activeSprite = null;
    [SerializeField]
    private Sprite _flemishSprite = null;
    [SerializeField]
    private Sprite _frenchSprite = null;

    [SerializeField]
    private RectTransform _position;
    public RectTransform Position { get { return _position; } }

    [Header("Tweening")]
    [SerializeField]
    private GameObject _campfireGlow = null;
    [SerializeField]
    private Vector3 _baseScale = Vector3.one;

    [Header("Campsite Warning")]
    [SerializeField] private List<Slider> _turnIndicators = new List<Slider>();
    [SerializeField] private float _slideDuration = 0.5f;
    [SerializeField] private float _refillDuration = 0.5f;

    private CampfireManager _campfireManager = null;
    private GameLoopManager _gameLoopManager = null;
    private int _turnOrder = -1;
    private int _numOfCampsDestroyed;

    public event EventHandler OnRadialChanged;

    private void Awake()
    {
        _campfireManager = FindObjectOfType<CampfireManager>();
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
    }

    private void Start()
    {
        _campfireGlow.SetActive(false);
        _currentImage.sprite = _emptySprite;
    }

    private void OnEnable()
    {
        _campfireManager.OnCampfireCountUpdated += UpdateCampfireCounter;
        _campfireManager.OnCampfireDefendCounterUpdated += UpdateCampfireSprite;
        _campfireManager.OnCampfireWarningUpdate += UpdateWarnings;
    }

    private void OnDisable()
    {
        _campfireManager.OnCampfireCountUpdated -= UpdateCampfireCounter;
        _campfireManager.OnCampfireDefendCounterUpdated -= UpdateCampfireSprite;
        _campfireManager.OnCampfireWarningUpdate -= UpdateWarnings;
    }

    private void UpdateCampfireCounter(object sender, ListImageEventArgs e)
    {
        for (int i = 0; i < _campfireManager.CampfireModel.Count; i++)
        {
            Image campfireIcon = e.Images.ElementAt(i);
            if (campfireIcon)
                campfireIcon.sprite = GetCorrectCampfireSprite(_campfireManager.CampfireModel.ElementAt(i));
        }
    }

    private Sprite GetCorrectCampfireSprite(CampfireModel campfire)
    {
        if (campfire.CurrentDefender == null)
            return _activeSprite;

        if (campfire.CurrentDefender.TroopData.Side == GameSides.French)
            return _frenchSprite;

        return _flemishSprite;
    }

    private void UpdateCampfireSprite(object sender, EventArgs e)
    {
        switch (_campfireManager.CountdownCounter)
        {
            case 0:
                _currentImage.sprite = _emptySprite;
                ShrinkenCampfire();
                break;
            case 1:
                _currentImage.sprite = _halfSprite;
                EnlargeCampfire();
                break;
            case 2:
                _currentImage.sprite = _fullSprite;
                EnlargeCampfire();
                break;
            default:
                _currentImage.sprite = _fullSprite;
                break;
        }
    }

    internal void RemoveCampfireFromList(int index)
    {
        Image campfireIconToBeRemoved = _campfireIcons[index];
        _campfireIcons.RemoveAt(index);

        Destroy(campfireIconToBeRemoved.gameObject);
    }

    private void ShrinkenCampfire()
    {
        LeanTween.cancel(this.gameObject);
        LeanTween.scale(this.gameObject, _baseScale * 0.75f, 0.5f);
        LeanTween.scale(this.gameObject, _baseScale, 0.5f).setDelay(0.5f);
    }

    internal void EnlargeCampfire()
    {
        LeanTween.cancel(this.gameObject);
        _campfireGlow.SetActive(true);
        LeanTween.scale(this.gameObject, _baseScale * 1.25f, 0.5f);
        LeanTween.scale(this.gameObject, _baseScale, 0.5f).setDelay(0.55f).setOnComplete(DisableCampfireGlow);
    }

    private void DisableCampfireGlow()
    {
        _campfireGlow.SetActive(false);
    }

    private void UpdateWarnings(object sender, EventArgs e)
    {
        if (_numOfCampsDestroyed > 1)
            return;

        _turnOrder++;
        EnlargenCampfireSmall();
        OnRadialChanged?.Invoke(this, EventArgs.Empty);

        if (_turnOrder != 3)
            LeanTween.value(_turnIndicators[_turnOrder].gameObject, UpdateSlider, 1f, 0f, _slideDuration);
        else
            LeanTween.value(_turnIndicators[_turnOrder].gameObject, UpdateSlider, 1f, 0f, _slideDuration).setOnComplete(RefillIndicators);
    }

    private void RefillIndicators()
    {
        LeanTween.value(_turnIndicators[3].gameObject, UpdateSliderOne, 0f, 1f, _refillDuration / 4f);
        LeanTween.value(_turnIndicators[2].gameObject, UpdateSliderTwo, 0f, 1f, _refillDuration / 4f).setDelay(_refillDuration / 4f);
        LeanTween.value(_turnIndicators[1].gameObject, UpdateSliderThree, 0f, 1f, _refillDuration / 4f).setDelay(_refillDuration / 2f);
        LeanTween.value(_turnIndicators[0].gameObject, UpdateSliderFour, 0f, 1f, _refillDuration / 4f).setDelay((3f * _refillDuration / 4f));
        _turnOrder = -1;
        _numOfCampsDestroyed++;
    }

    private void UpdateSlider(float value)
    {
        _turnIndicators[_turnOrder].value = value;
    }

    private void UpdateSliderOne(float value)
    {
        _turnIndicators[3].value = value;
    }
    private void UpdateSliderTwo(float value)
    {
        _turnIndicators[2].value = value;
    }
    private void UpdateSliderThree(float value)
    {
        _turnIndicators[1].value = value;
    }
    private void UpdateSliderFour(float value)
    {
        _turnIndicators[0].value = value;
    }

    private void EnlargenCampfireSmall()
    {
        LeanTween.scale(this.gameObject, _baseScale * 1.15f, 0.35f);
        LeanTween.scale(this.gameObject, _baseScale, 0.35f).setDelay(0.35f);
    }
}
