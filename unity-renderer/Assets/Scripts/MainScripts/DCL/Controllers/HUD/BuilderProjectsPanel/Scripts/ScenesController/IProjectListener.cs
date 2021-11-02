using System.Collections.Generic;

internal interface IProjectListener
{
    void SetScenes(Dictionary<string, ISceneCardView> scenes);
    void SceneAdded(ISceneCardView scene);
    void SceneRemoved(ISceneCardView scene);
}