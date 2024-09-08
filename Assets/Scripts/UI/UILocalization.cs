using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UILocalization : MonoBehaviour
{
    [Header("Language Images")]
    [SerializeField]
    private Image _dutch;
    [SerializeField]
    private Image _french;
    [SerializeField]
    private Image _english;
    [SerializeField]
    private Image _german;

    [Header("Selected Sprites")]
    [SerializeField]
    private Sprite _selectedDutch;
    [SerializeField]
    private Sprite _selectedFrench;
    [SerializeField]
    private Sprite _selectedEnglish;
    [SerializeField]
    private Sprite _selectedGerman;

    private Sprite _unselectedDutch;
    private Sprite _unselectedFrench;
    private Sprite _unselectedEnglish;
    private Sprite _unselectedGerman;

    private bool _isActive = false;

    public event EventHandler OnLocaleChanged;

    private void Awake()
    {
        _unselectedDutch = _dutch.sprite;
        _unselectedFrench = _french.sprite;
        _unselectedEnglish = _english.sprite;
        _unselectedGerman = _german.sprite;
    }

    private void Start()
    {
        int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        ChangeLocale(ID);
    }

    public void ChangeLocale(int localeID)
    {
        if (_isActive)
            return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int localeID)
    {
        _isActive = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        PlayerPrefs.SetInt("LocaleKey", localeID);
        SetActiveFlag(localeID);
        _isActive = false;

        OnLocaleChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetFlagsToInactive()
    {
        _dutch.sprite = _unselectedDutch;
        _french.sprite = _unselectedFrench;
        _english.sprite = _unselectedEnglish;
        _german.sprite = _unselectedGerman;
    }

    private void SetActiveFlag(int localeID)
    {
        SetFlagsToInactive();
        switch (localeID)
        {
            case 0:
                _dutch.sprite = _selectedDutch;
                break;
            case 1:
                _english.sprite = _selectedEnglish;
                break;
            case 2:
                _french.sprite = _selectedFrench;
                break;
            case 3:
                _german.sprite = _selectedGerman;
                break;
            default:
                _dutch.sprite = _selectedDutch;
                break;
        }
    }





}
