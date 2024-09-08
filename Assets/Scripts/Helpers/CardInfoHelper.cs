using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoHelper : MonoBehaviour
{
    [SerializeField]
    private Sprite _sprite;
    [SerializeField]
    private TroopScriptableObject _troopData;
    [SerializeField]
    private Sprite _troopSeed;
    [SerializeField]
    private int _nameIndex = 0;

    private UICardInfo _cardInfo;

    private void Awake()
    {
        _cardInfo = FindObjectOfType<UICardInfo>();
    }

    public void ShowCardInfo()
    {
        _cardInfo.UpdateInfoScreen(_sprite, _troopData, _troopSeed, _nameIndex);
    }
}
