using GameEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UICampfireIcons : MonoBehaviour
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

    private CampfireManager _campfireManager = null;

    private void Awake()
    {
        _campfireManager = FindObjectOfType<CampfireManager>();
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

    }

    private void OnDisable()
    {
        _campfireManager.OnCampfireCountUpdated -= UpdateCampfireCounter;
        _campfireManager.OnCampfireDefendCounterUpdated -= UpdateCampfireSprite;
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
        LeanTween.scale(this.gameObject, _baseScale * 0.75f, 0.5f);
        LeanTween.scale(this.gameObject, _baseScale, 0.5f).setDelay(0.5f);
    }

    internal void EnlargeCampfire()
    {
        _campfireGlow.SetActive(true);
        LeanTween.scale(this.gameObject, _baseScale * 1.25f, 0.5f);
        LeanTween.scale(this.gameObject, _baseScale, 0.5f).setDelay(0.55f).setOnComplete(DisableCampfireGlow);
    }

    private void DisableCampfireGlow()
    {
        _campfireGlow.SetActive(false);
    }
}
