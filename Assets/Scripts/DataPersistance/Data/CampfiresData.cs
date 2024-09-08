using System.Collections.Generic;
using GameEnum;

[System.Serializable]
public class CampfirePosition
{
    public List<int> Position;

    public CampfirePosition() : this(new List<int> { 0, 0 }) { }

    public CampfirePosition(List<int> position)
    {
        Position = position;
    }
}

[System.Serializable]
public class CampfiresData
{
    // Stats
    public int TurnsTillVictory;
    public int TurnsTillCampfireDies;
    public int CountdownCounter;
    public int SideThatCaptured;
    public int TurnCounter;
    public int SwitchCounter;
    public int CampfiresCappedByPlayer;
    public bool StartCounter;
    public bool WaitTurnAfterDestroy;

    // Objects to spawn
    public List<CampfirePosition> CampfirePositions;

    // Constructors
    public CampfiresData() : this(
        2, 5, GameSides.Flemish,
        0, 0, 0,
        0, false, false,
        new List<CampfirePosition>()
    ) { }

    public CampfiresData(
        int turnsTillVictory, int turnsTillCampfireDies, GameSides sideThatCaptured,
        int countdownCounter, int turnCounter, int switchCounter,
        int campfiresCappedByPlayer, bool startCounter, bool waitTurnAfterDestroy,
        List<CampfirePosition> campfirePositions
    )
    {
        TurnsTillVictory = turnsTillVictory;
        TurnsTillCampfireDies = turnsTillCampfireDies;
        SideThatCaptured = (int)sideThatCaptured;
        CountdownCounter = countdownCounter;
        TurnCounter = turnCounter;
        SwitchCounter = switchCounter;
        CampfiresCappedByPlayer = campfiresCappedByPlayer;
        StartCounter = startCounter;
        WaitTurnAfterDestroy = waitTurnAfterDestroy;

        CampfirePositions = campfirePositions;
    }
}
