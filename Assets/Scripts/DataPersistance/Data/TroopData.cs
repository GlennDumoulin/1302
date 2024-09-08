using System;
using System.Collections.Generic;

[Serializable]
public class TroopData
{
    // Stats
    public int HP;
    public int ShieldPoints;
    public bool ShieldBroken;

    // Properties
    public bool ShouldReload;
    public bool ReloadThisTurn;
    public bool HasActed;
    public bool CanMove;

    // Spawn Info
    public string AssetKey;
    public List<int> CurrentHexagonCoordinates;

    // Constructors
    public TroopData() : this(
        1, 0, false,
        false, false, true, true,
        string.Empty, new List<int> { 0, 0 }
    ) { }
    
    public TroopData(
        int hp, int shieldPoints, bool shieldBroken,
        bool shouldReload, bool reloadThisTurn, bool hasActed, bool canMove,
        string assetKey, List<int> currentHexagonCoordinates
    )
    {
        HP = hp;
        ShieldPoints = shieldPoints;
        ShieldBroken = shieldBroken;

        ShouldReload = shouldReload;
        ReloadThisTurn = reloadThisTurn;
        HasActed = hasActed;
        CanMove = canMove;

        AssetKey = assetKey;
        CurrentHexagonCoordinates = currentHexagonCoordinates;
    }
}
