using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using UnityEngine;
using Variables.RealmsInfo;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class BuilderMainPanelController : IHUD, IBuilderMainPanelController
{
    private const string DELETE_PROJECT_CONFIRM_TEXT = "Are you sure that you want to delete {0} project?";
    private const string CREATING_PROJECT_ERROR = "Error creating a new project: ";
    private const string OBTAIN_PROJECT_ERROR = "Error obtaining the project: ";

    private const string DUPLICATE_PROJECT_ERROR = "Error duplicating the project: ";
    private const string PUBLISH_PROJECT_ERROR = "Error publishing the project: ";

    private const string DELETE_PROJECT_ERROR = "Error deleting the project: ";
    private const string DELETE_PROJECT_SUCCESS = "<b>{0}</b> has been deleted";

    private const string TESTING_ETH_ADDRESS = "0xDc13378daFca7Fe2306368A16BCFac38c80BfCAD";
    private const string TESTING_TLD = "org";
    private const string VIEW_PREFAB_PATH = "BuilderProjectsPanelDev";

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

    BaseVariable<Transform> configureBuilderInFullscreenMenu => DataStore.i.exploreV2.configureBuilderInFullscreenMenu;

    public BuilderMainPanelController()
    {
        SetView(Object.Instantiate(Resources.Load<BuilderMainPanelView>(VIEW_PREFAB_PATH)));

        configureBuilderInFullscreenMenu.OnChange += ConfigureBuilderInFullscreenMenuChanged;
        ConfigureBuilderInFullscreenMenuChanged(configureBuilderInFullscreenMenu.Get(), null);
    }

    internal void SetView(IBuilderMainPanelView view)
    {
        this.view = view;
        view.OnClosePressed += OnClose;
        view.OnBackPressed += OnBack;
        view.OnGuestConnectWallet += ConnectGuestWallet;
    }

    private void ConnectGuestWallet() { WebInterface.OpenURL(BIWSettings.GUEST_WALLET_INFO); }

    private void OnBack()
    {
        if (newProjectFlowController.IsActive())
            newProjectFlowController.Hide();
    }

    public void Dispose()
    {
        StopFetchInterval();

        context.publisher.OnPublishFinish -= PublishFinish;
        
        sectionsController.OnRequestOpenUrl -= OpenUrl;
        sectionsController.OnRequestGoToCoords -= GoToCoords;
        sectionsController.OnRequestEditSceneAtCoords -= OnGoToEditScene;
        sectionsController.OnCreateProjectRequest -= newProjectFlowController.NewProject;
        sectionsController.OnSectionContentEmpty -= SectionContentEmpty;
        sectionsController.OnSectionContentNotEmpty -= SectionContentNotEmpty;
        
        scenesViewController.OnJumpInPressed -= GoToCoords;
        scenesViewController.OnRequestOpenUrl -= OpenUrl;
        scenesViewController.OnEditorPressed -= OnGoToEditScene;
        projectsController.OnEditorPressed -= GetManifestToEdit;
        projectsController.OnDeleteProject -= DeleteProject;
        projectsController.OnDuplicateProject -= DuplicateProject;
        projectsController.OnPublishProject -= PublishProject;

        newProjectFlowController.OnNewProjectCrated -= CreateNewProject;

        view.OnCreateProjectPressed -= newProjectFlowController.NewProject;

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange -= OnVisibilityChanged;
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange -= OnSceneUnpublished;
        view.OnClosePressed -= OnClose;
        view.OnBackPressed -= OnBack;
        view.OnGuestConnectWallet -= ConnectGuestWallet;

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

        configureBuilderInFullscreenMenu.OnChange -= ConfigureBuilderInFullscreenMenuChanged;

        view.Dispose();
    }

    public void Initialize(IContext context)
    {
        Initialize(context,new SectionsController(view.GetSectionContainer()),
            new ScenesViewController(view.GetSceneCardViewPrefab(), view.GetTransform()),
            new LandsController(),
            new ProjectsController(view.GetProjectCardView(), view.GetProjectCardViewContextMenu(), view.GetTransform()),
            new NewProjectFlowController(),
            Environment.i.platform.serviceProviders.theGraph,
            Environment.i.platform.serviceProviders.catalyst);
    }

    internal void Initialize(IContext context,ISectionsController sectionsController,
        IScenesViewController scenesViewController, ILandsController landsesController, IProjectsController projectsController, INewProjectFlowController newProjectFlowController, ITheGraph theGraph, ICatalyst catalyst)
    {
        if (isInitialized)
            return;

        isInitialized = true;
        
        this.context = context;
        this.context.publisher.OnPublishFinish += PublishFinish;

        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.landsesController = landsesController;
        this.projectsController = projectsController;

        this.newProjectFlowController = newProjectFlowController;

        this.theGraph = theGraph;
        this.catalyst = catalyst;

        this.unpublishPopupController = new UnpublishPopupController(context,view.GetUnpublishPopup());

        // set listeners for sections, setup searchbar for section, handle request for opening a new section
        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsesController, projectsController, view.GetSearchBar());
        // handle if main panel or settings panel should be shown in current section
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        // handle project scene info on the left menu panel
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.GetSettingsViewReferences(), scenesViewController);
        // handle scene's context menu options
        sceneContextMenuHandler = new SceneContextMenuHandler(view.GetSceneCardViewContextMenu(), sectionsController, scenesViewController, unpublishPopupController);

        this.projectsController.SetSceneContextMenuHandler(sceneContextMenuHandler);

        SetView();

        sectionsController.OnRequestOpenUrl += OpenUrl;
        sectionsController.OnRequestGoToCoords += GoToCoords;
        sectionsController.OnRequestEditSceneAtCoords += OnGoToEditScene;
        sectionsController.OnCreateProjectRequest += newProjectFlowController.NewProject;
        sectionsController.OnSectionContentEmpty += SectionContentEmpty;
        sectionsController.OnSectionContentNotEmpty += SectionContentNotEmpty;

        scenesViewController.OnJumpInPressed += GoToCoords;
        scenesViewController.OnRequestOpenUrl += OpenUrl;
        scenesViewController.OnEditorPressed += OnGoToEditScene;
        newProjectFlowController.OnNewProjectCrated += CreateNewProject;

        view.OnCreateProjectPressed += this.newProjectFlowController.NewProject;
        this.projectsController.OnEditorPressed += GetManifestToEdit;
        this.projectsController.OnDeleteProject += DeleteProject;
        this.projectsController.OnDuplicateProject += DuplicateProject;
        this.projectsController.OnPublishProject += PublishProject;

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += OnVisibilityChanged;
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange += OnSceneUnpublished;
    }
    
    private void SectionContentNotEmpty() { view.SetSearchViewVisible(true); }

    private void SectionContentEmpty() { view.SetSearchViewVisible(false); }

    private void DuplicateProject(ProjectData data)
    {
        Promise<Manifest> manifestPromise = context.builderAPIController.GetManifestById(data.id);
        manifestPromise.Then( (manifest) =>
        {
            DuplicateProject(data, manifest);
        });

        manifestPromise.Catch( errorString =>
        {
            BIWUtils.ShowGenericNotification(DUPLICATE_PROJECT_ERROR + errorString);
        });

    }
    
    private async void DuplicateProject(ProjectData data, Manifest manifest)
    {
        string url = BIWUrlUtils.GetBuilderProjecThumbnailUrl(data.id, data.thumbnail);
        Promise<Texture2D> screenshotPromise = new Promise<Texture2D>();
        BIWUtils.MakeGetTextureCall(url, screenshotPromise);

        string scene_id = Guid.NewGuid().ToString();
        manifest.project.title += " Copy";
        manifest.project.id = Guid.NewGuid().ToString();
        manifest.project.scene_id = scene_id;
        manifest.scene.id = scene_id;
        manifest.project.created_at = DateTime.UtcNow;
        manifest.project.updated_at = DateTime.UtcNow;

        screenshotPromise.Then(texture =>
        {
            context.builderAPIController.SetThumbnail(manifest.project.id, texture);
        });

        Promise<bool> createPromise = context.builderAPIController.SetManifest(manifest);
        createPromise.Then(isOk =>
        {
            if (!isOk)
                BIWUtils.ShowGenericNotification(DUPLICATE_PROJECT_ERROR);
        });
        createPromise.Catch(error =>
        {
            BIWUtils.ShowGenericNotification(DUPLICATE_PROJECT_ERROR + error);
        });

        await createPromise;
        await screenshotPromise;

        // We need to wait a bit before refreshing the projects so the server is able to process the data 
        CoroutineStarter.Start(WaitASecondAndRefreshProjects());
        
        BIWAnalytics.ProjectDuplicated(data.id, new Vector2Int(data.rows,data.cols));
    }

    private async void PublishProject(ProjectData data)
    {
        Promise<Manifest> manifestPromise = context.builderAPIController.GetManifestById(data.id);
        manifestPromise.Then( (manifest) =>
        {
            manifest.project = data;
            PublishProject(manifest);
        });

        manifestPromise.Catch( errorString =>
        {
            BIWUtils.ShowGenericNotification(PUBLISH_PROJECT_ERROR + errorString);
        });
    }

    private async void PublishProject(Manifest manifest)
    {
        string url = BIWUrlUtils.GetBuilderProjecThumbnailUrl(manifest.project.id, manifest.project.thumbnail);
        Promise<Texture2D> screenshotPromise = new Promise<Texture2D>();
        BIWUtils.MakeGetTextureCall(url, screenshotPromise);

        IBuilderScene builderScene = new BuilderScene(manifest, IBuilderScene.SceneType.PROJECT);
        builderScene.SetScene(ManifestTranslator.ManifestToParcelSceneWithOnlyData(manifest));
        screenshotPromise.Then((texture2D => builderScene.sceneScreenshotTexture = texture2D));
        await screenshotPromise;
        context.publisher.StartPublish(builderScene);
    }

    private void DeleteProject(ProjectData data)
    {
        string deleteText = DELETE_PROJECT_CONFIRM_TEXT.Replace("{0}", data.title);

        context.commonHUD.GetPopUp()
               .ShowPopUpWithoutTitle(deleteText, "YES", "NO", () =>
               {
                   Promise<bool> manifestPromise = context.builderAPIController.DeleteProject(data.id);
                   manifestPromise.Then( (isOk) =>
                   {
                       if (isOk)
                       {
                           string text = DELETE_PROJECT_SUCCESS.Replace("{0}", data.title);
                           view.ShowToast(text);
                           FetchProjectData();
                       }
                   });

                   manifestPromise.Catch( errorString =>
                   {
                       BIWUtils.ShowGenericNotification(DELETE_PROJECT_ERROR + errorString);
                   });
                   
                   BIWAnalytics.ProjectDeleted(data.id, new Vector2Int(data.rows,data.cols));
               }, null);
    }

    private void GetManifestToEdit(ProjectData data)
    {
        Promise<Manifest> manifestPromise = context.builderAPIController.GetManifestById(data.id);
        manifestPromise.Then( OpenEditorFromManifest);

        manifestPromise.Catch( errorString =>
        {
            BIWUtils.ShowGenericNotification(OBTAIN_PROJECT_ERROR + errorString);
        });
    }

    private void CreateNewProject(ProjectData project)
    {
        context.sceneManager.ShowBuilderLoading();
        Promise<ProjectData> projectPromise = context.builderAPIController.CreateNewProject(project);

        projectPromise.Then( OpenEditorFromNewProject);

        projectPromise.Catch( errorString =>
        {
            context.sceneManager.HideBuilderLoading();
            BIWUtils.ShowGenericNotification(CREATING_PROJECT_ERROR + errorString);
        });
        
        BIWAnalytics.CreatedNewProject(project.title, project.description, new Vector2Int(project.rows,project.cols));
    }

    private void OpenEditorFromNewProject(ProjectData projectData)
    {
        var manifest = BIWUtils.CreateManifestFromProject(projectData);
        DataStore.i.builderInWorld.lastProjectIdCreated.Set(manifest.project.id);
        OpenEditorFromManifest(manifest);
    }

    private void OpenEditorFromManifest(Manifest manifest) { context.sceneManager.StartFlowFromProject(manifest); }

    public void SetVisibility(bool visible)
    {
        // Note: we set it here since the profile is not ready at the initialization part
        view.SetGuestMode(UserProfile.GetOwnUserProfile().isGuest);
        DataStore.i.HUDs.builderProjectsPanelVisible.Set(visible);
    }

    private void OnVisibilityChanged(bool isVisible, bool prev)
    {
        if (isVisible == prev)
            return;

        view.SetVisible(isVisible);

        // Note: we set it here since the profile is not ready at the initialization part
        view.SetGuestMode(UserProfile.GetOwnUserProfile().isGuest);

        if (isVisible)
        {
            if (DataStore.i.builderInWorld.landsWithAccess.Get() != null)
                PanelOpenEvent(DataStore.i.builderInWorld.landsWithAccess.Get());
            else
                sendPlayerOpenPanelEvent = true;

            sectionsController.OpenSection(SectionId.PROJECTS);
            
            FetchPanelInfo();
            StartFetchInterval();
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

    private void PublishFinish(bool isOk)
    {
        if(isOk)
            FetchPanelInfo(0,0);
    }

    private void FetchPanelInfo() => FetchPanelInfo(CACHE_TIME_LAND, CACHE_TIME_SCENES);
    
    private void FetchPanelInfo(float landCacheTime, float scenesCacheTime)
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
            DataStore.i.realm.playerRealm.Set(new CurrentRealmModel()
            {
                domain = $"https://peer.decentraland.{TESTING_TLD}",
                contentServerUrl = $"https://peer.decentraland.{TESTING_TLD}/content",
            });
        }
#endif

        sectionsController.SetFetchingDataStart();

        fetchLandPromise = DeployedScenesFetcher.FetchLandsFromOwner(catalyst, theGraph, address, network, landCacheTime, scenesCacheTime);
        fetchLandPromise
            .Then(LandsFetched)
            .Catch(LandsFetchedError);
        
        FetchProjectData();
    }

    internal void FetchProjectData()
    {
        sectionsController.SetFetchingDataStart<SectionProjectController>();
        fetchProjectsPromise = BuilderPanelDataFetcher.FetchProjectData(context.builderAPIController);
        fetchProjectsPromise
            .Then(ProjectsFetched)
            .Catch(ProjectsFetchedError);
    }

    internal void ProjectsFetched(ProjectData[] data)
    {
        DataStore.i.builderInWorld.projectData.Set(data);
        isFetchingProjects = false;
        sectionsController.SetFetchingDataEnd<SectionProjectController>();
        projectsController.SetProjects(data);
        UpdateProjectsDeploymentStatus();
    }

    internal void ProjectsFetchedError(string error)
    {
        isFetchingProjects = false;
        sectionsController.SetFetchingDataEnd<SectionProjectController>();
        projectsController.SetProjects(new ProjectData[] { });
        BIWUtils.ShowGenericNotification(error);
    }

    private void UpdateProjectsDeploymentStatus()
    {
        if (isFetchingLands || isFetchingProjects)
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
        sectionsController.SetFetchingDataEnd<SectionScenesController>();
        isFetchingLands = false;
        UpdateProjectsDeploymentStatus();

        try
        {
            ISceneData[] places = { };
            
            if(lands.Length > 0)
                places = lands.Where(land => land.scenes != null && land.scenes.Count > 0)
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
            Debug.Log("Error setting the lands and scenes "+ e);
            landsesController.SetLands(lands);
            scenesViewController.SetScenes(new ISceneData[] { });
        }
    }

    internal void GoToCoords(Vector2Int coords)
    {
        Environment.i.world.teleportController.Teleport(coords.x, coords.y);
        SetVisibility(false);
        OnJumpInOrEdit?.Invoke();
    }

    private void OpenUrl(string url) { WebInterface.OpenURL(url); }

    internal void OnGoToEditScene(Vector2Int coords)
    {
        SetVisibility(false);
        context.sceneManager.StartFlowFromLandCoords(coords);
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

    IEnumerator WaitASecondAndRefreshProjects()
    {
        yield return new WaitForSeconds(1f);
        FetchProjectData();
    }

    private void OnSceneUnpublished(PublishSceneResultPayload current, PublishSceneResultPayload previous)
    {
        if (current.ok)
        {
            FetchPanelInfo(CACHE_TIME_LAND, 0);
        }
    }

    private void ConfigureBuilderInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) { view.SetAsFullScreenMenuMode(currentParentTransform); }
}