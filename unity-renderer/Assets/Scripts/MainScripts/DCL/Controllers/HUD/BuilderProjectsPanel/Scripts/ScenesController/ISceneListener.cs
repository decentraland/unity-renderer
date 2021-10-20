using System.Collections.Generic;

internal interface ISceneListener
{
    void SetScenes(Dictionary<string, ISceneCardView> scenes);
    void SceneAdded(ISceneCardView scene);
    void SceneRemoved(ISceneCardView scene);
}