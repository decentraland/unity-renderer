using System.Collections.Generic;

internal class PlacesRefreshHelper
{
    public Dictionary<string, IPlaceCardView> oldDeployedScenes { get; private set; }
    public Dictionary<string, IPlaceCardView> oldProjectsScenes { get; private set; }
    public bool isOldDeployedScenesEmpty { get; private set; }
    public bool isOldProjectScenesEmpty { get; private set; }

    public void Set(Dictionary<string, IPlaceCardView> oldDeployedScenes,
        Dictionary<string, IPlaceCardView> oldProjectsScenes)
    {
        this.oldDeployedScenes = oldDeployedScenes;
        this.oldProjectsScenes = oldProjectsScenes;

        isOldDeployedScenesEmpty = oldDeployedScenes == null || oldDeployedScenes.Count == 0;
        isOldProjectScenesEmpty = oldProjectsScenes == null || oldProjectsScenes.Count == 0;
    }

    public bool WasDeployedScene(IPlaceData placeData)
    {
        if (isOldDeployedScenesEmpty)
            return false;

        return oldDeployedScenes.ContainsKey(placeData.id);
    }

    public bool WasProjectScene(IPlaceData placeData)
    {
        if (isOldProjectScenesEmpty)
            return false;

        return oldProjectsScenes.ContainsKey(placeData.id);
    }

    public bool IsSceneUpdate(IPlaceData placeData) { return WasDeployedScene(placeData) || WasProjectScene(placeData); }

    public bool IsSceneDeployStatusChanged(IPlaceData placeData) { return (!placeData.isDeployed && WasDeployedScene(placeData)) || (placeData.isDeployed && WasProjectScene(placeData)); }

    public Dictionary<string, IPlaceCardView> GetOldDictionary(IPlaceData placeData)
    {
        if (WasDeployedScene(placeData))
        {
            return oldDeployedScenes;
        }
        if (WasProjectScene(placeData))
        {
            return oldProjectsScenes;
        }

        return null;
    }
}