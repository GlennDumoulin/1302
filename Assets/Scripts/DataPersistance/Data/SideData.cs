using System.Collections.Generic;

[System.Serializable]

public class SideData
{
    public List<TroopAction> Actions;
    public List<DeploymentData> Deployments;
    public bool IsFlemish;

    public SideData() : this(new List<TroopAction>(), new List<DeploymentData>(), true) { }

    public SideData(List<TroopAction> actions, List<DeploymentData> deployments, bool isFlemish)
    {
        IsFlemish = isFlemish;
        Actions = actions;
        Deployments = deployments;
    }
}
