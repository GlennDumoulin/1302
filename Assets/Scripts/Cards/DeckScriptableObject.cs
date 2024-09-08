using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DeckObject")]

public class DeckScriptableObject : ScriptableObject
{
    public List<GameObject> Deck;
}
