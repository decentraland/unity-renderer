using System.Collections.Generic;

/// <summary>
/// Th
/// </summary>
internal interface ISceneListener
{
    void SetScenes(Dictionary<string, ISceneCardView> scenes);
    void SceneAdded(ISceneCardView scene);
    void SceneRemoved(ISceneCardView scene);
}