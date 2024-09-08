using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TroopAction
{
    public int TroopIdx;
    public List<int> TargetPos;
    public string Instruction;
    public bool IsPlayerAction;
    public bool IsDeploy;
    public bool IsInstructionOnly;
    public int InstructionInfo;

    public TroopAction() : this(0, new List<int>{ 0, 0 }, "", false, false, false, 0) { }

    public TroopAction(int troopIdx, List<int> targetPos, string instruction, bool isPlayerAction, bool isDeploy, bool isInstructionOnly, int instructionInfo)
    {
        TroopIdx = troopIdx;
        TargetPos = targetPos;
        Instruction = instruction;
        IsPlayerAction = isPlayerAction;
        IsDeploy = isDeploy;
        IsInstructionOnly = isInstructionOnly;
        InstructionInfo = instructionInfo;
    }
}
