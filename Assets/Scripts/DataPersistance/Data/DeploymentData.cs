using System.Collections.Generic;

[System.Serializable]
public class DeploymentData
{
    public int CardIdx;
    public List<int> TargetPos;
    public string Instruction;
    public bool IsPlayerAction;
    public bool IsInstructionOnly;
    public int InstructionInfo;

    public DeploymentData() : this(0, new List<int> { 0, 0 }, "", false, false, 0) { }

    DeploymentData(int cardIdx, List<int> targetPos, string instruction, bool isPlayerAction, bool isInstructionOnly, int instructionInfo)
    {
        CardIdx = cardIdx;
        TargetPos = targetPos;
        Instruction = instruction;
        IsPlayerAction = isPlayerAction;
        IsInstructionOnly = isInstructionOnly;
        InstructionInfo = instructionInfo;
    }
}
