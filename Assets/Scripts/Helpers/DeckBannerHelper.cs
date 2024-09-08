using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBannerHelper : MonoBehaviour
{
    [SerializeField] private GameObject _banner;
    [SerializeField] private float _duration = 0.75f;
    [SerializeField] private LeanTweenType _easeType;

    private Vector3 _defaultLocalPos;
    private Vector3 _endLocalPos;

    private void Awake()
    {
        _defaultLocalPos = _banner.transform.localPosition;
        _endLocalPos = this.gameObject.transform.localPosition;
    }

    public void MoveBannerOffscreen()
    {
        _defaultLocalPos.x = _banner.transform.localPosition.x;
        LeanTween.moveLocalX(_banner, _endLocalPos.x, _duration).setEase(_easeType).setOnComplete(HideBanner);
    }

    private void HideBanner()
    {
        _banner.SetActive(false);
    }

    public void ResetBanner()
    {
        LeanTween.cancel(_banner);
        _banner.transform.localPosition = new Vector3(_defaultLocalPos.x, _banner.transform.localPosition.y, _banner.transform.localPosition.z);
        _banner.SetActive(true);
    }


}
