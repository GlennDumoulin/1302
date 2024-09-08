using System.Collections.Generic;

[System.Serializable]
public class MudPosition
{
    public List<int> Position;

    public MudPosition() : this(new List<int> { 0, 0 }) { }

    public MudPosition(List<int> position)
    {
        Position = position;
    }
}

[System.Serializable]
public class RainMudData
{
    // Stats
    public bool IsActive;
    public int TurnsWithoutRain;
    public int TurnsWithMud;
    public int MaxMudTurns;
    public int CooldownCounter;
    public int CooldownLimit;
    public bool OnCooldown;
    public bool IsRaining;

    // Objects to spawn
    public List<MudPosition> MudPositions;

    // Constructors
    public RainMudData() : this(
        false,
        0, 0, 3,
        0, 1,
        false, false,
        new List<MudPosition>()
    ) { }

    public RainMudData(
        bool isActive,
        int turnsWithoutRain, int turnsWithMud, int maxMudTurns,
        int cooldownCounter, int cooldownLimit,
        bool onCooldown, bool isRaining,
        List<MudPosition> mudPositions
    )
    {
        IsActive = isActive;
        TurnsWithoutRain = turnsWithoutRain;
        TurnsWithMud = turnsWithMud;
        MaxMudTurns = maxMudTurns;
        CooldownCounter = cooldownCounter;
        CooldownLimit = cooldownLimit;
        OnCooldown = onCooldown;
        IsRaining = isRaining;

        MudPositions = mudPositions;
    }
}
