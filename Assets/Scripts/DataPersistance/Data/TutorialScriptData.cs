using System.Collections.Generic;

[System.Serializable]
public class TutorialScriptData
{
    public List<TurnData> Turns;

    public TutorialScriptData() : this(new List<TurnData>()) { }

    public TutorialScriptData(List<TurnData> turns)
    {
        Turns = turns;
    }
}
