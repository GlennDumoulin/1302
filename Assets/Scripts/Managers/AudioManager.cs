using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioClip _youWinMusic;
    [SerializeField] private AudioClip _youLoseMusic;

    [Header("SFXs")]
    [SerializeField] private AudioClip _buttonReturnSFX;
    [SerializeField] private AudioClip _buttonForwardSFX;
    [SerializeField] private AudioClip _buttonToBattleSFX;


    public static AudioManager instance;
    public AudioSource MusicSource { get { return _musicSource; } }

    public AudioSource SFXSource { get { return _sfxSource; } }

    private float _defaultMusicVolume;
    public float DefaultMusicVolume {  get { return _defaultMusicVolume; } }

    private float _defaultSFXVolume = 1f;
    public float DefaultSFXVolume { get { return _defaultSFXVolume;} }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        _defaultMusicVolume = _musicSource.volume;
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        _musicSource.clip = _menuMusic;
        _musicSource.Play();
    }

    public void PlayGameMusic()
    {
        _musicSource.clip = _gameMusic;
        _musicSource.Play();
    }

    public void PlayYouWinMusic()
    {
        _musicSource.clip = _youWinMusic;
        _musicSource.Play();
    }

    public void PlayYouLoseMusic()
    {
        _musicSource.clip = _youLoseMusic;
        _musicSource.Play();
    }

    public void CheckToEnableMusic(bool play)
    {
        if (play)
            _musicSource.volume = _defaultMusicVolume;
        else
            _musicSource.volume = 0f;
    }

    public void CheckToEnableSFXs(bool play)
    {
        if (play)
            _sfxSource.volume = 1f;
        else
            _sfxSource.volume = 0f;
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        SFXSource.PlayOneShot(clip, volume * SFXSource.volume);
    }

    public void PlayButtonClick()
    {
        SFXSource.PlayOneShot(_buttonReturnSFX, SFXSource.volume * 0.25f);
    }

    public void PlayButtonForwardClick()
    {
        SFXSource.PlayOneShot(_buttonForwardSFX, SFXSource.volume * 0.25f);
    }

    public void PlayButtonBattle()
    {
        SFXSource.PlayOneShot(_buttonToBattleSFX, SFXSource.volume * 0.25f);
    }
}
