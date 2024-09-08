using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleAudioHelper : MonoBehaviour
{
    [SerializeField]
    private Toggle _toggleButton;

    public void CheckToEnableMusic(bool play)
    {
        PlayerPrefs.SetInt("MusicEnabled", play ? 1 : 0);

        if (play)
            AudioManager.instance.MusicSource.volume = AudioManager.instance.DefaultMusicVolume;
        else
            AudioManager.instance.MusicSource.volume = 0f;
    }

    private void Start()
    {
        if (AudioManager.instance == null)
            return;

        bool shouldPlay = Convert.ToBoolean(PlayerPrefs.GetInt("MusicEnabled", 1));

        _toggleButton.isOn = shouldPlay;
        CheckToEnableMusic(shouldPlay);
    }
}
