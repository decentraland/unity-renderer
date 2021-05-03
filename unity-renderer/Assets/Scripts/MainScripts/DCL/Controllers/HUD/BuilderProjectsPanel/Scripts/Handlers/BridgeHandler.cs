using System;
using DCL.Helpers;

internal class BridgeHandler : IDisposable
{
    private readonly IBuilderProjectsPanelBridge bridge;
    private readonly IScenesViewController scenesViewController;
    private readonly ILandController landsController;
    private readonly ISectionsController sectionsController;

    public BridgeHandler(IBuilderProjectsPanelBridge bridge, IScenesViewController scenesViewController, ILandController landsController, ISectionsController sectionsController)
    {
        if (bridge == null)
            return;
        
        this.bridge = bridge;
        this.scenesViewController = scenesViewController;
        this.landsController = landsController;
        this.sectionsController = sectionsController;

        bridge.OnProjectsSet += OnProjectsUpdated;
        
        sectionsController.OnRequestUpdateSceneData += OnRequestUpdateSceneData;
        sectionsController.OnRequestUpdateSceneContributors += OnRequestUpdateSceneContributors;
        sectionsController.OnRequestUpdateSceneAdmins += OnRequestUpdateSceneAdmins;
        sectionsController.OnRequestUpdateSceneBannedUsers += OnRequestUpdateSceneBannedUsers;
        
        bridge.SendFetchProjects();
    }

    public void Dispose()
    {
        if (bridge != null)
        {
            bridge.OnProjectsSet -= OnProjectsUpdated;
        }

        if (sectionsController != null)
        {
            sectionsController.OnRequestUpdateSceneData -= OnRequestUpdateSceneData;
            sectionsController.OnRequestUpdateSceneContributors -= OnRequestUpdateSceneContributors;
            sectionsController.OnRequestUpdateSceneAdmins -= OnRequestUpdateSceneAdmins;
            sectionsController.OnRequestUpdateSceneBannedUsers -= OnRequestUpdateSceneBannedUsers;
        }
    }
    
    private void OnProjectsUpdated(string payload)
    {
        if (scenesViewController != null)
        {
            var scenes = Utils.ParseJsonArray<SceneData[]>(payload);
            scenesViewController.SetScenes(scenes);
        }
    }

    private void OnRequestUpdateSceneData(string id, SceneDataUpdatePayload dataUpdatePayload)
    {
        bridge?.SendSceneDataUpdate(id, dataUpdatePayload);
    }

    private void OnRequestUpdateSceneContributors(string id, SceneContributorsUpdatePayload payload)
    {
        bridge?.SendSceneContributorsUpdate(id, payload);
    }
    
    private void OnRequestUpdateSceneAdmins(string id, SceneAdminsUpdatePayload payload)
    {
        bridge?.SendSceneAdminsUpdate(id, payload);
    }
    
    private void OnRequestUpdateSceneBannedUsers(string id, SceneBannedUsersUpdatePayload payload)
    {
        bridge?.SendSceneBannedUsersUpdate(id, payload);
    }
}
