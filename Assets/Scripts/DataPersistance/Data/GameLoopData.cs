using GameEnum;
using System.Collections.Generic;

[System.Serializable]
public class GameLoopData
{
    // Game State
    public bool IsFirstTurn;
    public bool IsAiPlaying;

    public int TotalTroopsDeployed;
    public int DeployTroopLimit;

    public int PlayerSide;
    public int CurrentGameSide;
    public int StartingSide;

    public int CurrentBattlePhase;

    // Objects To Spawn
    public List<TroopData> Troops;
    public List<CardData> PlayerCards;
    public List<CardData> EnemyCards;

    // Constructors
    public GameLoopData() : this(
        true, false,
        0, 2,
        GameSides.Flemish, GameSides.Flemish, GameSides.Flemish,
        BattlePhase.DeployPhase,
        new List<TroopData>(), new List<CardData>(), new List<CardData>()
    ) { }
    
    public GameLoopData(
        bool isFirstTurn, bool isAiPlaying,
        int totalTroopsDeployed, int deployTroopLimit,
        GameSides playerSide, GameSides currentGameSide, GameSides startingSide,
        BattlePhase currentBattlePhase,
        List<TroopData> troops, List<CardData> playerCards, List<CardData> enemyCards
    )
    {
        IsFirstTurn = isFirstTurn;
        IsAiPlaying = isAiPlaying;

        TotalTroopsDeployed = totalTroopsDeployed;
        DeployTroopLimit = deployTroopLimit;

        PlayerSide = (int)playerSide;
        CurrentGameSide = (int)currentGameSide;
        StartingSide = (int)startingSide;

        CurrentBattlePhase = (int)currentBattlePhase;

        Troops = troops;
        PlayerCards = playerCards;
        EnemyCards = enemyCards;
    }
}
