using System.Collections.Generic;

internal interface IDeployedSceneListener
{
    void OnSetScenes(Dictionary<string, ISceneCardView> scenes);
    void OnSceneAdded(ISceneCardView scene);
    void OnSceneRemoved(ISceneCardView scene);
}
