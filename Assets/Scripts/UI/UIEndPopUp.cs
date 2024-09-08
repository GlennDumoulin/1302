using GameEnum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEndPopUp : MonoBehaviour
{
    //GameObjects
    [Header("Parent Objects")]
    [SerializeField]
    private GameObject _imageParent;
    [SerializeField]
    private GameObject _textParent;
    [SerializeField]
    private GameObject _backgroundImageParent;
    [SerializeField]
    private GameObject _button;
    [SerializeField]
    private GameObject _deckbuildingButton;
    [SerializeField]
    private GameObject _tutorialButton;

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
    [SerializeField]
    private GameObject _wellDoneText;

    //UI Tweening
    [Header("Tweening Parameters")]
    [SerializeField]
    private float _scaleUpTime = 1f;
    [SerializeField]
    private LeanTweenType _backgroudEase;
    [SerializeField]
    private LeanTweenType _youLoseEase;
    [SerializeField]
    private LeanTweenType _youWinEase;
    [SerializeField]
    private LeanTweenType _buttonEase;
    [SerializeField]
    private CanvasGroup _bannerCanvas = null;
    [SerializeField]
    private CanvasGroup _backgroundCanvas = null;
    [SerializeField]
    private GameObject _defaultPos = null;
    [SerializeField]
    private GameObject _youLosePos = null;
    [SerializeField]
    private GameObject _youWinPos = null;
    [SerializeField]
    private Image _bgImage;
    [SerializeField]
    private Color _youLoseBGColor = Color.red;
    [SerializeField]
    private Color _youWinBGColor = Color.green;

    //UnityEvents
    [Header("Events")]
    public UnityEvent OnPlayerWin;
    public UnityEvent OnPlayerLose;

    private GameLoopManager _gameLoopManager = null;
    private TutorialScriptHandler _tutorial = null;

    private void Awake()
    {
        _gameLoopManager = FindObjectOfType<GameLoopManager>();
        _tutorial = FindObjectOfType<TutorialScriptHandler>();

        /*
        _backgroundImage.transform.localScale = Vector3.zero;
        _imageParent.transform.localScale = Vector3.zero;
        _textParent.transform.localScale = Vector3.zero;
        */

        _button.transform.localScale = Vector3.zero;
        _deckbuildingButton.transform.localScale = Vector3.zero;
        _tutorialButton.transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        _backgroundImageParent.SetActive(false);
        _imageParent.SetActive(false);
        _textParent.SetActive(false);
        _button.SetActive(false);
        _deckbuildingButton.SetActive(false);
        _tutorialButton.SetActive(false);
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
        _backgroundImageParent.SetActive(true);
        _imageParent.SetActive(true);
        _textParent.SetActive(true);

        if (_tutorial)
        {
            _tutorialButton.SetActive(true);

            _frame.sprite = _flemishFrame;
            _banner.sprite = _flemishWinBanner;
            _wellDoneText.SetActive(true);
            OnPlayerWin?.Invoke();
        }
        else
        {
            _button.SetActive(true);
            _deckbuildingButton.SetActive(true);

            switch (_gameLoopManager.PlayerSide)
            {
                case (GameSides.Flemish):
                    _frame.sprite = _flemishFrame;
                    if (e.Side == GameSides.Flemish)
                    {
                        _banner.sprite = _flemishWinBanner;
                        _youWinText.SetActive(true);
                        OnPlayerWin?.Invoke();
                    }
                    else
                    {
                        _banner.sprite = _flemishLoseBanner;
                        _youLoseText.SetActive(true);
                        OnPlayerLose?.Invoke();
                    }
                    break;
                case (GameSides.French):
                    _frame.sprite = _frenchFrame;

                    if (e.Side == GameSides.French)
                    {
                        _banner.sprite = _frenchWinBanner;
                        _youWinText.SetActive(true);
                        OnPlayerWin?.Invoke();
                    }
                    else
                    {
                        _banner.sprite = _frenchLoseBanner;
                        _youLoseText.SetActive(true);
                        OnPlayerLose?.Invoke();
                    }

                    break;
            }
        }

        /*
        LeanTween.scale(_backgroundImage, Vector3.one, _scaleUpTime).setEase(_easeType).setIgnoreTimeScale(true);
        LeanTween.scale(_imageParent, Vector3.one, _scaleUpTime).setEase(_easeType).setIgnoreTimeScale(true);
        LeanTween.scale(_textParent, Vector3.one, _scaleUpTime).setEase(_easeType).setOnComplete(ScaleButton).setIgnoreTimeScale(true);
        */
    }

    private void ScaleButton()
    {
        if (_tutorial)
        {
            LeanTween.scale(_tutorialButton, Vector3.one, 0.5f).setEase(_buttonEase).setIgnoreTimeScale(true);
        }
        else
        {
            LeanTween.scale(_button, Vector3.one, 0.5f).setEase(_buttonEase).setIgnoreTimeScale(true);
            LeanTween.scale(_deckbuildingButton, Vector3.one, 0.5f).setEase(_buttonEase).setIgnoreTimeScale(true);
        }
    }

    public void PlayYouWinMusic() 
    {
        AudioManager.instance.PlayYouWinMusic();

        _backgroundCanvas.alpha = 0f;
        _imageParent.transform.localPosition = _youWinPos.transform.localPosition;
        _bgImage.color = _youWinBGColor;

        LeanTween.alphaCanvas(_backgroundCanvas, 1f, _scaleUpTime / 2f).setEase(_backgroudEase).setIgnoreTimeScale(true);
        LeanTween.moveLocal(_imageParent, _defaultPos.transform.localPosition, _scaleUpTime).setEase(_youWinEase).setIgnoreTimeScale(true).setOnComplete(ScaleButton);
    }

    public void PlayYouLoseMusic()
    {
        AudioManager.instance.PlayYouLoseMusic();

        _backgroundCanvas.alpha = 0f;
        _bannerCanvas.alpha = 0f;
        _imageParent.transform.localPosition = _youLosePos.transform.localPosition;
        _bgImage.color = _youLoseBGColor;

        LeanTween.alphaCanvas(_backgroundCanvas, 1f, _scaleUpTime / 2f).setEase(_backgroudEase).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(_bannerCanvas, 1f, _scaleUpTime / 1.25f).setEase(_youLoseEase).setIgnoreTimeScale(true);
        LeanTween.moveLocal(_imageParent, _defaultPos.transform.localPosition, _scaleUpTime).setEase(_youLoseEase).setIgnoreTimeScale(true).setOnComplete(ScaleButton);
    }

    public void Restart()
    {
        AudioManager.instance.PlayGameMusic();
        SceneTransitionManager.instance.TransitionToGame();
    }

    public void ContinueTutorial()
    {
        _tutorial.LoadNextLevel();
    }

    public void GoToDeckbuilding()
    {
        AudioManager.instance.PlayMenuMusic();
        SceneTransitionManager.instance.TransitionToMain();
    }
}
