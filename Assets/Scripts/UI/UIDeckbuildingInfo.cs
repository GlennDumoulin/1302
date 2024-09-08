using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDeckbuildingInfo : MonoBehaviour
{
    [SerializeField] private GameObject _infoScreenParent = null;
    [SerializeField] private GameObject _leftButton = null;
    [SerializeField] private GameObject _rightButton = null;
    [SerializeField] private GameObject _returnButton = null;
    [SerializeField] private TextMeshProUGUI _counter;

    [SerializeField] private List<GameObject> _infoScreens = new List<GameObject>();

    [SerializeField] private bool _isTutorialCampfireText = false;


    [Header("Tweening")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private LeanTweenType _easeType;

    private int _currentIndex = 0;

    public void ShowInfoScreen()
    {
        LeanTween.cancel(_infoScreenParent);
        _infoScreenParent.transform.localScale = Vector3.zero;
        _infoScreenParent.SetActive(true);
        SetUpCounter();
        UpdateButtons();
        LeanTween.scale(_infoScreenParent, Vector3.one, _duration).setEase(_easeType);
    }

    public void HideInfoScreen()
    {
        LeanTween.cancel(_infoScreenParent);
        LeanTween.scale(_infoScreenParent, Vector3.zero, _duration).setEase(_easeType).setOnComplete(DisableInfoScreen);
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.SetInt("HasPlayedTutorial", 1);

        AudioManager.instance.PlayMenuMusic();
        SceneTransitionManager.instance.TransitionToMain();
    }

    private void DisableInfoScreen()
    {
        _infoScreenParent.SetActive(false);
        _infoScreens[_currentIndex].SetActive(false);
        _currentIndex = 0;
        _infoScreens[_currentIndex].SetActive(true);
    }

    public void ShowPreviousImage()
    {
        if (_currentIndex < 0)
            return;

        _infoScreens[_currentIndex].SetActive(false);

        _currentIndex--;

        _infoScreens[_currentIndex].SetActive(true);
        SetUpCounter();
        UpdateButtons();
    }

    public void ShowNextImage()
    {
        if (_currentIndex > _infoScreens.Count)
            return;

        _infoScreens[_currentIndex].SetActive(false);

        _currentIndex++;

        _infoScreens[_currentIndex].SetActive(true);

        SetUpCounter();
        UpdateButtons();
    }

    private void SetUpCounter()
    {
        _counter.text = $"{_currentIndex + 1} / {_infoScreens.Count}";
    }

    private void UpdateButtons()
    {
        _leftButton.SetActive(true);
        _rightButton.SetActive(true);
        if (_isTutorialCampfireText)
            _returnButton.SetActive(false);

        if (_currentIndex == 0)
        {
            _leftButton.SetActive(false);
        }

        if (_currentIndex == _infoScreens.Count - 1)
        {
            _rightButton.SetActive(false);
            if (_isTutorialCampfireText)
                _returnButton.SetActive(true);
        }
    }


}
