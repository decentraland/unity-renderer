using System;
using System.Collections;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using Variables.RealmsInfo;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IHUD
{
    private const string TESTING_ETH_ADDRESS = "0x2fa1859029A483DEFbB664bB6026D682f55e2fcD";
    private const string TESTING_TLD = "org";
    private const string VIEW_PREFAB_PATH = "BuilderProjectsPanel";

    private const float CACHE_TIME_LAND = 5 * 60;
    private const float CACHE_TIME_SCENES = 1 * 60;
    private const float REFRESH_INTERVAL = 2 * 60;

    internal readonly IBuilderProjectsPanelView view;

    private ISectionsController sectionsController;
    private IScenesViewController scenesViewController;
    private ILandController landsController;

    private SectionsHandler sectionsHandler;
    private SceneContextMenuHandler sceneContextMenuHandler;
    private LeftMenuHandler leftMenuHandler;
    private LeftMenuSettingsViewHandler leftMenuSettingsViewHandler;

    private ITheGraph theGraph;
    private ICatalyst catalyst;

    private bool isInitialized = false;
    private bool isFetching = false;
    private Coroutine fetchDataInterval;
    private Promise<LandWithAccess[]> fetchLandPromise = null;

    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>(VIEW_PREFAB_PATH))) { }

    internal BuilderProjectsPanelController(IBuilderProjectsPanelView view)
    {
        this.view = view;
        view.OnClosePressed += OnClose;
    }

    public void Dispose()
    {
        StopFetchInterval();

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange -= OnVisibilityChanged;
        view.OnClosePressed -= OnClose;

        fetchLandPromise?.Dispose();

        leftMenuSettingsViewHandler?.Dispose();
        sectionsHandler?.Dispose();
        sceneContextMenuHandler?.Dispose();
        leftMenuHandler?.Dispose();

        sectionsController?.Dispose();
        scenesViewController?.Dispose();

        view.Dispose();
    }

    public void Initialize()
    {
        Initialize(new SectionsController(view.GetSectionContainer()),
            new ScenesViewController(view.GetCardViewPrefab(), view.GetTransform()),
            new LandController(),
            Environment.i.platform.serviceProviders.theGraph,
            Environment.i.platform.serviceProviders.catalyst);
    }

    internal void Initialize(ISectionsController sectionsController,
        IScenesViewController scenesViewController, ILandController landController, ITheGraph theGraph, ICatalyst catalyst)
    {
        if (isInitialized)
            return;

        isInitialized = true;

        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.landsController = landController;

        this.theGraph = theGraph;
        this.catalyst = catalyst;

        // set listeners for sections, setup searchbar for section, handle request for opening a new section
        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsController, view.GetSearchBar());
        // handle if main panel or settings panel should be shown in current section
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        // handle project scene info on the left menu panel
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.GetSettingsViewReferences(), scenesViewController);
        // handle scene's context menu options
        sceneContextMenuHandler = new SceneContextMenuHandler(view.GetSceneCardViewContextMenu(), sectionsController, scenesViewController);

        SetView();

        sectionsController.OnRequestOpenUrl += OpenUrl;
        sectionsController.OnRequestGoToCoords += GoToCoords;
        sectionsController.OnRequestEditSceneAtCoords += OnGoToEditScene;
        scenesViewController.OnJumpInPressed += GoToCoords;
        scenesViewController.OnRequestOpenUrl += OpenUrl;
        scenesViewController.OnEditorPressed += OnGoToEditScene;

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += OnVisibilityChanged;
    }

    public void SetVisibility(bool visible) { DataStore.i.HUDs.builderProjectsPanelVisible.Set(visible); }

    private void OnVisibilityChanged(bool isVisible, bool prev)
    {
        if (isVisible == prev)
            return;

        view.SetVisible(isVisible);

        if (isVisible)
        {
            FetchLandsAndScenes();
            StartFetchInterval();
            sectionsController.OpenSection(SectionId.SCENES_DEPLOYED);
        }
        else
        {
            StopFetchInterval();
        }
    }

    private void OnClose() { SetVisibility(false); }

    private void SetView()
    {
        scenesViewController.AddListener((IDeployedSceneListener) view);
        scenesViewController.AddListener((IProjectSceneListener) view);
    }

    private void FetchLandsAndScenes()
    {
        if (isFetching)
            return;

        isFetching = true;

        var address = UserProfile.GetOwnUserProfile().ethAddress;
        var tld = KernelConfig.i.Get().tld;

#if UNITY_EDITOR
        // NOTE: to be able to test in editor without getting a profile we hardcode an address here
        if (string.IsNullOrEmpty(address))
        {
            address = TESTING_ETH_ADDRESS;
            tld = TESTING_TLD;
            DataStore.i.playerRealm.Set(new CurrentRealmModel()
            {
                domain = $"https://peer.decentraland.{TESTING_TLD}",
                contentServerUrl = $"https://peer.decentraland.{TESTING_TLD}/content",
            });
        }
#endif

        sectionsController.SetFetchingDataStart();

        fetchLandPromise = DeployedScenesFetcher.FetchLandsFromOwner(catalyst, theGraph, address, tld, CACHE_TIME_LAND, CACHE_TIME_SCENES);
        fetchLandPromise
            .Then(lands =>
            {
                sectionsController.SetFetchingDataEnd();
                isFetching = false;

                try
                {
                    var scenes = lands.Where(land => land.scenes != null && land.scenes.Count > 0)
                                      .Select(land => land.scenes.Select(scene => (ISceneData)new SceneData(scene)))
                                      .Aggregate((i, j) => i.Concat(j))
                                      .ToArray();

                    landsController.SetLands(lands);
                    scenesViewController.SetScenes(scenes);
                }
                catch (Exception e)
                {
                    landsController.SetLands(lands);
                    scenesViewController.SetScenes(new ISceneData[] { });
                }
            })
            .Catch(error =>
            {
                isFetching = false;
                sectionsController.SetFetchingDataEnd();
                landsController.SetLands(new LandWithAccess[] { });
                scenesViewController.SetScenes(new ISceneData[] { });
                Debug.LogError(error);
            });
    }

    private void GoToCoords(Vector2Int coords)
    {
        WebInterface.GoTo(coords.x, coords.y);
        SetVisibility(false);
    }

    private void OpenUrl(string url) { WebInterface.OpenURL(url); }

    private void OnGoToEditScene(Vector2Int coords)
    {
        bool isGoingToTeleport = BuilderInWorldTeleportAndEdit.TeleportAndEdit(coords);
        if (isGoingToTeleport)
        {
            SetVisibility(false);
        }
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
            FetchLandsAndScenes();
        }
    }
}