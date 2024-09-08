using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEndScreen : MonoBehaviour
{
    //GameObjects
    [Header("Parent Objects")]
    [SerializeField]
    private GameObject _imageParent;
    [SerializeField]
    private GameObject _textParent;
    [SerializeField]
    private GameObject _backgroundImage;
    [SerializeField]
    private GameObject _button;

    //Images
    [Header("Images")]
    [SerializeField]
    private Image _frame;
    [SerializeField]
    private Image _banner;

    //French UI Elements
    [Header("French Elements")]
    [SerializeField]
    private Sprite _frenchFrame;
    [SerializeField]
    private Sprite _frenchWinBanner;
    [SerializeField]
    private Sprite _frenchLoseBanner;

    //Flemish UI Elements
    [Header("Flemish Elements")]
    [SerializeField]
    private Sprite _flemishFrame;
    [SerializeField]
    private Sprite _flemishWinBanner;
    [SerializeField]
    private Sprite _flemishLoseBanner;

    //Text UI Elements
    [Header("Textboxes")]
    [SerializeField]
    private GameObject _youWinText;
    [SerializeField]
    private GameObject _youLoseText;

    //UI Tweening
    [Header("Tweening Parameters")]
    [SerializeField]
    private float _scaleUpTime = 1f;
    [SerializeField]
    private LeanTweenType _easeType;

    private GameLoopManager _gameLoopManager = null;

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();

        _backgroundImage.transform.localScale = Vector3.zero;
        _imageParent.transform.localScale = Vector3.zero;
        _textParent.transform.localScale = Vector3.zero;
        _button.transform.localScale = Vector3.zero;
    }

    private void Start()
    {

        _backgroundImage.SetActive(false);
        _imageParent.SetActive(false);
        _textParent.SetActive(false);
        _button.SetActive(false);
    }

    private void OnEnable()
    {
        _gameLoopManager.OnGameEnd += SetBanners;
    }

    private void OnDisable()
    {
        _gameLoopManager.OnGameEnd -= SetBanners;
    }

    private void SetBanners(object sender, GameSideEventArgs e)
    {
        _backgroundImage.SetActive(true);
        _imageParent.SetActive(true);
        _textParent.SetActive(true);
        _button.SetActive(true);

        switch (_gameLoopManager.PlayerSide)
        {
            case (GameSides.Flemish):
                _frame.sprite = _flemishFrame;
                if (e.Side == GameSides.Flemish)
                {
                    _banner.sprite = _flemishWinBanner;
                    _youWinText.SetActive(true);
                } else
                {
                    _banner.sprite = _flemishLoseBanner;
                    _youLoseText.SetActive(true);
                }
                break;
            case (GameSides.French):
                _frame.sprite = _frenchFrame;

                if (e.Side == GameSides.French)
                {
                    _banner.sprite = _frenchWinBanner;
                    _youWinText.SetActive(true);
                }
                else
                {
                    _banner.sprite = _frenchLoseBanner;
                    _youLoseText.SetActive(true);
                }

                break;
        }

        LeanTween.scale(_backgroundImage, Vector3.one, _scaleUpTime).setEase(_easeType);
        LeanTween.scale(_imageParent, Vector3.one, _scaleUpTime).setEase(_easeType);
        LeanTween.scale(_textParent, Vector3.one, _scaleUpTime).setEase(_easeType).setOnComplete(ScaleButton);
    }

    private void ScaleButton()
    {
        LeanTween.scale(_button, Vector3.one, _scaleUpTime).setDelay(_scaleUpTime).setEase(_easeType);
    }

    public void Restart()
    {
        SceneTransitionManager.instance.TransitionToGame();
    }
}

