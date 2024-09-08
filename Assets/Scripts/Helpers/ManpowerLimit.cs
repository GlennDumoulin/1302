using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManpowerLimit : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField]
    private GameObject _manpowerObject = null;
    [SerializeField]
    private TextMeshProUGUI _manpowerCounter;
    [SerializeField]
    private Image _confirmButton = null;
    [SerializeField]
    private GameObject _confirmGlow = null;
    [SerializeField]
    private Sprite _continueIcon = null;
    [SerializeField]
    private Sprite _randomIcon = null;
    [SerializeField]
    private Color _randomColor = Color.gray;
    [SerializeField]
    private Color _continueColor = Color.white;

    [Header("Tweening")]
    [SerializeField]
    private float _duration = 0.1f;
    [SerializeField]
    private LeanTweenType _easeType;

    private int _currentManpower = 0;
    public int CurrentManpower { get { return _currentManpower; } }

    private void Awake()
    {
        UpdateConfirmIcon();
    }

    public bool UpdateManpower(int count)
    {
        if (_currentManpower + count > 20)
        {
            WarnLimit();
            return false;
        }

        if (_currentManpower + count > -1)
        {
            _currentManpower += count;
            _manpowerCounter.text = $"{_currentManpower}/20";
            UpdateConfirmIcon();
            return true;
        }

        return false;
    }

    internal void ResetManpower()
    {
        _currentManpower = 0;
        _manpowerCounter.text = $"{_currentManpower}/20";
    }

    private void WarnLimit()
    {
        LeanTween.cancel(_manpowerObject);

        LeanTween.scale(_manpowerObject, new Vector3(1.2f, 1.2f, 1.2f), _duration).setEase(_easeType);
        LeanTween.value(_manpowerObject, UpdateTextColorToRed, Color.white, Color.red, _duration).setEase(_easeType).setOnComplete(ScaleDownLimit);
    }

    private void ScaleDownLimit()
    {
        LeanTween.scale(_manpowerObject, Vector3.one, _duration).setEase(_easeType);
        LeanTween.value(_manpowerObject, UpdateTextColorToWhite, Color.red, Color.white, _duration).setEase(_easeType);
    }

    private void UpdateTextColorToRed(Color color)
    {
        _manpowerCounter.color = color;
    }

    private void UpdateTextColorToWhite(Color color)
    {
        _manpowerCounter.color = color;
    }

    private void UpdateConfirmIcon()
    {
        if (_currentManpower != 20)
        {
            _confirmButton.sprite = _randomIcon;
            _confirmButton.color = _randomColor;
            _confirmGlow.SetActive(false);
        }
        else if (_currentManpower ==  20)
        {
            _confirmButton.sprite = _continueIcon;
            _confirmButton.color = _continueColor;
            _confirmGlow.SetActive(true);
        }
    }
}
