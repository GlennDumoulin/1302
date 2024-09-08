using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialCampfireHelper : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _duration;
    [SerializeField] private LeanTweenType _easeType;

    private UICampfireCounter _counter;

    private void Awake()
    {
        _counter = FindObjectOfType<UICampfireCounter>();

        var defaultScale = this.gameObject.transform.localScale;
        this.gameObject.transform.localScale = Vector3.zero;

        LeanTween.scale(gameObject, defaultScale, _duration).setEase(_easeType);
    }

    private void OnEnable()
    {
        _counter.OnRadialChanged += UpdateCounter;
    }

    private void OnDisable()
    {
        _counter.OnRadialChanged -= UpdateCounter;
    }


    internal void UpdateCounter(object sender, EventArgs e)
    {
        LeanTween.value(_slider.gameObject, UpdateSlider, _slider.value, _slider.value - 0.25f, 0.5f).setOnComplete(CheckToDestroy);
    }

    internal void UpdateSlider(float value)
    {
        _slider.value = value;
    }

    private void CheckToDestroy()
    {
        if (_slider.value <= 0)
            LeanTween.scale(gameObject, Vector3.zero, _duration).setEase(_easeType).setOnComplete(DestroyGameObject);
    }

    private void DestroyGameObject()
    {
        Destroy(this.gameObject);
    }
}
