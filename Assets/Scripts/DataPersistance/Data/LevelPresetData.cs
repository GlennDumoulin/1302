[System.Serializable]
public class LevelPresetData
{
    // Game Loop Data
    public GameLoopData GameLoop;

    // Campfire Data
    public CampfiresData Campfires;

    // RainMud Data
    public RainMudData RainMud;

    // Constructors
    public LevelPresetData() : this(
        new GameLoopData(),
        new CampfiresData(),
        new RainMudData()
    ) { }
    
    public LevelPresetData(
        GameLoopData gameData,
        CampfiresData campfires,
        RainMudData rainMud
    )
    {
        GameLoop = gameData;
        Campfires = campfires;
        RainMud = rainMud;
    }
}
