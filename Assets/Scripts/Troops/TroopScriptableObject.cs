using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;

[CreateAssetMenu(fileName = "TroopCardObject")]

/*
 * Instead of a Bool to check whether the troop can charge or can reflect damage, an Enum class was created
 * Since a troop can only have one special characteristic at a time, this seemed like the best solution 
 */

public class TroopScriptableObject : ScriptableObject
{
    public GameSides Side = GameSides.French;
    public int Health = 1;
    public int Damage = 1;
    public int Shield = 0;
    public int MovementRange = 1;
    public int AttackRange = 1;
    public int MovementSpeed = 10;
    public int ManpowerCost = 1;
    public TroopSpecialCharacteristics SpecialCharacteristic = TroopSpecialCharacteristics.Charge;
    public bool IsDamageReflectable = false;
    public bool CanAttackNeigbours = true;
}
