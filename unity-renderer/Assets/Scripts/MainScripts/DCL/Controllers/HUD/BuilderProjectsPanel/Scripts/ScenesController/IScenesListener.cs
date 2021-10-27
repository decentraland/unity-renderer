using System.Collections.Generic;

internal interface IScenesListener
{
    void OnSetScenes(Dictionary<string, ISceneCardView> scenes);
    void OnSceneAdded(ISceneCardView scene);
    void OnSceneRemoved(ISceneCardView scene);
}