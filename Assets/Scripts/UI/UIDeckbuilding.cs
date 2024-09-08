using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckbuilding : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] private GameObject _chooseSideParent = null;

    [Header("CHOOSE SIDE")]
    [SerializeField] private Image _flemishImage = null;
    [SerializeField] private Image _frenchImage = null;
    [SerializeField] private Sprite _selectedFlemishSprite = null;
    [SerializeField] private Sprite _selectedFrenchSprite = null;
    [SerializeField] private GameObject _flemishText = null;
    [SerializeField] private GameObject _frenchText = null;
    [SerializeField] private GameObject _moveToDeckbuildingButton = null;

    [Header("Card Parents Objects")]
    [SerializeField] private GameObject _flemishCards = null;
    [SerializeField] private GameObject _frenchCards = null;

    [Header("Card Tier Objects")]
    [SerializeField] private GameObject _flemishTierOneCards = null;
    [SerializeField] private GameObject _flemishTierTwoCards = null;
    [SerializeField] private GameObject _flemishTierThreeCards = null;
    [SerializeField] private GameObject _flemishTierFourCards = null;
    [SerializeField] private GameObject _frenchTierOneCards = null;
    [SerializeField] private GameObject _frenchTierTwoCards = null;
    [SerializeField] private GameObject _frenchTierThreeCards = null;
    [SerializeField] private GameObject _frenchTierFourCards = null;

    [Header("Text Fields")]
    [SerializeField] private List<CounterHelper> _counters = new List<CounterHelper>();
    [SerializeField] private UIDeckTiers _deckTiers = null;

    [Header("Tweening")]
    [SerializeField] private Transform _offscreenPos;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _buttonDuration = 0.25f;
    [SerializeField] private LeanTweenType _easeTypeOffscreen;
    [SerializeField] private LeanTweenType _easeTypeOnscreen;
    [SerializeField] private LeanTweenType _easeTypeButton;

    private Sprite _unselectedFlemishSprite = null;
    private Sprite _unselectedFrenchSprite = null;

    private DeckBuildingManager _deckbuildingManager;
    private DeckBannerHelper _deckBannerHelper;

    //Int = 0, Flemish
    //Int = 1, French
    private int _sideChosen = -1;

    private void Awake()
    {
        _deckbuildingManager = FindObjectOfType<DeckBuildingManager>();
        _deckBannerHelper = FindObjectOfType<DeckBannerHelper>();
        _unselectedFlemishSprite = _flemishImage.sprite;
        _unselectedFrenchSprite = _frenchImage.sprite;
    }

    public void GoToMainMenu()
    {
        SceneTransitionManager.instance.TransitionToMain();
    }

    public void MoveToDeckbuilding()
    {
        if (_sideChosen < 0)
            return;

        if (_sideChosen == 0)
        {
            _frenchCards.SetActive(false);
            _flemishCards.SetActive(true);
        }
        else
        {
            _flemishCards.SetActive(false);
            _frenchCards.SetActive(true);
        }

        _deckbuildingManager.SetUpSides(_sideChosen);

        MoveChooseSideOffScreen();

        // Automatically open the deckbuilding info if the player hasn't played a game yet
        bool hasPlayedGame = Convert.ToBoolean(PlayerPrefs.GetInt("HasPlayedGame", 0));
        if (!hasPlayedGame)
        {
            Invoke("ShowInfoScreen", 0.75f);
        }
    }

    private void ShowInfoScreen()
    {
        UIDeckbuildingInfo info = FindObjectOfType<UIDeckbuildingInfo>();
        if (info) info.ShowInfoScreen();
    }

    public void MoveBackToChooseSides()
    {
        MoveChooseSideOnScreen();
    }

    public void SelectedFlemish()
    {
        _flemishImage.sprite = _selectedFlemishSprite;
        _flemishText.SetActive(true);
        _frenchImage.sprite = _unselectedFrenchSprite;
        _frenchText.SetActive(false);
        _sideChosen = 0;
        ShowMoveToDeckbuildingButton();
    }

    public void SelectedFrench()
    {
        _flemishImage.sprite = _unselectedFlemishSprite;
        _flemishText.SetActive(false);
        _frenchImage.sprite = _selectedFrenchSprite;
        _frenchText.SetActive(true);
        _sideChosen = 1;
        ShowMoveToDeckbuildingButton();
    }

    public void ShowTierOneCards()
    {
        DisableAllCards();
        _flemishTierOneCards.SetActive(true);
        _frenchTierOneCards.SetActive(true);
    }
    public void ShowTierTwoCards()
    {
        DisableAllCards();
        _flemishTierTwoCards.SetActive(true);
        _frenchTierTwoCards.SetActive(true);
    }
    public void ShowTierThreeCards()
    {
        DisableAllCards();
        _flemishTierThreeCards.SetActive(true);
        _frenchTierThreeCards.SetActive(true);
    }
    public void ShowTierFourCards()
    {
        DisableAllCards();
        _flemishTierFourCards.SetActive(true);
        _frenchTierFourCards.SetActive(true);
    }

    private void DisableAllCards()
    {
        _flemishTierOneCards.SetActive(false);
        _flemishTierTwoCards.SetActive(false);
        _flemishTierThreeCards.SetActive(false);
        _flemishTierFourCards.SetActive(false);

        _frenchTierOneCards.SetActive(false);
        _frenchTierTwoCards.SetActive(false);
        _frenchTierThreeCards.SetActive(false);
        _frenchTierFourCards.SetActive(false);
    }

    internal void ResetDeckUI()
    {
        foreach (CounterHelper counter in _counters)
            counter.ResetCounter();

        ShowTierOneCards();
        _deckTiers.HighlightTier(1);
    }

    private void ShowMoveToDeckbuildingButton()
    {
        _moveToDeckbuildingButton.SetActive(true);
        LeanTween.scale(_moveToDeckbuildingButton, Vector3.one, _buttonDuration).setEase(_easeTypeButton);
    }

    private void MoveChooseSideOffScreen()
    {
        LeanTween.moveLocalY(_chooseSideParent.gameObject, _offscreenPos.localPosition.y, _duration).setEase(_easeTypeOffscreen).setOnComplete(_deckBannerHelper.MoveBannerOffscreen);
    }

    private void MoveChooseSideOnScreen()
    {
        LeanTween.moveLocalY(_chooseSideParent.gameObject, 0f, _duration).setEase(_easeTypeOnscreen).setOnComplete(_deckBannerHelper.ResetBanner);
    }
}
