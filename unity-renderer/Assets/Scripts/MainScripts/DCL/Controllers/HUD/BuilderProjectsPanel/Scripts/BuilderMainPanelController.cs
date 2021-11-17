using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Builder;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using Variables.RealmsInfo;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class BuilderMainPanelController : IHUD, IBuilderMainPanelController
{
    private const string CREATING_PROJECT_ERROR = "Error creating a new project: ";
    private const string TESTING_ETH_ADDRESS = "0xDc13378daFca7Fe2306368A16BCFac38c80BfCAD";
    private const string TESTING_TLD = "org";
    private const string VIEW_PREFAB_PATH = "BuilderProjectsPanel";
    private const string VIEW_PREFAB_PATH_DEV = "BuilderProjectsPanelDev";

    private const float CACHE_TIME_LAND = 5 * 60;
    private const float CACHE_TIME_SCENES = 1 * 60;
    private const float REFRESH_INTERVAL = 2 * 60;

    internal IBuilderMainPanelView view;

    private ISectionsController sectionsController;
    private IProjectsController projectsController;
    private IScenesViewController scenesViewController;
    private ILandsController landsesController;
    private UnpublishPopupController unpublishPopupController;

    private INewProjectFlowController newProjectFlowController;

    private SectionsHandler sectionsHandler;
    private SceneContextMenuHandler sceneContextMenuHandler;
    private LeftMenuHandler leftMenuHandler;
    private LeftMenuSettingsViewHandler leftMenuSettingsViewHandler;

    private ITheGraph theGraph;
    private ICatalyst catalyst;

    private bool isInitialized = false;
    internal bool isFetchingLands = false;
    internal bool isFetchingProjects = false;
    private bool sendPlayerOpenPanelEvent = false;
    private Coroutine fetchDataInterval;
    private Promise<LandWithAccess[]> fetchLandPromise = null;
    private Promise<ProjectData[]> fetchProjectsPromise = null;

    public event Action OnJumpInOrEdit;

    internal IContext context;

    public BuilderMainPanelController()
    {
        if (DataStore.i.builderInWorld.isDevBuild.Get())
            SetView(Object.Instantiate(Resources.Load<BuilderMainPanelView>(VIEW_PREFAB_PATH_DEV)));
        else
            SetView(Object.Instantiate(Resources.Load<BuilderMainPanelView>(VIEW_PREFAB_PATH)));
    }

    internal void SetView(IBuilderMainPanelView view)
    {
        this.view = view;
        view.OnClosePressed += OnClose;
        view.OnBackPressed += OnBack;
    }

    private void OnBack()
    {
        if(newProjectFlowController.IsActive())
            newProjectFlowController.Hide();
        else
            OnClose();
    }

    public void Dispose()
    {
        StopFetchInterval();

        sectionsController.OnRequestOpenUrl -= OpenUrl;
        sectionsController.OnRequestGoToCoords -= GoToCoords;
        sectionsController.OnRequestEditSceneAtCoords -= OnGoToEditScene;
        sectionsController.OnCreateProjectRequest -= newProjectFlowController.NewProject;
        
        scenesViewController.OnJumpInPressed -= GoToCoords;
        scenesViewController.OnRequestOpenUrl -= OpenUrl;
        scenesViewController.OnEditorPressed -= OnGoToEditScene;
        newProjectFlowController.OnNewProjectCrated -= CreateNewProject;

        view.OnCreateProjectPressed -= newProjectFlowController.NewProject;
        
        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange -= OnVisibilityChanged;
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange -= OnSceneUnpublished;
        view.OnClosePressed -= OnClose;
        view.OnBackPressed -= OnBack;

        unpublishPopupController?.Dispose();

        fetchLandPromise?.Dispose();
        fetchProjectsPromise?.Dispose();

        leftMenuSettingsViewHandler?.Dispose();
        sectionsHandler?.Dispose();
        sceneContextMenuHandler?.Dispose();
        leftMenuHandler?.Dispose();

        sectionsController?.Dispose();
        scenesViewController?.Dispose();

        newProjectFlowController?.Dispose();

        view.Dispose();
    }

    public void Initialize(IContext context)
    {
        this.context = context;
        Initialize(new SectionsController(view.GetSectionContainer()),
            new ScenesViewController(view.GetSceneCardViewPrefab(), view.GetTransform()),
            new LandsController(),
            new ProjectsController(view.GetProjectCardView(), view.GetTransform()),
            new NewProjectFlowController(),
            Environment.i.platform.serviceProviders.theGraph,
            Environment.i.platform.serviceProviders.catalyst);
    }

    internal void Initialize(ISectionsController sectionsController,
        IScenesViewController scenesViewController, ILandsController landsesController, IProjectsController projectsController, INewProjectFlowController newProjectFlowController, ITheGraph theGraph, ICatalyst catalyst)
    {
        if (isInitialized)
            return;

        isInitialized = true;

        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.landsesController = landsesController;
        this.projectsController = projectsController;

        this.newProjectFlowController = newProjectFlowController;

        this.theGraph = theGraph;
        this.catalyst = catalyst;

        this.unpublishPopupController = new UnpublishPopupController(view.GetUnpublishPopup());

        // set listeners for sections, setup searchbar for section, handle request for opening a new section
        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsesController, projectsController, view.GetSearchBar());
        // handle if main panel or settings panel should be shown in current section
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        // handle project scene info on the left menu panel
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.GetSettingsViewReferences(), scenesViewController);
        // handle scene's context menu options
        sceneContextMenuHandler = new SceneContextMenuHandler(view.GetSceneCardViewContextMenu(), sectionsController, scenesViewController, unpublishPopupController);

        SetView();

        sectionsController.OnRequestOpenUrl += OpenUrl;
        sectionsController.OnRequestGoToCoords += GoToCoords;
        sectionsController.OnRequestEditSceneAtCoords += OnGoToEditScene;
        sectionsController.OnCreateProjectRequest += newProjectFlowController.NewProject;
        
        scenesViewController.OnJumpInPressed += GoToCoords;
        scenesViewController.OnRequestOpenUrl += OpenUrl;
        scenesViewController.OnEditorPressed += OnGoToEditScene;
        newProjectFlowController.OnNewProjectCrated += CreateNewProject;

        view.OnCreateProjectPressed += this.newProjectFlowController.NewProject;

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += OnVisibilityChanged;
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange += OnSceneUnpublished;
    }

    private void CreateNewProject(ProjectData project)
    {
        Promise<APIResponse> projectPromise = context.builderAPIController.CreateNewProject(project);

        projectPromise.Then( apiResponse =>
        {
            //TODO: If it is ok, Start the editor
            if (!apiResponse.ok)
                BIWUtils.ShowGenericNotification(CREATING_PROJECT_ERROR+apiResponse.error);
        });
        
        projectPromise.Catch( errorString =>
        {
            BIWUtils.ShowGenericNotification(CREATING_PROJECT_ERROR+errorString);
        });
    }

    public void SetVisibility(bool visible)
    {
        DataStore.i.HUDs.builderProjectsPanelVisible.Set(visible);
    }

    private void OnVisibilityChanged(bool isVisible, bool prev)
    {
        if (isVisible == prev)
            return;

        view.SetVisible(isVisible);

        if (isVisible)
        {
            if (DataStore.i.builderInWorld.landsWithAccess.Get() != null)
                PanelOpenEvent(DataStore.i.builderInWorld.landsWithAccess.Get());
            else
                sendPlayerOpenPanelEvent = true;

            FetchPanelInfo();
            StartFetchInterval();
            if (DataStore.i.builderInWorld.isDevBuild.Get())
                sectionsController.OpenSection(SectionId.PROJECTS);
            else
                sectionsController.OpenSection(SectionId.SCENES);
        }
        else
        {
            StopFetchInterval();
        }
    }

    private void OnClose()
    {
        if (!view.IsVisible())
            return;

        SetVisibility(false);

        LandWithAccess[] lands = landsesController.GetLands();
        if (lands != null)
        {
            Vector2Int totalLands = GetAmountOfLandsOwnedAndOperator(lands);
            BIWAnalytics.PlayerClosesPanel(totalLands.x, totalLands.y);
        }
    }

    private void PanelOpenEvent(LandWithAccess[] lands)
    {
        Vector2Int totalLands = GetAmountOfLandsOwnedAndOperator(lands);
        BIWAnalytics.PlayerOpenPanel(totalLands.x, totalLands.y);
    }

    /// <summary>
    /// This counts the amount of lands that the user own and the amount of lands that the user operate
    /// </summary>
    /// <param name="lands"></param>
    /// <returns>Vector2: X = amount of owned lands, Y = amount of operator lands</returns>
    private Vector2Int GetAmountOfLandsOwnedAndOperator(LandWithAccess[] lands)
    {
        int ownedLandsCount = 0;
        int operatorLandsCount = 0;
        foreach (var land in lands)
        {
            if (land.role == LandRole.OWNER)
                ownedLandsCount++;
            else
                operatorLandsCount++;
        }

        return new Vector2Int(ownedLandsCount, operatorLandsCount);
    }

    private void SetView()
    {
        scenesViewController.AddListener((ISceneListener) view);
        projectsController.AddListener((IProjectsListener) view);
    }

    private void FetchPanelInfo(float landCacheTime = CACHE_TIME_LAND, float scenesCacheTime = CACHE_TIME_SCENES)
    {
        if (isFetchingLands || isFetchingProjects)
            return;

        isFetchingLands = true;
        isFetchingProjects = true;

        var address = UserProfile.GetOwnUserProfile().ethAddress;
        var network = KernelConfig.i.Get().network;

#if UNITY_EDITOR
        // NOTE: to be able to test in editor without getting a profile we hardcode an address here
        if (string.IsNullOrEmpty(address))
        {
            address = TESTING_ETH_ADDRESS;
            network = TESTING_TLD;
            DataStore.i.playerRealm.Set(new CurrentRealmModel()
            {
                domain = $"https://peer-lb.decentraland.{TESTING_TLD}",
                contentServerUrl = $"https://peer-lb.decentraland.{TESTING_TLD}/content",
            });
        }
#endif

        sectionsController.SetFetchingDataStart();

        fetchLandPromise = DeployedScenesFetcher.FetchLandsFromOwner(catalyst, theGraph, address, network, landCacheTime, scenesCacheTime);
        fetchLandPromise
            .Then(LandsFetched)
            .Catch(LandsFetchedError);

        if (!DataStore.i.builderInWorld.isDevBuild.Get())
            return;
        fetchProjectsPromise = BuilderPanelDataFetcher.FetchProjectData(context.builderAPIController);
        fetchProjectsPromise
            .Then(ProjectsFetched)
            .Catch(ProjectsFetchedError);
    }

    internal void ProjectsFetched(ProjectData[] data)
    {
        DataStore.i.builderInWorld.projectData.Set(data);
        isFetchingProjects = false;
        projectsController.SetProjects(data);
        UpdateProjectsDeploymentStatus();
    }

    internal void ProjectsFetchedError(string error)
    {
        isFetchingProjects = false;
        sectionsController.SetFetchingDataEnd<SectionProjectController>();
        projectsController.SetProjects(new ProjectData[]{ });
        BIWUtils.ShowGenericNotification(error);
    }

    private void UpdateProjectsDeploymentStatus()
    {
        if(isFetchingLands || isFetchingProjects)
            return;
        
        projectsController.UpdateDeploymentStatus();
    }

    internal void LandsFetchedError(string error)
    {
        isFetchingLands = false;
        sectionsController.SetFetchingDataEnd<SectionLandController>();
        sectionsController.SetFetchingDataEnd<SectionScenesController>();
        landsesController.SetLands(new LandWithAccess[] { });
        scenesViewController.SetScenes(new ISceneData[] { });
        Debug.LogError(error);
    }

    internal void LandsFetched(LandWithAccess[] lands)
    {
        DataStore.i.builderInWorld.landsWithAccess.Set(lands.ToArray(), true);
        sectionsController.SetFetchingDataEnd<SectionLandController>();
        isFetchingLands = false;
        UpdateProjectsDeploymentStatus();
        
        try
        {
            ISceneData[] places = lands.Where(land => land.scenes != null && land.scenes.Count > 0)
                                     .Select(land => land.scenes.Where(scene => !scene.isEmpty).Select(scene => (ISceneData)new SceneData(scene)))
                                     .Aggregate((i, j) => i.Concat(j))
                                     .ToArray();

            if (sendPlayerOpenPanelEvent)
                PanelOpenEvent(lands);
            landsesController.SetLands(lands);
            scenesViewController.SetScenes(places);
        }
        catch (Exception e)
        {
            landsesController.SetLands(lands);
            scenesViewController.SetScenes(new ISceneData[] { });
        }
    }

    internal void GoToCoords(Vector2Int coords)
    {
        WebInterface.GoTo(coords.x, coords.y);
        SetVisibility(false);
        OnJumpInOrEdit?.Invoke();
    }

    private void OpenUrl(string url) { WebInterface.OpenURL(url); }

    internal void OnGoToEditScene(Vector2Int coords)
    {
        bool isGoingToTeleport = BIWTeleportAndEdit.TeleportAndEdit(coords);
        if (isGoingToTeleport)
        {
            SetVisibility(false);
        }

        OnJumpInOrEdit?.Invoke();
    }

    private void StartFetchInterval()
    {
        if (fetchDataInterval != null)
        {
            StopFetchInterval();
        }

        fetchDataInterval = CoroutineStarter.Start(RefreshDataInterval());
    }

    private void StopFetchInterval()
    {
        CoroutineStarter.Stop(fetchDataInterval);
        fetchDataInterval = null;
    }

    IEnumerator RefreshDataInterval()
    {
        while (true)
        {
            yield return WaitForSecondsCache.Get(REFRESH_INTERVAL);
            FetchPanelInfo();
        }
    }

    private void OnSceneUnpublished(PublishSceneResultPayload current, PublishSceneResultPayload previous)
    {
        if (current.ok)
        {
            FetchPanelInfo(CACHE_TIME_LAND, 0);
        }
    }
}