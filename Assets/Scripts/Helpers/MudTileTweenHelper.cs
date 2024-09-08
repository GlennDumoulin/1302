using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MudTileTweenHelper : MonoBehaviour
{
    [SerializeField]
    private LeanTweenType _easeType;

    [SerializeField]
    private float _fadeInTime = 1f;

    private void Awake()
    {
        this.gameObject.transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        float randDelay = Random.Range(0f, 0.75f);
        LeanTween.scale(gameObject, Vector3.one, _fadeInTime).setDelay(randDelay).setEase(_easeType);
    }

    public void SetToDestroy()
    {
        float randDelay = Random.Range(0f, 0.75f);
        LeanTween.scale(gameObject, Vector3.zero, _fadeInTime).setEase(_easeType).setDelay(randDelay).setOnComplete(DestroyMud);
    }

    private void DestroyMud()
    {
        Destroy(this.gameObject);
    }
}
