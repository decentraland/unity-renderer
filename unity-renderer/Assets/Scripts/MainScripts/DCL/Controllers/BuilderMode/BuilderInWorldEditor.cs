using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Tutorial;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using UnityEngine;
using Environment = DCL.Environment;

public class BuilderInWorldEditor : IBIWEditor
{
    internal static bool BYPASS_LAND_OWNERSHIP_CHECK = false;
    private const float DISTANCE_TO_DISABLE_BUILDER_IN_WORLD = 45f;
    private const float MAX_DISTANCE_STOP_TRYING_TO_ENTER = 16;

    private GameObject cursorGO;
    private InputController inputController;
    private GameObject[] groundVisualsGO;

    internal IBIWOutlinerController outlinerController => context.editorContext.outlinerController;
    internal IBIWInputHandler inputHandler => context.editorContext.inputHandler;
    internal IBIWPublishController publishController => context.editorContext.publishController;
    internal IBIWCreatorController creatorController => context.editorContext.creatorController;
    internal IBIWModeController modeController => context.editorContext.modeController;
    internal IBIWFloorHandler floorHandler => context.editorContext.floorHandler;
    internal IBIWEntityHandler entityHandler => context.editorContext.entityHandler;
    internal IBIWActionController actionController => context.editorContext.actionController;
    internal IBIWSaveController saveController => context.editorContext.saveController;
    internal IBIWInputWrapper inputWrapper => context.editorContext.inputWrapper;
    internal IBIWRaycastController raycastController => context.editorContext.raycastController;
    internal IBIWGizmosController gizmosController => context.editorContext.gizmosController;

    internal IBuilderInWorldLoadingController initialLoadingController;

    private BuilderInWorldBridge builderInWorldBridge;
    private BuilderInWorldAudioHandler biwAudioHandler;
    internal IContext context;

    private readonly List<IBIWController> controllers = new List<IBIWController>();

    internal ParcelScene sceneToEdit;
    private BiwSceneMetricsAnalyticsHelper sceneMetricsAnalyticsHelper;

    private Material skyBoxMaterial;

    public bool isBuilderInWorldActivated { get; internal set; } = false;

    private InputAction_Trigger editModeChangeInputAction;

    internal int checkerInsideSceneOptimizationCounter = 0;
    internal string sceneToEditId;
    internal bool catalogAdded = false;
    private bool sceneReady = false;
    private bool isInit = false;
    private Material previousSkyBoxMaterial;
    private Vector3 parcelUnityMiddlePoint;
    private bool previousAllUIHidden;
    private bool isCatalogLoading = false;
    private float beginStartFlowTimeStamp = 0;
    private float startEditorTimeStamp = 0;
    internal bool isCatalogRequested = false;
    internal bool isEnteringEditMode = false;
    private bool activeFeature = false;
    
    private IWebRequestAsyncOperation catalogAsyncOp;

    private UserProfile userProfile;
    internal Coroutine updateLandsWithAcessCoroutine;

    internal bool isWaitingForPermission = false;
    private bool alreadyAskedForLandPermissions = false;
    private Vector3 askPermissionLastPosition;

    public void Initialize(IContext context)
    {
        if (isInit)
            return;

        activeFeature = true;
        isInit = true;

        this.context = context;
        
        InitReferences(SceneReferences.i);

        if (builderInWorldBridge != null)
            builderInWorldBridge.OnBuilderProjectInfo += BuilderProjectPanelInfo;

        BIWNFTController.i.OnNFTUsageChange += OnNFTUsageChange;
        
        InitHUD();

        BIWTeleportAndEdit.OnTeleportEnd += OnPlayerTeleportedToEditScene;

        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        userProfile = UserProfile.GetOwnUserProfile();

        context.builderAPIController.OnWebRequestCreated += WebRequestCreated;
        
        editModeChangeInputAction = context.inputsReferencesAsset.editModeChangeInputAction;
        editModeChangeInputAction.OnTriggered += ChangeEditModeStatusByShortcut;

        biwAudioHandler = UnityEngine.Object.Instantiate(context.projectReferencesAsset.audioPrefab, Vector3.zero, Quaternion.identity).GetComponent<BuilderInWorldAudioHandler>();
        biwAudioHandler.Initialize(context);
        biwAudioHandler.gameObject.SetActive(false);
    }

    public void InitReferences(SceneReferences sceneReferences)
    {
        builderInWorldBridge = sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
        cursorGO = sceneReferences.cursorCanvas;
        inputController = sceneReferences.inputController;

        List<GameObject> grounds = new List<GameObject>();

        for (int i = 0; i < sceneReferences.groundVisual.transform.transform.childCount; i++)
        {
            grounds.Add(sceneReferences.groundVisual.transform.transform.GetChild(i).gameObject);
        }

        groundVisualsGO = grounds.ToArray();
        skyBoxMaterial = context.projectReferencesAsset.skyBoxMaterial;
    }

    private void InitHUD()
    {
        context.editorContext.editorHUD.Initialize();

        context.editorContext.editorHUD.OnTutorialAction += StartTutorial;
        context.editorContext.editorHUD.OnStartExitAction += StartExitMode;
        context.editorContext.editorHUD.OnLogoutAction += ExitEditMode;

        if (context.panelHUD != null)
            context.panelHUD.OnJumpInOrEdit += GetCatalog;

        ConfigureLoadingController();
    }

    public void Dispose()
    {
        sceneMetricsAnalyticsHelper?.Dispose();

        CoroutineStarter.Stop(updateLandsWithAcessCoroutine);

        if (sceneToEdit != null)
            sceneToEdit.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;

        Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;
        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnTutorialAction -= StartTutorial;
            context.editorContext.editorHUD.OnStartExitAction -= StartExitMode;
            context.editorContext.editorHUD.OnLogoutAction -= ExitEditMode;
        }

        BIWTeleportAndEdit.OnTeleportEnd -= OnPlayerTeleportedToEditScene;
        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        context.builderAPIController.OnWebRequestCreated -= WebRequestCreated;

        BIWNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;

        BIWNFTController.i.Dispose();
        builderInWorldBridge.OnBuilderProjectInfo -= BuilderProjectPanelInfo;

        floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;

        CleanItems();

        if (context.panelHUD != null)
            context.panelHUD.OnJumpInOrEdit -= GetCatalog;
        editModeChangeInputAction.OnTriggered -= ChangeEditModeStatusByShortcut;

        if (biwAudioHandler.gameObject != null)
        {
            biwAudioHandler.Dispose();
            UnityEngine.Object.Destroy(biwAudioHandler.gameObject);
        }

        initialLoadingController?.Dispose();
    }

    public void OnGUI()
    {
        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.OnGUI();
        }
    }
    
    public void WebRequestCreated(IWebRequestAsyncOperation webRequest)
    {
        if (isCatalogLoading)
            catalogAsyncOp = webRequest;
    }

    public void Update()
    {
        if (isCatalogLoading && catalogAsyncOp?.webRequest != null)
            UpdateCatalogLoadingProgress(catalogAsyncOp.webRequest.downloadProgress * 100);

        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.Update();
        }

        if (checkerInsideSceneOptimizationCounter >= 60)
        {
            if (Vector3.Distance(DCLCharacterController.i.characterPosition.unityPosition, parcelUnityMiddlePoint) >= DISTANCE_TO_DISABLE_BUILDER_IN_WORLD)
                ExitEditMode();
            checkerInsideSceneOptimizationCounter = 0;
        }
        else
        {
            checkerInsideSceneOptimizationCounter++;
        }
    }

    public void LateUpdate()
    {
        if (!isBuilderInWorldActivated)
            return;

        foreach (var controller in controllers)
        {
            controller.LateUpdate();
        }
    }

    private void ConfigureLoadingController()
    {
        initialLoadingController = new BuilderInWorldLoadingController();
        initialLoadingController.Initialize();
    }

    private void OnNFTUsageChange()
    {
        context.editorContext.editorHUD.RefreshCatalogAssetPack();
        context.editorContext.editorHUD.RefreshCatalogContent();
    }

    internal void ActivateLandAccessBackgroundChecker()
    {
        userProfile = UserProfile.GetOwnUserProfile();
        if (!string.IsNullOrEmpty(userProfile.userId))
        {
            if (updateLandsWithAcessCoroutine != null)
                CoroutineStarter.Stop(updateLandsWithAcessCoroutine);
            updateLandsWithAcessCoroutine = CoroutineStarter.Start(CheckLandsAccess());
        }
    }

    private void BuilderProjectPanelInfo(string title, string description) {  context.editorContext.editorHUD.SetBuilderProjectInfo(title, description); }
    
    public void CatalogLoaded()
    {
        isCatalogLoading = false;
        catalogAdded = true;
        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.RefreshCatalogContent();
        StartEditMode();
    }

    internal void GetCatalog()
    {
        if (catalogAdded)
            return;

        isCatalogLoading = true;
        BIWNFTController.i.StartFetchingNft();
        var catalogPromise = context.builderAPIController.GetCompleteCatalog(userProfile.ethAddress);
        catalogPromise.Then(x =>
        {
            CatalogLoaded();
        });
        catalogPromise.Catch(error =>
        {
            BIWUtils.ShowGenericNotification(error);
        });

        isCatalogRequested = true;
    }

    private void InitControllers()
    {
        InitController(entityHandler);
        InitController(modeController);
        InitController(publishController);
        InitController(creatorController);
        InitController(outlinerController);
        InitController(floorHandler);
        InitController(inputHandler);
        InitController(saveController);
        InitController(actionController);
        InitController(inputWrapper);
        InitController(raycastController);
        InitController(gizmosController);
    }

    public void InitController(IBIWController controller)
    {
        controller.Initialize(context);
        controllers.Add(controller);
    }

    private void StartTutorial() { TutorialController.i.SetBuilderInWorldTutorialEnabled(); }

    public void CleanItems()
    {
        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.Dispose();

        Camera camera = Camera.main;

        if (camera != null)
        {
            BIWOutline outliner = camera.GetComponent<BIWOutline>();
            UnityEngine.Object.Destroy(outliner);
        }

        floorHandler?.CleanUp();
        creatorController?.CleanUp();
    }

    public void ChangeEditModeStatusByShortcut(DCLAction_Trigger action)
    {
        if (!activeFeature)
            return;

        if (isEnteringEditMode)
            return;

        if (isBuilderInWorldActivated)
        {
            context.editorContext.editorHUD.ExitStart();
            return;
        }

        if (DataStore.i.builderInWorld.landsWithAccess.Get().Length == 0 && !alreadyAskedForLandPermissions)
        {
            ActivateLandAccessBackgroundChecker();
            BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_WAITING_FOR_PERMISSIONS_MESSAGE, DCL.NotificationModel.Type.GENERIC_WITHOUT_BUTTON, BIWSettings.LAND_CHECK_MESSAGE_TIMER);
            isWaitingForPermission = true;
            askPermissionLastPosition = DCLCharacterController.i.characterPosition.unityPosition;
        }
        else
        {
            CheckSceneToEditByShorcut();
        }
    }

    internal void CheckSceneToEditByShorcut()
    {
        FindSceneToEdit();

        if (!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
            return;
        }

        if (IsParcelSceneDeployedFromSDK(sceneToEdit))
        {
            BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
            return;
        }

        GetCatalog();
        TryStartEnterEditMode(true, null, "Shortcut");
    }

    internal void NewSceneAdded(IParcelScene newScene)
    {
        if (newScene.sceneData.id != sceneToEditId)
            return;

        Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;

        sceneToEdit = (ParcelScene)Environment.i.world.state.GetScene(sceneToEditId);
        sceneMetricsAnalyticsHelper = new BiwSceneMetricsAnalyticsHelper(sceneToEdit);
        sceneToEdit.OnLoadingStateUpdated += UpdateSceneLoadingProgress;
    }

    private void NewSceneReady(string id)
    {
        if (sceneToEditId != id)
            return;

        sceneToEdit.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;
        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;
        sceneToEditId = null;
        sceneReady = true;
        CheckEnterEditMode();
    }

    internal bool UserHasPermissionOnParcelScene(ParcelScene sceneToCheck)
    {
        if (BYPASS_LAND_OWNERSHIP_CHECK)
            return true;

        List<Vector2Int> allParcelsWithAccess = DataStore.i.builderInWorld.landsWithAccess.Get().SelectMany(land => land.parcels).ToList();
        foreach (Vector2Int parcel in allParcelsWithAccess)
        {
            if (sceneToCheck.sceneData.parcels.Any(currentParcel => currentParcel.x == parcel.x && currentParcel.y == parcel.y))
                return true;
        }

        return false;
    }

    internal bool IsParcelSceneDeployedFromSDK(ParcelScene sceneToCheck)
    {
        List<Scene> allDeployedScenesWithAccess = DataStore.i.builderInWorld.landsWithAccess.Get().SelectMany(land => land.scenes).ToList();
        foreach (Scene scene in allDeployedScenesWithAccess)
        {
            if (scene.source != Scene.Source.SDK)
                continue;

            List<Vector2Int> parcelsDeployedFromSDK = scene.parcels.ToList();
            foreach (Vector2Int parcel in parcelsDeployedFromSDK)
            {
                if (sceneToCheck.sceneData.parcels.Any(currentParcel => currentParcel.x == parcel.x && currentParcel.y == parcel.y))
                    return true;
            }
        }

        return false;
    }

    private void CheckEnterEditMode()
    {
        if (catalogAdded && sceneReady)
            EnterEditMode();
    }

    public void TryStartEnterEditMode() { TryStartEnterEditMode(true, null); }

    public void TryStartEnterEditMode(IParcelScene targetScene) { TryStartEnterEditMode(true, targetScene); }

    public void TryStartEnterEditMode(bool activateCamera, IParcelScene targetScene = null , string source = "BuilderPanel")
    {
        if (sceneToEditId != null)
            return;

        FindSceneToEdit(targetScene);

        if (!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
            return;
        }
        else if (IsParcelSceneDeployedFromSDK(sceneToEdit))
        {
            BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
            return;
        }

        //If the scene is still not loaded, we return as we still can't enter in builder in world 
        if (sceneToEditId != null)
            return;

        isEnteringEditMode = true;
        previousAllUIHidden = CommonScriptableObjects.allUIHidden.Get();
        NotificationsController.i.allowNotifications = false;
        CommonScriptableObjects.allUIHidden.Set(true);
        NotificationsController.i.allowNotifications = true;
        inputController.inputTypeMode = InputTypeMode.BUILD_MODE_LOADING;
        initialLoadingController.Show();
        initialLoadingController.SetPercentage(0f);
        DataStore.i.appMode.Set(AppMode.BUILDER_IN_WORLD_EDITION);
        DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
        BIWAnalytics.StartEditorFlow(source);
        beginStartFlowTimeStamp = Time.realtimeSinceStartup;

        if (biwAudioHandler != null && biwAudioHandler.gameObject != null)
            biwAudioHandler.gameObject.SetActive(true);
        
        //Note (Adrian) this should handle different when we have the full flow of the feature
        if (activateCamera)
            modeController.ActivateCamera(sceneToEdit);

        if (catalogAdded)
            StartEditMode();
    }

    private void StartEditMode()
    {
        if (sceneToEdit == null)
            return;

        isEnteringEditMode = true;

        Environment.i.platform.cullingController.Stop();

        sceneToEditId = sceneToEdit.sceneData.id;

        // In this point we're sure that the catalog loading (the first half of our progress bar) has already finished
        initialLoadingController.SetPercentage(50f);
        Environment.i.world.sceneController.OnNewSceneAdded += NewSceneAdded;
        Environment.i.world.sceneController.OnReadyScene += NewSceneReady;
        Environment.i.world.blockersController.SetEnabled(false);

        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
    }

    private void EnterEditMode()
    {
        if (!initialLoadingController.isActive)
            return;

        isEnteringEditMode = false;
        BIWNFTController.i.StartEditMode();

        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        parcelUnityMiddlePoint = BIWUtils.CalculateUnityMiddlePoint(sceneToEdit);

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.SetParcelScene(sceneToEdit);
            context.editorContext.editorHUD.RefreshCatalogContent();
            context.editorContext.editorHUD.RefreshCatalogAssetPack();
            context.editorContext.editorHUD.SetVisibilityOfCatalog(true);
            context.editorContext.editorHUD.SetVisibilityOfInspector(true);
        }

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);
        DataStore.i.builderInWorld.showTaskBar.Set(true);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

        EnterBiwControllers();
        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

        initialLoadingController.SetPercentage(100f);

        if (IsNewScene())
        {
            floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
            floorHandler.OnAllParcelsFloorLoaded += OnAllParcelsFloorLoaded;
            SetupNewScene();
        }
        else
        {
            initialLoadingController.Hide(onHideAction: () =>
            {
                inputController.inputTypeMode = InputTypeMode.BUILD_MODE;
                context.editorContext.editorHUD?.SetVisibility(true);
                CommonScriptableObjects.allUIHidden.Set(previousAllUIHidden);
                OpenNewProjectDetailsIfNeeded();
            });
        }

        isBuilderInWorldActivated = true;

        previousSkyBoxMaterial = RenderSettings.skybox;
        RenderSettings.skybox = skyBoxMaterial;

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(false);
        }

        startEditorTimeStamp = Time.realtimeSinceStartup;

        BIWAnalytics.AddSceneInfo(sceneToEdit.sceneData.basePosition, BIWUtils.GetLandOwnershipType(DataStore.i.builderInWorld.landsWithAccess.Get().ToList(), sceneToEdit).ToString(), BIWUtils.GetSceneSize(sceneToEdit));
        BIWAnalytics.EnterEditor( Time.realtimeSinceStartup - beginStartFlowTimeStamp);
    }

    internal void OnAllParcelsFloorLoaded()
    {
        if (!initialLoadingController.isActive)
            return;

        floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(onHideAction: () =>
        {
            inputController.inputTypeMode = InputTypeMode.BUILD_MODE;
            context.editorContext.editorHUD.SetVisibility(true);
            CommonScriptableObjects.allUIHidden.Set(previousAllUIHidden);
            OpenNewProjectDetailsIfNeeded();
        });
    }

    private void OpenNewProjectDetailsIfNeeded()
    {
        if (builderInWorldBridge.builderProject.isNewEmptyProject)
            modeController.OpenNewProjectDetails();
    }

    public void StartExitMode()
    {
        if (saveController.GetSaveTimes() > 0)
        {
            modeController.TakeSceneScreenshotForExit();

            if (  context.editorContext.editorHUD != null)
                context.editorContext.editorHUD.ConfigureConfirmationModal(
                    BIWSettings.EXIT_MODAL_TITLE,
                    BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_SUBTITLE,
                    BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_CANCEL_BUTTON,
                    BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_CONFIRM_BUTTON);
        }
        else
        {
            context.editorContext.editorHUD.ConfigureConfirmationModal(
                BIWSettings.EXIT_MODAL_TITLE,
                BIWSettings.EXIT_MODAL_SUBTITLE,
                BIWSettings.EXIT_MODAL_CANCEL_BUTTON,
                BIWSettings.EXIT_MODAL_CONFIRM_BUTTON);
        }
    }

    public void ExitEditMode()
    {
        Environment.i.platform.cullingController.Start();
        BIWNFTController.i.ExitEditMode();
        
        floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(true);
        inputController.inputTypeMode = InputTypeMode.GENERAL;

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);
        DataStore.i.builderInWorld.showTaskBar.Set(true);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);

        if (sceneToEdit != null)
            sceneToEdit.SetEditMode(false);

        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;

        InmediateExit();

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.ClearEntityList();
            context.editorContext.editorHUD.SetVisibility(false);
        }

        Environment.i.world.sceneController.DeactivateBuilderInWorldEditScene();
        Environment.i.world.blockersController.SetEnabled(true);

        ExitBiwControllers();

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(true);
        }

        isBuilderInWorldActivated = false;
        RenderSettings.skybox = previousSkyBoxMaterial;

        if (biwAudioHandler.gameObject != null)
            biwAudioHandler.gameObject.SetActive(false);
        DataStore.i.appMode.Set(AppMode.DEFAULT);
        DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);
        BIWAnalytics.ExitEditor(Time.realtimeSinceStartup - startEditorTimeStamp);
    }

    public void InmediateExit()
    {
        CommonScriptableObjects.allUIHidden.Set(previousAllUIHidden);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);
    }

    public void EnterBiwControllers()
    {
        foreach (var controller in controllers)
        {
            controller.EnterEditMode(sceneToEdit);
        }

        //Note: This audio should inside the controllers, it is here because it is still a monobehaviour
        biwAudioHandler.EnterEditMode(sceneToEdit);
    }

    public void ExitBiwControllers()
    {
        foreach (var controller in controllers)
        {
            controller.ExitEditMode();
        }

        if (biwAudioHandler.gameObject != null)
            biwAudioHandler.ExitEditMode();
    }

    public bool IsNewScene() { return sceneToEdit.entities.Count <= 0; }

    public void SetupNewScene() { floorHandler.CreateDefaultFloor(); }

    internal void ExitAfterCharacterTeleport(DCLCharacterPosition position) { ExitEditMode(); }

    public void FindSceneToEdit(IParcelScene targetScene)
    {
        if (targetScene != null)
        {
            var parcelSceneTarget = (ParcelScene)targetScene;
            if (sceneToEdit != null && sceneToEdit != parcelSceneTarget)
                actionController.Clear();

            sceneToEdit = parcelSceneTarget;
        }
        else
        {
            FindSceneToEdit();
        }
    }

    public IParcelScene FindSceneToEdit()
    {
        foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (WorldStateUtils.IsCharacterInsideScene(scene))
            {
                ParcelScene parcelScene = (ParcelScene)scene;

                if (sceneToEdit != null && sceneToEdit != parcelScene)
                    actionController.Clear();

                sceneToEdit = parcelScene;
                return sceneToEdit;
            }
        }

        return null;
    }

    private void OnPlayerTeleportedToEditScene(Vector2Int coords)
    {
        if (activeFeature)
        {
            var targetScene = Environment.i.world.state.scenesSortedByDistance
                                         .FirstOrDefault(scene => scene.sceneData.parcels.Contains(coords));
            TryStartEnterEditMode(targetScene);
        }
    }

    private void UpdateCatalogLoadingProgress(float catalogLoadingProgress) { initialLoadingController.SetPercentage(catalogLoadingProgress / 2); }

    private void UpdateSceneLoadingProgress(float sceneLoadingProgress) { initialLoadingController.SetPercentage(50f + (sceneLoadingProgress / 2)); }

    private IEnumerator CheckLandsAccess()
    {
        while (true)
        {
            UpdateLandsWithAccess();
            yield return WaitForSecondsCache.Get(BIWSettings.REFRESH_LANDS_WITH_ACCESS_INTERVAL);
        }
    }

    private void UpdateLandsWithAccess()
    {
        if (isBuilderInWorldActivated)
            return;

        DeployedScenesFetcher.FetchLandsFromOwner(
                                 Environment.i.platform.serviceProviders.catalyst,
                                 Environment.i.platform.serviceProviders.theGraph,
                                 userProfile.ethAddress,
                                 KernelConfig.i.Get().network,
                                 BIWSettings.CACHE_TIME_LAND,
                                 BIWSettings.CACHE_TIME_SCENES)
                             .Then(lands =>
                             {
                                 DataStore.i.builderInWorld.landsWithAccess.Set(lands.ToArray(), true);
                                 if (isWaitingForPermission && Vector3.Distance(askPermissionLastPosition, DCLCharacterController.i.characterPosition.unityPosition) <= MAX_DISTANCE_STOP_TRYING_TO_ENTER)
                                 {
                                     CheckSceneToEditByShorcut();
                                 }

                                 isWaitingForPermission = false;
                                 alreadyAskedForLandPermissions = true;
                             });
    }
}