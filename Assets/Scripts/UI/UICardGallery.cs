using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardGallery : MonoBehaviour
{
    [SerializeField]
    private Image _frontImage = null;
    [SerializeField]
    private Image _backImage = null;
    [SerializeField]
    private Sprite _frenchTroopSprite = null;

    private Sprite _flemishTroopSprite = null;

    private CardGalleryManager _cardGalleryManager = null;

    private void Start()
    {
        _flemishTroopSprite = _frontImage.sprite;
        _cardGalleryManager = FindObjectOfType<CardGalleryManager>();
    }

    internal void SetToFlemishImage()
    {
        _frontImage.sprite = _flemishTroopSprite;
        _backImage.sprite = _flemishTroopSprite;
    }

    internal void SetToFrenchImage()
    {
        _frontImage.sprite = _frenchTroopSprite;
        _backImage.sprite = _frenchTroopSprite;
    }
}
