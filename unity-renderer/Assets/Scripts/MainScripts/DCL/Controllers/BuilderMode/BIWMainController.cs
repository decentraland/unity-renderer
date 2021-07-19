using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Tutorial;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Environment = DCL.Environment;

public class BIWMainController : Feature
{
    private const bool BYPASS_LAND_OWNERSHIP_CHECK = false;
    private const float DISTANCE_TO_DISABLE_BUILDER_IN_WORLD = 45f;

    private GameObject cursorGO;
    private InputController inputController;
    private GameObject[] groundVisualsGO;

    private BIWOutlinerController outlinerController;
    private BIWInputHandler inputHandler;
    private BIWPublishController publishController;
    private BIWCreatorController creatorController;
    private BIWModeController modeController;
    private BIWFloorHandler floorHandler;
    private BIWEntityHandler entityHandler;
    private BIWActionController actionController;
    private BIWSaveController saveController;
    private BIWInputWrapper inputWrapper;
    private BIWRaycastController raycastController;
    private BIWGizmosController gizmosController;

    private BuilderInWorldBridge builderInWorldBridge;
    private BuilderInWorldAudioHandler biwAudioHandler;
    private BIWContext context;

    private readonly List<BIWController> controllers = new List<BIWController>();

    private ParcelScene sceneToEdit;

    private Material skyBoxMaterial;

    public bool isBuilderInWorldActivated { get; private set; } = false;

    private InputAction_Trigger editModeChangeInputAction;

    private int checkerInsideSceneOptimizationCounter = 0;
    private string sceneToEditId;
    private bool catalogAdded = false;
    private bool sceneReady = false;
    private bool isInit = false;
    private Material previousSkyBoxMaterial;
    private Vector3 parcelUnityMiddlePoint;
    private bool previousAllUIHidden;
    private WebRequestAsyncOperation catalogAsyncOp;
    private bool isCatalogLoading = false;
    private bool areCatalogHeadersReady = false;
    private float beginStartFlowTimeStamp = 0;
    private float startEditorTimeStamp = 0;
    private bool isCatalogRequested = false;
    private bool isEnteringEditMode = false;
    private bool activeFeature = false;

    internal IBuilderInWorldLoadingController initialLoadingController;

    private UserProfile userProfile;
    private List<LandWithAccess> landsWithAccess = new List<LandWithAccess>();
    private Coroutine updateLandsWithAcessCoroutine;
    private Dictionary<string, string> catalogCallHeaders;

    public override void Initialize()
    {
        base.Initialize();

        if (isInit)
            return;

        activeFeature = true;
        isInit = true;

        BIWCatalogManager.Init();

        CreateControllers();
        InitReferences(InitialSceneReferences.i);


        if (builderInWorldBridge != null)
        {
            builderInWorldBridge.OnCatalogHeadersReceived += CatalogHeadersReceived;
            builderInWorldBridge.OnBuilderProjectInfo -= BuilderProjectPanelInfo;
        }

        userProfile = UserProfile.GetOwnUserProfile();
        if (!string.IsNullOrEmpty(userProfile.userId))
            updateLandsWithAcessCoroutine = CoroutineStarter.Start(CheckLandsAccess());
        else
            userProfile.OnUpdate += OnUserProfileUpdate;

        InitHUD();

        BIWTeleportAndEdit.OnTeleportEnd += OnPlayerTeleportedToEditScene;

        ConfigureLoadingController();
        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        builderInWorldBridge.AskKernelForCatalogHeaders();

        isCatalogLoading = true;
        BIWNFTController.i.Initialize();
        BIWNFTController.i.OnNFTUsageChange += OnNFTUsageChange;

        editModeChangeInputAction = context.inputsReferences.editModeChangeInputAction;
        editModeChangeInputAction.OnTriggered += ChangeEditModeStatusByShortcut;

        biwAudioHandler = GameObject.Instantiate(context.projectReferences.audioPrefab, Vector3.zero, Quaternion.identity).GetComponent<BuilderInWorldAudioHandler>();
        biwAudioHandler.SetReferences(context);
        biwAudioHandler.gameObject.SetActive(false);
    }

    private void InitReferences(InitialSceneReferences initalReference)
    {
        builderInWorldBridge = initalReference.builderInWorldBridge;
        cursorGO = initalReference.cursorCanvas;
        inputController = initalReference.inputController;

        List<GameObject> grounds = new List<GameObject>();
        for (int i = 0; i < InitialSceneReferences.i.groundVisual.transform.transform.childCount; i++)
        {
            grounds.Add(InitialSceneReferences.i.groundVisual.transform.transform.GetChild(i).gameObject);
        }
        groundVisualsGO = grounds.ToArray();

        context = new BIWContext();
        context.Init(
            outlinerController,
            inputHandler,
            inputWrapper,
            publishController,
            creatorController,
            modeController,
            floorHandler,
            entityHandler,
            actionController,
            saveController,
            raycastController,
            gizmosController
        );

        skyBoxMaterial = context.projectReferences.skyBoxMaterial;
    }

    private void CreateControllers()
    {
        outlinerController = new BIWOutlinerController();
        inputHandler = new BIWInputHandler();
        publishController = new BIWPublishController();
        creatorController = new BIWCreatorController();
        modeController = new BIWModeController();
        floorHandler = new BIWFloorHandler();
        entityHandler = new BIWEntityHandler();
        actionController = new BIWActionController();
        saveController = new BIWSaveController();
        inputWrapper = new BIWInputWrapper();
        raycastController = new BIWRaycastController();
        gizmosController = new BIWGizmosController();
    }

    private void InitBuilderProjectPanel()
    {
        if (HUDController.i.builderProjectsPanelController != null)
            HUDController.i.builderProjectsPanelController.OnJumpInOrEdit += GetCatalog;
    }

    private void InitHUD()
    {
        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement(hudConfig, HUDElementID.BUILDER_IN_WORLD_MAIN);
        HUDController.i.OnBuilderProjectPanelCreation += InitBuilderProjectPanel;

        HUDController.i.builderInWorldMainHud.Initialize();

        HUDController.i.builderInWorldMainHud.OnTutorialAction += StartTutorial;
        HUDController.i.builderInWorldMainHud.OnStartExitAction += StartExitMode;
        HUDController.i.builderInWorldMainHud.OnLogoutAction += ExitEditMode;

        if (HUDController.i.builderProjectsPanelController != null)
            HUDController.i.builderProjectsPanelController.OnJumpInOrEdit += GetCatalog;
    }

    public override void Dispose()
    {
        base.Dispose();

        if (userProfile != null)
            userProfile.OnUpdate -= OnUserProfileUpdate;

        CoroutineStarter.Stop(updateLandsWithAcessCoroutine);

        if (sceneToEdit != null)
            sceneToEdit.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;

        Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;
        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;


        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnTutorialAction -= StartTutorial;
            HUDController.i.builderInWorldMainHud.OnStartExitAction -= StartExitMode;
            HUDController.i.builderInWorldMainHud.OnLogoutAction -= ExitEditMode;
        }

        BIWTeleportAndEdit.OnTeleportEnd -= OnPlayerTeleportedToEditScene;

        if (initialLoadingController != null)
            initialLoadingController.Dispose();


        BIWNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;
        builderInWorldBridge.OnCatalogHeadersReceived -= CatalogHeadersReceived;
        builderInWorldBridge.OnBuilderProjectInfo -= BuilderProjectPanelInfo;
        CleanItems();

        HUDController.i.OnBuilderProjectPanelCreation -= InitBuilderProjectPanel;
        editModeChangeInputAction.OnTriggered -= ChangeEditModeStatusByShortcut;

        if (biwAudioHandler.gameObject != null)
            GameObject.Destroy(biwAudioHandler.gameObject);

        foreach (var controller in controllers)
        {
            controller.Dispose();
        }

        context.Dispose();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        foreach (var controller in controllers)
        {
            controller.OnGUI();
        }
    }

    public override void Update()
    {
        base.Update();

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

    public override void LateUpdate()
    {
        foreach (var controller in controllers)
        {
            controller.LateUpdate();
        }
    }

    private void OnNFTUsageChange()
    {
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
    }

    private void BuilderProjectPanelInfo(string title, string description) { HUDController.i.builderInWorldMainHud.SetBuilderProjectInfo(title, description); }

    private void CatalogReceived(string catalogJson)
    {
        isCatalogLoading = false;
        AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        CatalogLoaded();
    }

    public void CatalogLoaded()
    {
        catalogAdded = true;
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
        StartEditMode();
    }

    private void CatalogHeadersReceived(string rawHeaders)
    {
        catalogCallHeaders = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawHeaders);
        areCatalogHeadersReady = true;
        if (isCatalogRequested)
            GetCatalog();
    }

    private void GetCatalog()
    {
        if (catalogAdded)
            return;

        if (areCatalogHeadersReady)
            catalogAsyncOp = BIWUtils.MakeGetCall(BIWUrlUtils.GetUrlCatalog(), CatalogReceived, catalogCallHeaders);
        else
            builderInWorldBridge.AskKernelForCatalogHeaders();

        isCatalogRequested = true;
    }

    private void ConfigureLoadingController()
    {
        initialLoadingController = new BuilderInWorldLoadingController();
        initialLoadingController.Initialize();
    }

    private void InitControllers()
    {
        entityHandler.Init(context);
        modeController.Init(context);
        publishController.Init(context);
        creatorController.Init(context);
        outlinerController.Init(context);
        floorHandler.Init(context);
        inputHandler.Init(context);
        saveController.Init(context);
        actionController.Init(context);
        inputWrapper.Init(context);
        raycastController.Init(context);
        gizmosController.Init(context);

        controllers.Add(entityHandler);
        controllers.Add(modeController);
        controllers.Add(publishController);
        controllers.Add(creatorController);
        controllers.Add(outlinerController);
        controllers.Add(floorHandler);
        controllers.Add(inputHandler);
        controllers.Add(saveController);
        controllers.Add(actionController);
        controllers.Add(inputWrapper);
        controllers.Add(raycastController);
        controllers.Add(gizmosController);
    }

    private void StartTutorial() { TutorialController.i.SetBuilderInWorldTutorialEnabled(); }

    public void CleanItems()
    {
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.Dispose();

        if (Camera.main != null)
        {
            BIWOutline outliner = Camera.main.GetComponent<BIWOutline>();
            GameObject.Destroy(outliner);
        }

        floorHandler?.Clean();
        creatorController?.Clean();
    }

    public void ChangeEditModeStatusByShortcut(DCLAction_Trigger action)
    {
        if (!activeFeature)
            return;

        if (isEnteringEditMode)
            return;

        if (isBuilderInWorldActivated)
        {
            HUDController.i.builderInWorldMainHud.ExitStart();
            return;
        }

        FindSceneToEdit();

        if (!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
            return;
        }
        if (IsParcelSceneDeployedFromSDK(sceneToEdit))
        {
            ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
            return;
        }

        GetCatalog();
        TryStartEnterEditMode(true, null, "Shortcut");
    }

    private void NewSceneAdded(IParcelScene newScene)
    {
        if (newScene.sceneData.id != sceneToEditId)
            return;

        Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;

        sceneToEdit = (ParcelScene)Environment.i.world.state.GetScene(sceneToEditId);
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

    private bool UserHasPermissionOnParcelScene(ParcelScene sceneToCheck)
    {
        if (BYPASS_LAND_OWNERSHIP_CHECK)
            return true;

        List<Vector2Int> allParcelsWithAccess = landsWithAccess.SelectMany(land => land.parcels).ToList();
        foreach (Vector2Int parcel in allParcelsWithAccess)
        {
            if (sceneToCheck.sceneData.parcels.Any(currentParcel => currentParcel.x == parcel.x && currentParcel.y == parcel.y))
                return true;
        }

        return false;
    }

    private bool IsParcelSceneDeployedFromSDK(ParcelScene sceneToCheck)
    {
        List<DeployedScene> allDeployedScenesWithAccess = landsWithAccess.SelectMany(land => land.scenes).ToList();
        foreach (DeployedScene scene in allDeployedScenesWithAccess)
        {
            if (scene.source != DeployedScene.Source.SDK)
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
            ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
            return;
        }
        else if (IsParcelSceneDeployedFromSDK(sceneToEdit))
        {
            ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
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
        BIWAnalytics.StartEditorFlow(source);
        beginStartFlowTimeStamp = Time.realtimeSinceStartup;

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
        BIWNFTController.i.ClearNFTs();

        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        parcelUnityMiddlePoint = BIWUtils.CalculateUnityMiddlePoint(sceneToEdit);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.SetParcelScene(sceneToEdit);
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
            HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
            HUDController.i.builderInWorldMainHud.SetVisibilityOfCatalog(true);
            HUDController.i.builderInWorldMainHud.SetVisibilityOfInspector(true);
        }

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);
        DataStore.i.dataStoreBuilderInWorld.showTaskBar.Set(true);

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
                HUDController.i.builderInWorldMainHud?.SetVisibility(true);
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

        BIWAnalytics.AddSceneInfo(sceneToEdit.sceneData.basePosition, BIWUtils.GetLandOwnershipType(landsWithAccess, sceneToEdit).ToString(), BIWUtils.GetSceneSize(sceneToEdit));
        BIWAnalytics.EnterEditor( Time.realtimeSinceStartup - beginStartFlowTimeStamp);
    }

    private void OnAllParcelsFloorLoaded()
    {
        if (!initialLoadingController.isActive)
            return;

        floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(onHideAction: () =>
        {
            inputController.inputTypeMode = InputTypeMode.BUILD_MODE;
            HUDController.i.builderInWorldMainHud.SetVisibility(true);
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
        if (saveController.numberOfSaves > 0)
        {
            modeController.TakeSceneScreenshotForExit();

            HUDController.i.builderInWorldMainHud.ConfigureConfirmationModal(
                BIWSettings.EXIT_MODAL_TITLE,
                BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_SUBTITLE,
                BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_CANCEL_BUTTON,
                BIWSettings.EXIT_WITHOUT_PUBLISH_MODAL_CONFIRM_BUTTON);
        }
        else
        {
            HUDController.i.builderInWorldMainHud.ConfigureConfirmationModal(
                BIWSettings.EXIT_MODAL_TITLE,
                BIWSettings.EXIT_MODAL_SUBTITLE,
                BIWSettings.EXIT_MODAL_CANCEL_BUTTON,
                BIWSettings.EXIT_MODAL_CONFIRM_BUTTON);
        }
    }

    public void ExitEditMode()
    {
        Environment.i.platform.cullingController.Start();

        floorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(true);
        inputController.inputTypeMode = InputTypeMode.GENERAL;

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);
        DataStore.i.dataStoreBuilderInWorld.showTaskBar.Set(true);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);

        sceneToEdit.SetEditMode(false);

        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;

        InmediateExit();

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.ClearEntityList();
            HUDController.i.builderInWorldMainHud.SetVisibility(false);
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

        biwAudioHandler.gameObject.SetActive(false);
        DataStore.i.appMode.Set(AppMode.DEFAULT);
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

        biwAudioHandler.ExitEditMode();
    }

    public bool IsNewScene() { return sceneToEdit.entities.Count <= 0; }

    public void SetupNewScene() { floorHandler.CreateDefaultFloor(); }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position) { ExitEditMode(); }

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

    public void FindSceneToEdit()
    {
        foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (WorldStateUtils.IsCharacterInsideScene(scene))
            {
                ParcelScene parcelScene = (ParcelScene)scene;

                if (sceneToEdit != null && sceneToEdit != parcelScene)
                    actionController.Clear();

                sceneToEdit = parcelScene;
                break;
            }
        }
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

    private void OnUserProfileUpdate(UserProfile user)
    {
        userProfile.OnUpdate -= OnUserProfileUpdate;
        updateLandsWithAcessCoroutine = CoroutineStarter.Start(CheckLandsAccess());
    }

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
                                 KernelConfig.i.Get().tld,
                                 BIWSettings.CACHE_TIME_LAND,
                                 BIWSettings.CACHE_TIME_SCENES)
                             .Then(lands => landsWithAccess = lands.ToList());
    }

    private static void ShowGenericNotification(string message)
    {
        Notification.Model notificationModel = new Notification.Model();
        notificationModel.message = message;
        notificationModel.type = NotificationFactory.Type.GENERIC;
        notificationModel.timer = BIWSettings.LAND_NOTIFICATIONS_TIMER;
        HUDController.i.notificationHud.ShowNotification(notificationModel);
    }
}