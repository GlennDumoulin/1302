using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSFXHelper : MonoBehaviour
{
    [SerializeField]
    private Toggle _toggleButton;

    public event EventHandler OnSFXVolumeChanged;

    public void CheckToEnableSFX(bool play)
    {
        PlayerPrefs.SetInt("SFXEnabled", play ? 1 : 0);

        if (play)
            AudioManager.instance.SFXSource.volume = AudioManager.instance.DefaultSFXVolume;
        else
            AudioManager.instance.SFXSource.volume = 0f;

        OnSFXVolumeChanged?.Invoke(this, new EventArgs());
    }

    private void Start()
    {
        if (AudioManager.instance == null)
            return;

        bool shouldPlay = Convert.ToBoolean(PlayerPrefs.GetInt("SFXEnabled", 1));

        _toggleButton.isOn = shouldPlay;
        CheckToEnableSFX(shouldPlay);
    }
}
