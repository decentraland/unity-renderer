using System.Collections.Generic;

internal interface IProjectSceneListener
{
    void OnSetScenes(Dictionary<string, ISceneCardView> scenes);
    void OnSceneAdded(ISceneCardView scene);
    void OnSceneRemoved(ISceneCardView scene);
}
