using JetBrains.Annotations;
using GameEnum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardInfo : MonoBehaviour
{
    [Header("Card Info Fields")]
    [SerializeField]
    private GameObject _cardInfoParent = null;

    [SerializeField]
    private Image _troopImage;

    [SerializeField]
    private Image _troopSeed;

    [SerializeField]
    private TextMeshProUGUI _troopHP = null;
    [SerializeField]
    private TextMeshProUGUI _troopATK = null;
    [SerializeField]
    private TextMeshProUGUI _troopMVMT = null;
    [SerializeField]
    private TextMeshProUGUI _troopRNG = null;

    [Header("Card Name Fields")]
    [SerializeField]
    private List<GameObject> _troopNames = new List<GameObject>();

    [Header("Ability Text Fields")]
    [SerializeField]
    private GameObject _swordsmanKnightInfo = null;
    [SerializeField]
    private GameObject _archerInfo = null;
    [SerializeField]
    private GameObject _antiHorseInfo = null;
    [SerializeField]
    private GameObject _crossbowInfo = null;

    [Header("Tweening")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private LeanTweenType _easeType;


    internal void UpdateInfoScreen(Sprite cardSprite, TroopScriptableObject troopData, Sprite troopSeed, int nameIndex)
    {
        LeanTween.cancel(_cardInfoParent);
        ShowPauseScreen();
        DisableAllInfoText();
        DisableTroopName();
        _troopNames[nameIndex].SetActive(true);

        _troopImage.sprite = cardSprite;
        _troopSeed.sprite = troopSeed;
        _troopHP.text = troopData.Health.ToString();
        _troopATK.text = troopData.Damage.ToString();
        _troopMVMT.text = troopData.MovementRange.ToString();
        _troopRNG.text = troopData.AttackRange.ToString();

        if (troopData.MovementRange >= 20)
            _troopMVMT.text = "\u221E";
        if (troopData.AttackRange >= 20)
            _troopRNG.text = "\u221E";



        switch (troopData.SpecialCharacteristic)
        {
            case TroopSpecialCharacteristics.Charge:
                _swordsmanKnightInfo.SetActive(true);
                break;
            case TroopSpecialCharacteristics.UseRadius:
                _archerInfo.SetActive(true);
                break;
            case TroopSpecialCharacteristics.DamageReflect:
                _antiHorseInfo.SetActive(true);
                break;
            case TroopSpecialCharacteristics.CrossbowFireReload:
                _crossbowInfo.SetActive(true); 
                break;
        }

        _cardInfoParent.SetActive(true);

        LeanTween.scale(_cardInfoParent, Vector3.one, _duration).setEase(_easeType);
    }

    public void HidePauseScreen()
    {
        LeanTween.cancel(_cardInfoParent);
        LeanTween.scale(_cardInfoParent, Vector3.zero, _duration).setEase(_easeType).setOnComplete(DisableCardInfo);
    }
    
    private void DisableCardInfo()
    {
        _cardInfoParent.SetActive(false);
    }


    private void ShowPauseScreen()
    {
        _cardInfoParent.SetActive(true);
    }

    private void DisableAllInfoText()
    {
        _swordsmanKnightInfo.SetActive(false);
        _archerInfo.SetActive(false);
        _antiHorseInfo.SetActive(false);
        _crossbowInfo.SetActive(false);
    }

    private void DisableTroopName()
    {
        foreach (GameObject go in _troopNames)
            go.SetActive(false);
    }
}
