using System.Collections.Generic;

internal interface IProjectSceneListener
{
    void OnSetScenes(Dictionary<string, SceneCardView> scenes);
    void OnSceneAdded(SceneCardView scene);
    void OnSceneRemoved(SceneCardView scene);
}
