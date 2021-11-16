using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Camera;
using DCL.Configuration;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public class SceneManager : ISceneManager
    {
        internal static bool BYPASS_LAND_OWNERSHIP_CHECK = false;
        private const float MAX_DISTANCE_STOP_TRYING_TO_ENTER = 16;

        private InputAction_Trigger editModeChangeInputAction;

        internal IContext context;
        internal string sceneToEditId;
        private UserProfile userProfile;
        internal Coroutine updateLandsWithAcessCoroutine;

        internal bool catalogAdded = false;
        private bool sceneReady = false;
        private bool alreadyAskedForLandPermissions = false;
        private Vector3 askPermissionLastPosition;
        private IWebRequestAsyncOperation catalogAsyncOp;
        internal bool isWaitingForPermission = false;
        private bool isCatalogLoading = false;
        internal ParcelScene sceneToEdit;
        internal bool isCatalogRequested = false;
        internal bool isEnteringEditMode = false;
        internal bool isEditingScene = false;
        private BiwSceneMetricsAnalyticsHelper sceneMetricsAnalyticsHelper;
        private InputController inputController;
        private BuilderInWorldBridge builderInWorldBridge;
        internal IBuilderInWorldLoadingController initialLoadingController;
        private float beginStartFlowTimeStamp = 0;
        private CameraController cameraController;

        public void Initialize(IContext context)
        {
            this.context = context;
            editModeChangeInputAction = context.inputsReferencesAsset.editModeChangeInputAction;
            editModeChangeInputAction.OnTriggered += ChangeEditModeStatusByShortcut;
            inputController = context.sceneReferences.inputController;

            builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
            userProfile = UserProfile.GetOwnUserProfile();

            cameraController = new CameraController();
            cameraController.Initialize(context);

            context.editorContext.editorHUD.OnLogoutAction += ExitEditMode;

            BIWTeleportAndEdit.OnTeleportEnd += OnPlayerTeleportedToEditScene;
            context.builderAPIController.OnWebRequestCreated += WebRequestCreated;

            ConfigureLoadingController();
            if (context.panelHUD != null)
                context.panelHUD.OnJumpInOrEdit += GetCatalog;

        }

        public void Dispose()
        {
            if (context.editorContext.editorHUD != null)
                context.editorContext.editorHUD.OnLogoutAction -= ExitEditMode;

            cameraController.Dispose();
            sceneMetricsAnalyticsHelper?.Dispose();

            initialLoadingController?.Dispose();

            Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;
            Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;
            BIWTeleportAndEdit.OnTeleportEnd -= OnPlayerTeleportedToEditScene;
            DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;

            if (sceneToEdit != null)
                sceneToEdit.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;

            if (context.panelHUD != null)
                context.panelHUD.OnJumpInOrEdit -= GetCatalog;
            editModeChangeInputAction.OnTriggered -= ChangeEditModeStatusByShortcut;
            context.builderAPIController.OnWebRequestCreated -= WebRequestCreated;

            CoroutineStarter.Stop(updateLandsWithAcessCoroutine);
        }

        private void ConfigureLoadingController()
        {
            initialLoadingController = new BuilderInWorldLoadingController();
            initialLoadingController.Initialize();
        }

        public void WebRequestCreated(IWebRequestAsyncOperation webRequest)
        {
            if (isCatalogLoading)
                catalogAsyncOp = webRequest;
        }

        public void Update()
        {
            if (!isEditingScene && !isEnteringEditMode)
                return;

            if (isCatalogLoading && catalogAsyncOp?.webRequest != null)
                UpdateCatalogLoadingProgress(catalogAsyncOp.webRequest.downloadProgress * 100);
        }

        private void OnPlayerTeleportedToEditScene(Vector2Int coords)
        {
            var targetScene = Environment.i.world.state.scenesSortedByDistance
                                         .FirstOrDefault(scene => scene.sceneData.parcels.Contains(coords));
            TryStartEnterEditMode(targetScene);

        }

        public void FindSceneToEdit(IParcelScene targetScene)
        {
            if (targetScene != null)
            {
                var parcelSceneTarget = (ParcelScene)targetScene;
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
                    sceneToEdit = parcelScene;
                    return sceneToEdit;
                }
            }

            return null;
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

            isCatalogRequested = true;
        }

        public void ChangeEditModeStatusByShortcut(DCLAction_Trigger action)
        {
            if (isEnteringEditMode)
                return;

            if (isEditingScene)
            {
                context.editorContext.editorHUD.ExitStart();
                return;
            }

            if (DataStore.i.builderInWorld.landsWithAccess.Get().Length == 0 && !alreadyAskedForLandPermissions)
            {
                ActivateLandAccessBackgroundChecker();
                ShowGenericNotification(BIWSettings.LAND_EDITION_WAITING_FOR_PERMISSIONS_MESSAGE, DCL.NotificationModel.Type.GENERIC_WITHOUT_BUTTON, BIWSettings.LAND_CHECK_MESSAGE_TIMER);
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
                ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
                return;
            }

            if (IsParcelSceneDeployedFromSDK(sceneToEdit))
            {
                ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
                return;
            }

            GetCatalog();
            TryStartEnterEditMode(null, "Shortcut");
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

        internal void EnterEditMode()
        {
            isEnteringEditMode = false;
            initialLoadingController.SetPercentage(100f);
            initialLoadingController.Hide(onHideAction: () =>
            {
                inputController.inputTypeMode = InputTypeMode.BUILD_MODE;
                context.editorContext.editorHUD?.SetVisibility(true);
                CommonScriptableObjects.allUIHidden.Set(true);
                OpenNewProjectDetails();
            });

            isEditingScene = true;
            DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

            context.editor.EnterEditMode(sceneToEdit);
            BIWAnalytics.EnterEditor( Time.realtimeSinceStartup - beginStartFlowTimeStamp);
        }

        internal void ExitEditMode()
        {
            isEditingScene = false;
            initialLoadingController.Hide(true);
            inputController.inputTypeMode = InputTypeMode.GENERAL;
            CommonScriptableObjects.allUIHidden.Set(false);
            cameraController.DeactivateCamera();
            context.editor.ExitEditMode();

            DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        }

        internal void OpenNewProjectDetails()
        {
            if (!builderInWorldBridge.builderProject.isNewEmptyProject)
                return;

            cameraController.TakeSceneScreenshotFromResetPosition((sceneSnapshot) =>
            {
                context.editorContext.editorHUD?.NewProjectStart(sceneSnapshot);
            });

        }

        public void TryStartEnterEditMode(IParcelScene targetScene = null , string source = "BuilderPanel")
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

            cameraController.ActivateCamera(sceneToEdit);

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

        private void UpdateSceneLoadingProgress(float sceneLoadingProgress) { initialLoadingController.SetPercentage(50f + (sceneLoadingProgress / 2)); }

        private void UpdateCatalogLoadingProgress(float catalogLoadingProgress) { initialLoadingController.SetPercentage(catalogLoadingProgress / 2); }

        internal void ExitAfterCharacterTeleport(DCLCharacterPosition position) { ExitEditMode(); }

        private static void ShowGenericNotification(string message, DCL.NotificationModel.Type type = DCL.NotificationModel.Type.GENERIC, float timer = BIWSettings.LAND_NOTIFICATIONS_TIMER )
        {
            DCL.NotificationModel.Model notificationModel = new DCL.NotificationModel.Model();
            notificationModel.message = message;
            notificationModel.type = type;
            notificationModel.timer = timer;
            if (HUDController.i.notificationHud != null)
                HUDController.i.notificationHud.ShowNotification(notificationModel);
        }
    }
}