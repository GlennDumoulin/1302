using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITroopHPManager : MonoBehaviour
{
    public event EventHandler OnUpdateUIDirection;

    private Camera _mainCamera = null;

    private float _fadeDuration = 0.5f;
    private float _hpDuration = 1.75f;
    [SerializeField]
    private TroopModel _troop = null;
    [SerializeField]
    private GameObject _hpBar = null;
    [SerializeField]
    private GameObject _hp1 = null;
    [SerializeField] 
    private GameObject _hp2 = null;
    [SerializeField]
    private GameObject _hp3 = null;

    private CanvasGroup _canvasGroup = null;

    private CanvasGroup _hp1Canvas = null;
    private CanvasGroup _hp2Canvas = null;
    private CanvasGroup _hp3Canvas = null;

    private int _projectedHP = 0;
    private bool _hpShowing = false;
    private bool _shouldRepeat = false;
    private float _pulseFrequency = 1f;

    private void Awake()
    {
        //Get camera
        _mainCamera = FindObjectOfType<Camera>();

        //Link the events
        _troop.OnTroopAttacked += DamagedUnit; //Show HP when a troop is damaged

        //Get the HP canvas 
        _canvasGroup = _hpBar.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0.0f;

        _hp1Canvas = _hp1.GetComponent<CanvasGroup>();
        if (_hp2 != null)
            _hp2Canvas = _hp2.GetComponent<CanvasGroup>();
        if (_hp3 != null)
            _hp3Canvas = _hp3.GetComponent<CanvasGroup>();

    }

    private void Start()
    {
        //only fade in/out when it is not in the deployment phase
        /*
        if(!FindObjectOfType<GameLoopManager>().CurrentBattlePhase.Equals(GameEnum.BattlePhase.DeployPhase))
            DamagedUnit(this, EventArgs.Empty);
        */

        ShowHP(true);
    }

    private void ShowHP(bool fadeOutAgain)
    {
        //Show HP
        switch (_troop.HP)
        {
            case 0:
                _hp1.SetActive(false);
                if (_hp2 != null)
                    _hp2.SetActive(false);
                if (_hp3 != null)
                    _hp3.SetActive(false);
                break;
            case 1:
                if (_hp2 != null)
                    _hp2.SetActive(false);
                if (_hp3 != null)
                    _hp3.SetActive(false);
                break;
            case 2:
                if (_hp3 != null)
                    _hp3?.gameObject.SetActive(false);
                break;
        }

        //Hp shows
        _hpShowing = true;

        //Start fading in/out
        if (fadeOutAgain)
            FadeInAndOut();
        else
            FadeInOnly();

        //StartCoroutine(FadeIn(0f, 1f, _fadeDuration, fadeOutAgain));
    }


    //Use this function if you wish to have the HP Bar fade in and then fade out
    private void FadeInAndOut()
    {
        LookAtCamera();
        LeanTween.alphaCanvas(_canvasGroup, 1f, _fadeDuration).setOnComplete(FadeOutDelay);
    }

    public void FadeInOnly()
    {
        LookAtCamera();
        LeanTween.alphaCanvas(_canvasGroup, 1f, _fadeDuration);
    }

    //This function will start fading out the HP bar immedately 
    public void FadeOutImmedately()
    {
        _hpShowing = false;
        LookAtCamera();
        LeanTween.alphaCanvas(_canvasGroup, 0f, _fadeDuration).setOnComplete(SetAlphaCanvasOfAllHP);
    }

    //This function will start fading out the HP Bar after the given delay duration
    private void FadeOutDelay()
    {
        StopLoopTroopHP();
        LookAtCamera();
        _hpShowing = false;
        //The HP should start fading out after it has been on screen for atleast the time specified
        LeanTween.alphaCanvas(_canvasGroup, 0f, _fadeDuration).setDelay(_hpDuration).setOnComplete(SetAlphaCanvasOfAllHP);
    }

    private void TacticalView(object sender, EventArgs e)
    {
        if(!_hpShowing)
        {
            //Show it's HP
            ShowHP(false);
        }
    }

    internal void LookAtCamera()
    {
        transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward, _mainCamera.transform.rotation * Vector3.up);
        OnUpdateUIDirection?.Invoke(this, EventArgs.Empty);
    }

    private void DamagedUnit(object sender, EventArgs e)
    {
        //Show it's HP
        ShowHP(true);
    }

    public void LoopTroopHPOpacity(int damage)
    {
        //Tell the UI to start pulsing
        _shouldRepeat = true;

        _projectedHP = (_troop.HP + _troop.ShieldPoints) - damage;

        if (_hp3Canvas != null)
            _hp3Canvas.alpha = 1f;
        if (_hp2Canvas != null)
            _hp2Canvas.alpha = 1f;
        if (_hp3Canvas != null)
            _hp3Canvas.alpha = 1f;

        FadeHPBarsInAndOut();
    }

    public void StopLoopTroopHP()
    {
        //After a pulse completes, do not pulse again
        _shouldRepeat = false;
    }

    private void FadeHPBarsInAndOut()
    {
        //Pulse the correct HP bars
        if (_projectedHP < 3 && _hp3Canvas != null)
        {
            LeanTween.alphaCanvas(_hp3Canvas, 0.1f, _pulseFrequency / 2);
            LeanTween.alphaCanvas(_hp3Canvas, 1f, _pulseFrequency / 2).setDelay(_pulseFrequency / 2);
        }

        if (_projectedHP < 2 && _hp2Canvas != null)
        {
            LeanTween.alphaCanvas(_hp2Canvas, 0.1f, _pulseFrequency / 2);
            LeanTween.alphaCanvas(_hp2Canvas, 1f, _pulseFrequency / 2).setDelay(_pulseFrequency / 2);
        }

        if (_projectedHP < 1 && _hp1Canvas != null)
        {
            LeanTween.alphaCanvas(_hp1Canvas, 0.1f, _pulseFrequency / 2);
            LeanTween.alphaCanvas(_hp1Canvas, 1f, _pulseFrequency / 2).setDelay(_pulseFrequency / 2);
        }

        //After they have all pulsed once, check if they should pulse again
        LeanTween.alpha(this.gameObject, 1f, _pulseFrequency).setOnComplete(CheckToRepeat);
    }

    private void CheckToRepeat()
    {
        //If they should, call the method again.
        if (_shouldRepeat)
            FadeHPBarsInAndOut();
        else
            SetAlphaCanvasOfAllHP();
    }

    private void SetAlphaCanvasOfAllHP()
    {
        //To avoid any mismatched or not full alphas, set all of the alphas back to full (1f)
        if (_hp3Canvas != null)
            _hp3Canvas.alpha = 1f;
        if (_hp2Canvas != null)
            _hp2Canvas.alpha = 1f;
        if (_hp3Canvas != null)
            _hp3Canvas.alpha = 1f;
    }
}
