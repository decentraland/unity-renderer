using System.Collections.Generic;

internal interface IDeployedSceneListener
{
    void OnSetScenes(Dictionary<string, SceneCardView> scenes);
    void OnSceneAdded(SceneCardView scene);
    void OnSceneRemoved(SceneCardView scene);
}
