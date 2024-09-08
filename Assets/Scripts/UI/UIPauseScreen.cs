using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPauseScreen : MonoBehaviour
{
    [Header("Pause Screen")]
    [SerializeField]
    private GameObject _pauseScreen;
    [SerializeField]
    private CanvasGroup _pauseScreenBackground;
    [SerializeField]
    private GameObject _pauseScreenPanel;

    [SerializeField]
    private float _duration = 0.75f;
    [SerializeField]
    private LeanTweenType _easeType;

    [Header("Settings Screen")]
    [SerializeField]
    private GameObject _settingsMenu;
    [SerializeField]
    private GameObject _defaultPosition;
    [SerializeField]
    private GameObject _offscreenPosition;

    private float _currentTimeScale = 1f;
    public void PauseGame()
    {
        LeanTween.cancel(_pauseScreenBackground.gameObject);
        LeanTween.cancel(_pauseScreenPanel);

        _currentTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _pauseScreen.SetActive(true);

        LeanTween.alphaCanvas(_pauseScreenBackground, 1f, _duration).setEase(_easeType).setIgnoreTimeScale(true);
        LeanTween.scale(_pauseScreenPanel, Vector3.one, _duration).setIgnoreTimeScale(true);
    }
    
    public void UnpauseGame()
    {
        LeanTween.cancel(_pauseScreenBackground.gameObject);
        LeanTween.cancel(_pauseScreenPanel);

        Time.timeScale = _currentTimeScale;

        LeanTween.alphaCanvas(_pauseScreenBackground, 0f, _duration).setEase(_easeType).setIgnoreTimeScale(true);
        LeanTween.scale(_pauseScreenPanel, Vector3.zero, _duration).setIgnoreTimeScale(true).setOnComplete(HidePauseScreen);
    }

    private void HidePauseScreen()
    {
        _pauseScreen.SetActive(false);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        AudioManager.instance.PlayMenuMusic();
        SceneTransitionManager.instance.TransitionToMain();
    }

    public void ShowSettingsScreen()
    {
        LeanTween.cancel(_settingsMenu);
        LeanTween.moveLocalX(_settingsMenu, _defaultPosition.transform.localPosition.x, _duration).setEase(_easeType).setIgnoreTimeScale(true);
    }

    public void HideSettingsScreen()
    {
        LeanTween.cancel(_settingsMenu);
        LeanTween.moveLocalX(_settingsMenu, _offscreenPosition.transform.localPosition.x, _duration).setEase(_easeType).setIgnoreTimeScale(true);
    }
}
