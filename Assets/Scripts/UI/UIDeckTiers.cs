using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeckTiers : MonoBehaviour
{
    [SerializeField]
    private UITier _tierOne = null;
    [SerializeField]
    private UITier _tierTwo = null;
    [SerializeField]
    private UITier _tierThree = null;
    [SerializeField]
    private UITier _tierFour = null;


    private void Start()
    {
        HighlightTier(1);
    }


    public void HighlightTier(int tier)
    {
        UnhighlightAll();
        switch (tier)
        {
            case 1:
                _tierOne.HighlightText(); 
                break;
            case 2:
                _tierTwo.HighlightText(); 
                break;
            case 3:
                _tierThree.HighlightText(); 
                break;
            case 4:
                _tierFour.HighlightText(); 
                break;
            default:
                _tierOne.HighlightText(); 
                break;
        }
    }


    private void UnhighlightAll()
    {
        _tierOne.UnhighlightText();
        _tierTwo.UnhighlightText();
        _tierThree.UnhighlightText();
        _tierFour.UnhighlightText();
    }
}
