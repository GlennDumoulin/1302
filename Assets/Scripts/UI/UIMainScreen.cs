using System;
using UnityEngine;

public class UIMainScreen : MonoBehaviour
{
    [SerializeField] private GameObject _warningScreen;
    [SerializeField] private float _duration = 0.75f;
    [SerializeField] private LeanTweenType _easeType;

    public void CheckToShowWarning()
    {
        bool hasPlayedTutorial = Convert.ToBoolean(PlayerPrefs.GetInt("HasPlayedTutorial", 0));
        bool hasPlayedGame = Convert.ToBoolean(PlayerPrefs.GetInt("HasPlayedGame", 0));

        if (!hasPlayedTutorial && !hasPlayedGame)
        {
            LeanTween.cancel(_warningScreen);
            _warningScreen.transform.localScale = Vector3.zero;
            _warningScreen.SetActive(true);
            LeanTween.scale(_warningScreen, Vector3.one, _duration).setEase(_easeType);

        } else
        {
            StartGame();
        }
    }
    public void StartGame()
    {
        SceneTransitionManager.instance.TransitionToDeckbuilding();
    }

    public void StartTutorial()
    {
        // Reset tutorial level index
        TutorialScriptHandler.TutorialLevelIdx = 0;

        AudioManager.instance.PlayGameMusic();
        SceneTransitionManager.instance.TransitionToTutorial();
    }
}
