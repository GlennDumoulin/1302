using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITier : MonoBehaviour
{
    [SerializeField]
    private Image _knightHelmet;
    [SerializeField]
    private TextMeshProUGUI _tier;
    [SerializeField]
    private TextMeshProUGUI _manpowerCount;
    [SerializeField]
    private Color _highlightColor;

    internal void HighlightText()
    {
        _knightHelmet.color = _highlightColor;
        _tier.color = _highlightColor;
        _manpowerCount.color = _highlightColor;
    }

    internal void UnhighlightText()
    {
        _knightHelmet.color = Color.white;
        _tier.color = Color.white;
        _manpowerCount.color = Color.white;
    }
}
