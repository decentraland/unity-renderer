using Builder;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Tutorial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Environment = DCL.Environment;

public class BuilderInWorldController : MonoBehaviour
{
    [Header("Activation of Feature")]
    public bool activeFeature = false;

    public bool bypassLandOwnershipCheck = false;

    [Header("DesignVariables")]
    [SerializeField]
    private float distanceToDisableBuilderInWorld = 45f;

    [Header("Scene References")]
    public GameObject cameraParentGO;

    public GameObject cursorGO;
    public InputController inputController;
    public GameObject[] groundVisualsGO;

    [Header("Prefab References")]
    public BIWOutlinerController outlinerController;

    public BIWInputHandler bIWInputHandler;
    public BIWPublishController biwPublishController;
    public BIWCreatorController biwCreatorController;
    public BIWModeController biwModeController;
    public BIWFloorHandler biwFloorHandler;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;
    public BIWSaveController biwSaveController;
    public BuilderInWorldAudioHandler biwAudioHandler;

    [Header("Build Modes")]
    public BuilderInWorldGodMode editorMode;

    public LayerMask layerToRaycast;

    private ParcelScene sceneToEdit;

    [Header("Project References")]
    public Material skyBoxMaterial;

    [Header("Loading")]
    public BuilderInWorldLoadingView initialLoadingView;

    [HideInInspector]
    public bool isBuilderInWorldActivated = false;

    private GameObject editionGO;
    private GameObject undoGO;
    private GameObject snapGO;
    private GameObject freeMovementGO;
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
    private bool isCatalogRequested = false;
    private bool isEnteringEditMode = false;

    public event Action OnEnterEditMode;
    public event Action OnExitEditMode;

    internal IBuilderInWorldLoadingController initialLoadingController;

    private UserProfile userProfile;
    private List<LandWithAccess> landsWithAccess = new List<LandWithAccess>();
    private Coroutine updateLandsWithAcessCoroutine;
    private Dictionary<string, string> catalogCallHeaders;

    private void Awake()
    {
        BIWCatalogManager.Init();
        builderInWorldBridge.OnCatalogHeadersReceived += CatalogHeadersReceived;
    }

    void Start()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(config =>  EnableFeature(config.features.enableBuilderInWorld));
        KernelConfig.i.OnChange += OnKernelConfigChanged;
    }

    private void OnDestroy()
    {
        if (userProfile != null)
            userProfile.OnUpdate -= OnUserProfileUpdate;

        CoroutineStarter.Stop(updateLandsWithAcessCoroutine);

        if (sceneToEdit != null)
            sceneToEdit.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;

        Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;
        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;

        KernelConfig.i.OnChange -= OnKernelConfigChanged;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnTutorialAction -= StartTutorial;
            HUDController.i.builderInWorldMainHud.OnStartExitAction -= StartExitMode;
            HUDController.i.builderInWorldMainHud.OnLogoutAction -= ExitEditMode;
        }

        BuilderInWorldTeleportAndEdit.OnTeleportEnd -= OnPlayerTeleportedToEditScene;

        if (initialLoadingController != null)
            initialLoadingController.Dispose();


        BuilderInWorldNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;
        builderInWorldBridge.OnCatalogHeadersReceived -= CatalogHeadersReceived;
        CleanItems();

        HUDController.i.OnBuilderProjectPanelCreation -= InitBuilderProjectPanel;
    }

    private void Update()
    {
        if (isCatalogLoading && catalogAsyncOp?.webRequest != null)
            UpdateCatalogLoadingProgress(catalogAsyncOp.webRequest.downloadProgress * 100);

        if (!isBuilderInWorldActivated)
            return;

        if (checkerInsideSceneOptimizationCounter >= 60)
        {
            if (Vector3.Distance(DCLCharacterController.i.characterPosition.unityPosition, parcelUnityMiddlePoint) >= distanceToDisableBuilderInWorld)
                ExitEditMode();
            checkerInsideSceneOptimizationCounter = 0;
        }
        else
        {
            checkerInsideSceneOptimizationCounter++;
        }
    }

    private void OnNFTUsageChange()
    {
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { EnableFeature(current.features.enableBuilderInWorld); }

    private void EnableFeature(bool enable)
    {
        activeFeature = enable;

        if (enable)
            Init();
    }

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

    public void Init()
    {
        if (isInit)
            return;

        isInit = true;

        userProfile = UserProfile.GetOwnUserProfile();
        if (!string.IsNullOrEmpty(userProfile.userId))
            updateLandsWithAcessCoroutine = CoroutineStarter.Start(CheckLandsAccess());
        else
            userProfile.OnUpdate += OnUserProfileUpdate;

        InitGameObjects();

        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_MAIN);
        HUDController.i.OnBuilderProjectPanelCreation += InitBuilderProjectPanel;

        HUDController.i.builderInWorldMainHud.Initialize();

        HUDController.i.builderInWorldMainHud.OnTutorialAction += StartTutorial;
        HUDController.i.builderInWorldMainHud.OnStartExitAction += StartExitMode;
        HUDController.i.builderInWorldMainHud.OnLogoutAction += ExitEditMode;

        if (HUDController.i.builderProjectsPanelController != null)
            HUDController.i.builderProjectsPanelController.OnJumpInOrEdit += GetCatalog;

        BuilderInWorldTeleportAndEdit.OnTeleportEnd += OnPlayerTeleportedToEditScene;

        ConfigureLoadingController();
        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        builderInWorldBridge.AskKernelForCatalogHeaders();

        isCatalogLoading = true;
        BuilderInWorldNFTController.i.Initialize();
        BuilderInWorldNFTController.i.OnNFTUsageChange += OnNFTUsageChange;
    }

    private void InitBuilderProjectPanel()
    {
        if (HUDController.i.builderProjectsPanelController != null)
            HUDController.i.builderProjectsPanelController.OnJumpInOrEdit += GetCatalog;
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
            catalogAsyncOp = BuilderInWorldUtils.MakeGetCall(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived, catalogCallHeaders);
        else
            builderInWorldBridge.AskKernelForCatalogHeaders();

        isCatalogRequested = true;
    }

    private void ConfigureLoadingController()
    {
        initialLoadingController = new BuilderInWorldLoadingController();
        initialLoadingController.Initialize(initialLoadingView);
    }

    public void InitGameObjects()
    {
        if (snapGO == null)
            snapGO = new GameObject("SnapGameObject");

        snapGO.transform.SetParent(transform);

        if (freeMovementGO == null)
            freeMovementGO = new GameObject("FreeMovementGO");

        freeMovementGO.transform.SetParent(cameraParentGO.transform);

        if (editionGO == null)
            editionGO = new GameObject("EditionGO");

        editionGO.transform.SetParent(cameraParentGO.transform);

        if (undoGO == null)
        {
            undoGO = new GameObject("UndoGameObject");
            undoGO.transform.SetParent(transform);
        }
    }

    public void InitControllers()
    {
        builderInWorldEntityHandler.Init();
        biwModeController.Init(editionGO, undoGO, snapGO, freeMovementGO);
        biwPublishController.Init();
        biwCreatorController.Init();
        outlinerController.Init();
        biwFloorHandler.Init();
        bIWInputHandler.Init();
        biwSaveController.Init();
        actionController.Init();
        biwAudioHandler.Init();
    }

    private void StartTutorial() { TutorialController.i.SetBuilderInWorldTutorialEnabled(); }

    public void CleanItems()
    {
        Destroy(undoGO);
        Destroy(snapGO);
        Destroy(editionGO);
        Destroy(freeMovementGO);

        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.Dispose();

        if (Camera.main != null)
        {
            DCLBuilderOutline outliner = Camera.main.GetComponent<DCLBuilderOutline>();
            Destroy(outliner);
        }

        biwFloorHandler?.Clean();
        biwCreatorController?.Clean();
    }

    [ContextMenu("Activate feature")]
    public void ActivateFeature()
    {
        activeFeature = true;
        HUDController.i.taskbarHud.SetBuilderInWorldStatus(activeFeature);
    }

    public void ChangeEditModeStatusByShortcut()
    {
        if (!activeFeature)
            return;

        if (isEnteringEditMode)
            return;

        if (!isBuilderInWorldActivated)
        {
            GetCatalog();
            TryStartEnterEditMode();
        }
        else
        {
            HUDController.i.builderInWorldMainHud.ExitStart();
        }
    }

    public VoxelEntityHit GetCloserUnselectedVoxelEntityOnPointer()
    {
        RaycastHit[] hits;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;

        hits = Physics.RaycastAll(ray, BuilderInWorldSettings.RAYCAST_MAX_DISTANCE, layerToRaycast);

        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                DCLBuilderInWorldEntity entityToCheck = builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);

                if (entityToCheck == null)
                    continue;

                Camera camera = Camera.main;

                if (!entityToCheck.IsSelected && entityToCheck.tag == BuilderInWorldSettings.VOXEL_TAG)
                {
                    if (Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                    {
                        voxelEntityHit = new VoxelEntityHit(entityToCheck, hit);
                        currentDistance = Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                    }
                }
            }
        }

        return voxelEntityHit;
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
        if (bypassLandOwnershipCheck)
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

    public void TryStartEnterEditMode() { TryStartEnterEditMode(true); }

    public void TryStartEnterEditMode(bool activateCamera)
    {
        if (sceneToEditId != null)
            return;

        FindSceneToEdit();

        if (!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            ShowGenericNotification(BuilderInWorldSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
            return;
        }
        else if (IsParcelSceneDeployedFromSDK(sceneToEdit))
        {
            ShowGenericNotification(BuilderInWorldSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
            return;
        }

        isEnteringEditMode = true;
        previousAllUIHidden = CommonScriptableObjects.allUIHidden.Get();
        NotificationsController.i.allowNotifications = false;
        CommonScriptableObjects.allUIHidden.Set(true);
        NotificationsController.i.allowNotifications = true;
        inputController.inputTypeMode = InputTypeMode.BUILD_MODE_LOADING;
        initialLoadingController.Show();
        initialLoadingController.SetPercentage(0f);
        DataStore.i.appMode.Set(AppMode.BUILDER_IN_WORLD_EDITION);

        //Note (Adrian) this should handle different when we have the full flow of the feature
        if (activateCamera)
            editorMode.ActivateCamera(sceneToEdit);

        if (catalogAdded)
            StartEditMode();
    }

    private void StartEditMode()
    {
        if (sceneToEdit == null)
            return;

        isEnteringEditMode = true;

        sceneToEditId = sceneToEdit.sceneData.id;

        // In this point we're sure that the catalog loading (the first half of our progress bar) has already finished
        initialLoadingController.SetPercentage(50f);
        Environment.i.world.sceneController.OnNewSceneAdded += NewSceneAdded;
        Environment.i.world.sceneController.OnReadyScene += NewSceneReady;
        Environment.i.world.blockersController.SetEnabled(false);

        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
    }

    public void EnterEditMode()
    {
        if (!initialLoadingController.isActive)
            return;

        isEnteringEditMode = false;
        BuilderInWorldNFTController.i.ClearNFTs();

        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        FindSceneToEdit();

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        parcelUnityMiddlePoint = BuilderInWorldUtils.CalculateUnityMiddlePoint(sceneToEdit);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.SetParcelScene(sceneToEdit);
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
            HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        }

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

        EnterBiwControllers();
        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

        initialLoadingController.SetPercentage(100f);

        if (IsNewScene())
        {
            biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
            biwFloorHandler.OnAllParcelsFloorLoaded += OnAllParcelsFloorLoaded;
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

        OnEnterEditMode?.Invoke();
    }

    private void OnAllParcelsFloorLoaded()
    {
        if (!initialLoadingController.isActive)
            return;

        biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
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
            editorMode.OpenNewProjectDetails();
    }

    public void StartExitMode()
    {
        if (biwSaveController.numberOfSaves > 0)
            editorMode.TakeSceneScreenshotForExit();
    }

    public void ExitEditMode()
    {
        if (biwSaveController.numberOfSaves > 0)
        {
            HUDController.i.builderInWorldMainHud?.SaveSceneInfo();
            biwSaveController.ResetNumberOfSaves();
        }

        biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(true);
        inputController.inputTypeMode = InputTypeMode.GENERAL;

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        snapGO.transform.SetParent(transform);

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

        OnExitEditMode?.Invoke();
        DataStore.i.appMode.Set(AppMode.DEFAULT);
    }

    public void InmediateExit()
    {
        CommonScriptableObjects.allUIHidden.Set(previousAllUIHidden);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);
    }

    public void EnterBiwControllers()
    {
        biwModeController.EnterEditMode(sceneToEdit);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);
        biwFloorHandler.EnterEditMode(sceneToEdit);
        biwCreatorController.EnterEditMode(sceneToEdit);
        biwPublishController.EnterEditMode(sceneToEdit);
        bIWInputHandler.EnterEditMode(sceneToEdit);
        outlinerController.EnterEditMode(sceneToEdit);
        biwSaveController.EnterEditMode(sceneToEdit);
        actionController.EnterEditMode(sceneToEdit);
        biwAudioHandler.EnterEditMode(sceneToEdit);
    }

    public void ExitBiwControllers()
    {
        biwModeController.ExitEditMode();
        builderInWorldEntityHandler.ExitEditMode();
        biwFloorHandler.ExitEditMode();
        biwCreatorController.ExitEditMode();
        biwPublishController.ExitEditMode();
        bIWInputHandler.ExitEditMode();
        outlinerController.ExitEditMode();
        biwSaveController.ExitEditMode();
        actionController.ExitEditMode();
        biwAudioHandler.ExitEditMode();
    }

    public bool IsNewScene() { return sceneToEdit.entities.Count <= 0; }

    public void SetupNewScene() { biwFloorHandler.CreateDefaultFloor(); }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position) { ExitEditMode(); }

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
            TryStartEnterEditMode();
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
            yield return WaitForSecondsCache.Get(BuilderInWorldSettings.REFRESH_LANDS_WITH_ACCESS_INTERVAL);
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
                                 BuilderInWorldSettings.CACHE_TIME_LAND,
                                 BuilderInWorldSettings.CACHE_TIME_SCENES)
                             .Then(lands => landsWithAccess = lands.ToList());
    }

    private static void ShowGenericNotification(string message)
    {
        Notification.Model notificationModel = new Notification.Model();
        notificationModel.message = message;
        notificationModel.type = NotificationFactory.Type.GENERIC;
        HUDController.i.notificationHud.ShowNotification(notificationModel);
    }
}