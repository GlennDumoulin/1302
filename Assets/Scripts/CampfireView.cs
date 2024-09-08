using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CampfireView : MonoBehaviour
{

    [Header("Campfire Warning")]
    [SerializeField] private ParticleSystem _fireRFXParent;

    [SerializeField] private GameObject _fireChildRFX;
    [SerializeField] private GameObject _fireChildRFX1;

    [SerializeField] private ParticleSystem _fireChildParticle;
    [SerializeField] private ParticleSystem _fireChildParticle1;

    [SerializeField] private LeanTweenType _scaleUpEase;
    [SerializeField] private LeanTweenType _scaleDownEase;
    [SerializeField] private Vector3 _baseScale;

    [SerializeField] private Color _smokeColor;



    [Header("Campfire Destroy")]
    [SerializeField] private GameObject _destoryRFX;
    public UnityEvent OnCampsiteExtinguished;


    internal void ScaleUpFlame()
    {
        _fireRFXParent.Play(true);
        _fireChildRFX.transform.localScale = _baseScale;
        _fireChildRFX.transform.localScale = _baseScale;

        LeanTween.scale(_fireChildRFX, _baseScale * 2.25f, 1f).setEase(_scaleUpEase);
        LeanTween.scale(_fireChildRFX1, _baseScale * 2.25f, 1f).setEase(_scaleUpEase).setOnComplete(DisableFlame);

    }
    

    private void DisableFlame()
    {
        _fireRFXParent.Stop(true);

        LeanTween.scale(_fireChildRFX, _baseScale, 0.35f).setEase(_scaleDownEase);
        LeanTween.scale(_fireChildRFX1, _baseScale, 0.35f).setEase(_scaleDownEase).setOnComplete(ChangeFlame);
    }

    private void ChangeFlame()
    {
        StartCoroutine(SetFlameToSmoke(1f));
    }

    internal void SpawnSmokeRFX()
    {
        LeanTween.cancel(_fireChildRFX);
        LeanTween.cancel(_fireChildRFX1);

        OnCampsiteExtinguished?.Invoke();
        Instantiate(_destoryRFX, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator SetFlameToSmoke(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Destroy the game object
        _fireChildRFX.transform.localScale = _baseScale;
        _fireChildRFX1.transform.localScale = _baseScale;

        ParticleSystem.MainModule mainModule = _fireChildParticle.main;
        mainModule.startColor = _smokeColor;
        ParticleSystem.MainModule otherMainModule = _fireChildParticle1.main;
        otherMainModule.startColor = _smokeColor;

        _fireRFXParent.Play(true);
    }
}
