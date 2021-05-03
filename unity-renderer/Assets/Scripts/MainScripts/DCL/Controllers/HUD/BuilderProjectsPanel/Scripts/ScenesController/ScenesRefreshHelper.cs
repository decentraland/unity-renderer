using System.Collections.Generic;

internal class ScenesRefreshHelper
{
    public Dictionary<string, ISceneCardView> oldDeployedScenes { get; private set; }
    public Dictionary<string, ISceneCardView> oldProjectsScenes { get; private set; }
    public bool isOldDeployedScenesEmpty { get; private set; }
    public bool isOldProjectScenesEmpty { get; private set; }

    public void Set(Dictionary<string, ISceneCardView> oldDeployedScenes,
        Dictionary<string, ISceneCardView> oldProjectsScenes)
    {
        this.oldDeployedScenes = oldDeployedScenes;
        this.oldProjectsScenes = oldProjectsScenes;

        isOldDeployedScenesEmpty = oldDeployedScenes == null || oldDeployedScenes.Count == 0;
        isOldProjectScenesEmpty = oldProjectsScenes == null || oldProjectsScenes.Count == 0;
    }

    public bool WasDeployedScene(ISceneData sceneData)
    {
        if (isOldDeployedScenesEmpty)
            return false;

        return oldDeployedScenes.ContainsKey(sceneData.id);
    }

    public bool WasProjectScene(ISceneData sceneData)
    {
        if (isOldProjectScenesEmpty)
            return false;

        return oldProjectsScenes.ContainsKey(sceneData.id);
    }

    public bool IsSceneUpdate(ISceneData sceneData)
    {
        return WasDeployedScene(sceneData) || WasProjectScene(sceneData);
    }

    public bool IsSceneDeployStatusChanged(ISceneData sceneData)
    {
        return (!sceneData.isDeployed && WasDeployedScene(sceneData)) || (sceneData.isDeployed && WasProjectScene(sceneData));
    }

    public Dictionary<string, ISceneCardView> GetOldDictionary(ISceneData sceneData)
    {
        if (WasDeployedScene(sceneData))
        {
            return oldDeployedScenes;
        }
        if (WasProjectScene(sceneData))
        {
            return oldProjectsScenes;
        }

        return null;
    }
}
