using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField]
    private GameObject _mainMenu;
    [SerializeField]
    private GameObject _galleryMenu;
    [SerializeField]
    private GameObject _settingsMenu;
    [SerializeField]
    private GameObject _creditsMenu;

    [Header("Tweening")]
    [SerializeField]
    private GameObject _positionHelper;
    [SerializeField]
    private float _duration = 1f;
    [SerializeField]
    private LeanTweenType _easeType;

    public void MoveToMainMenu()
    {
        LeanTween.cancelAll();
        _mainMenu.SetActive(true);
        LeanTween.moveLocalX(_galleryMenu, _positionHelper.transform.localPosition.x, _duration).setEase(_easeType);
        LeanTween.moveLocalX(_settingsMenu, _positionHelper.transform.localPosition.x, _duration).setEase(_easeType);
        LeanTween.moveLocalX(_creditsMenu, _positionHelper.transform.localPosition.x, _duration).setEase(_easeType);
    }

    public void MoveToGalleryMenu()
    {
        LeanTween.cancel(_galleryMenu);
        _galleryMenu.SetActive(true);
        LeanTween.moveLocalX(_galleryMenu, -2f, _duration).setEase(_easeType);
    }

    public void MoveToSettingsMenu()
    {
        LeanTween.cancel(_settingsMenu);
        _settingsMenu.SetActive(true);
        LeanTween.moveLocalX(_settingsMenu, -2f, _duration).setEase(_easeType);
    }

    public void MoveToCreditsMenu()
    {
        LeanTween.cancel(_creditsMenu);
        _creditsMenu.SetActive(true);
        LeanTween.moveLocalX(_creditsMenu, -2f, _duration).setEase(_easeType);
    }


    private void SetAllMenusToInactive()
    {
        _galleryMenu.SetActive(false);
        _settingsMenu.SetActive(false);
        _creditsMenu.SetActive(false);

        SetAllMenuPositionsOffscreen();
    }

    private void SetAllMenuPositionsOffscreen()
    {
        _galleryMenu.transform.localPosition = _positionHelper.transform.localPosition;
        _settingsMenu.transform.localPosition = _positionHelper.transform.localPosition;
        _creditsMenu.transform.localPosition = _positionHelper.transform.localPosition;
    }
}
