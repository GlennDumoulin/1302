using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SFXHelper : MonoBehaviour
{
    private Dictionary<AudioSource, float> _audioClipVolume = new Dictionary<AudioSource, float>();

    private ToggleSFXHelper _sfxToggle;

    private void Awake()
    {
        _sfxToggle = FindObjectOfType<ToggleSFXHelper>();
    }

    private void OnEnable()
    {
        _sfxToggle.OnSFXVolumeChanged += UpdateContiniousSFXVolume;
    }

    private void OnDisable()
    {
        _sfxToggle.OnSFXVolumeChanged -= UpdateContiniousSFXVolume;
    }

    public void PlaySFXClipOnce(AudioSource audioSource)
    {
        audioSource.PlayOneShot(audioSource.clip, AudioManager.instance.SFXSource.volume);
    }

    public void PlaySFXClipContiniously(AudioSource source)
    {
        if (!_audioClipVolume.ContainsKey(source))
            _audioClipVolume.Add(source, source.volume);

        if (!_audioClipVolume.TryGetValue(source, out float volume))
            return;

        source.volume = volume * AudioManager.instance.SFXSource.volume;
        source.Play();
    }

    public void StopSFXClip(AudioSource audioSource)
    {
        audioSource.Stop();
    }

    private void UpdateContiniousSFXVolume(object sender, EventArgs e)
    {
        foreach (AudioSource audio in _audioClipVolume.Keys)
        {
            if (!_audioClipVolume.TryGetValue(audio, out float volume))
                continue;

            audio.volume = volume * AudioManager.instance.SFXSource.volume;
        }
    }

    public void PlaySFXOnceFromManager(AudioSource audioSource)
    {
        AudioManager.instance.PlaySFX(audioSource.clip, audioSource.volume);
    }
}
