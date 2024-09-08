using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _blackScreen;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Positions")]
    [SerializeField] private GameObject _topPosition;
    [SerializeField] private GameObject _leftPosition;
    [SerializeField] private GameObject _rightPosition;
    [SerializeField] private GameObject _bottomPosition;
    [SerializeField] private GameObject _centerPosition;

    [Header("Tweening")]
    [SerializeField] private float _duration;
    [SerializeField] private LeanTweenType _easeOutType;
    [SerializeField] private LeanTweenType _easeInType;

    public static SceneTransitionManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += FadeOut;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= FadeOut;
    }

    public void TransitionToDeckbuilding()
    {
        _blackScreen.SetActive(true);

        LeanTween.alphaCanvas(_canvasGroup, 1f, _duration).setEase(_easeOutType).setIgnoreTimeScale(true).setOnComplete(LoadDeckbuildingScene);
    }

    public void TransitionToMain()
    {
        _blackScreen.SetActive(true);

        LeanTween.alphaCanvas(_canvasGroup, 1f, _duration).setEase(_easeOutType).setIgnoreTimeScale(true).setOnComplete(LoadMainMenuScene);
    }

    public void TransitionToTutorial()
    {
        _blackScreen.SetActive(true);

        LeanTween.alphaCanvas(_canvasGroup, 1f, _duration).setEase(_easeOutType).setIgnoreTimeScale(true).setOnComplete(LoadTutorialScene);
    }

    public void TransitionToGame()
    {
        _blackScreen.SetActive(true);

        LeanTween.alphaCanvas(_canvasGroup, 1f, _duration).setEase(_easeOutType).setIgnoreTimeScale(true).setOnComplete(LoadMainGameScene);
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadSceneAsync("MainMenuScene", LoadSceneMode.Single);
    }

    private void LoadDeckbuildingScene()
    {
        SceneManager.LoadSceneAsync("Deckbuilding", LoadSceneMode.Single);
    }

    private void LoadMainGameScene()
    {
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
    }

    private void LoadTutorialScene()
    {
        SceneManager.LoadSceneAsync("Tutorial", LoadSceneMode.Single);
    }

    private void FadeOut(Scene scene, LoadSceneMode mode)
    {
        LeanTween.alphaCanvas(_canvasGroup, 0f, _duration).setEase(_easeInType).setIgnoreTimeScale(true);
    }

    private void ResetToDefault()
    {
        _blackScreen.SetActive(false);
    }
}
