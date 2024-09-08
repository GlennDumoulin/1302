[System.Serializable]
public class TurnData
{
    public SideData FlemishSide;
    public SideData FrenchSide;
    public bool ExplainNextTurn;

    public TurnData() : this(new SideData(), new SideData(), false) { }

    public TurnData(SideData flemishSide, SideData frenchSide, bool explainNextTurn)
    {
        FlemishSide = flemishSide;
        FrenchSide = frenchSide;
        ExplainNextTurn = explainNextTurn;
    }
}
