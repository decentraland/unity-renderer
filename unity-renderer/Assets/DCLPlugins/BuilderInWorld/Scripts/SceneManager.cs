using System;
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

        private const string SOURCE_BUILDER_PANEl = "BuilderPanel";
        private const string SOURCE_SHORTCUT = "Shortcut";

        public enum State
        {
            IDLE = 0,
            LOADING_CATALOG = 1,
            CATALOG_LOADED = 2,
            PREPARE_SCENE = 3,
            LOADING_SCENE = 4,
            SCENE_LOADED = 5,
            EDITING = 6
        }

        internal State currentState = State.IDLE;

        private InputAction_Trigger editModeChangeInputAction;

        internal IContext context;
        internal string sceneToEditId;
        private UserProfile userProfile;
        internal Coroutine updateLandsWithAcessCoroutine;

        private bool alreadyAskedForLandPermissions = false;
        private Vector3 askPermissionLastPosition;
        private IWebRequestAsyncOperation catalogAsyncOp;
        internal bool isWaitingForPermission = false;
        internal IParcelScene sceneToEdit;
        private BiwSceneMetricsAnalyticsHelper sceneMetricsAnalyticsHelper;
        private InputController inputController;
        internal BuilderInWorldBridge builderInWorldBridge;
        internal IBuilderInWorldLoadingController initialLoadingController;
        private float beginStartFlowTimeStamp = 0;
        internal ICameraController cameraController;

        internal bool catalogLoaded = false;

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
            
            editModeChangeInputAction.OnTriggered -= ChangeEditModeStatusByShortcut;
            context.builderAPIController.OnWebRequestCreated -= WebRequestCreated;

            CoroutineStarter.Stop(updateLandsWithAcessCoroutine);
        }

        private void ConfigureLoadingController()
        {
            initialLoadingController = new BuilderInWorldLoadingController();
            initialLoadingController.Initialize();
        }

        public void NextState()
        {
            currentState++;
            switch (currentState)
            {
                case State.LOADING_CATALOG:
                    if (!catalogLoaded)
                        GetCatalog();
                    else
                        NextState();
                    break;

                case State.CATALOG_LOADED:
                    NextState();
                    break;

                //TODO: This step wil be implemented in the future
                case State.PREPARE_SCENE:
                    NextState();
                    break;

                case State.LOADING_SCENE:
                    LoadScene();
                    break;

                case State.SCENE_LOADED:
                    EnterEditMode();
                    break;
            }
        }

        public void WebRequestCreated(IWebRequestAsyncOperation webRequest)
        {
            if (currentState == State.LOADING_CATALOG)
                catalogAsyncOp = webRequest;
        }

        public void Update()
        {
            if (currentState != State.LOADING_CATALOG)
                return;

            if (catalogAsyncOp?.webRequest != null)
                UpdateCatalogLoadingProgress(catalogAsyncOp.webRequest.downloadProgress * 100);
        }

        private void OnPlayerTeleportedToEditScene(Vector2Int coords)
        {
            var targetScene = Environment.i.world.state.scenesSortedByDistance
                                         .FirstOrDefault(scene => scene.sceneData.parcels.Contains(coords));
            TryStartFlow(targetScene,SOURCE_BUILDER_PANEl);
        }

        public IParcelScene FindSceneToEdit()
        {
            foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
            {
                if (WorldStateUtils.IsCharacterInsideScene(scene))
                    return scene;
            }

            return null;
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

        internal void CatalogLoaded()
        {
            catalogLoaded = true;
            if ( context.editorContext.editorHUD != null)
                context.editorContext.editorHUD.RefreshCatalogContent();
            NextState();
        }

        internal void StartFlow(string source)
        {
            if (currentState != State.IDLE)
                return;

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

            NextState();
        }

        internal void GetCatalog()
        {
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
        }

        public void ChangeEditModeStatusByShortcut(DCLAction_Trigger action)
        {
            if (currentState != State.EDITING && currentState != State.IDLE)
                return;

            if (currentState == State.EDITING )
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
            var scene = FindSceneToEdit();
            TryStartFlow(scene, SOURCE_SHORTCUT);
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
            NextState();
        }

        internal bool UserHasPermissionOnParcelScene(IParcelScene sceneToCheck)
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

        internal bool IsParcelSceneDeployedFromSDK(IParcelScene sceneToCheck)
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

        internal void EnterEditMode()
        {
            initialLoadingController.SetPercentage(100f);
            initialLoadingController.Hide(true, onHideAction: () =>
            {
                inputController.inputTypeMode = InputTypeMode.BUILD_MODE;
                context.editorContext.editorHUD?.SetVisibility(true);
                CommonScriptableObjects.allUIHidden.Set(true);
                OpenNewProjectDetails();
            });

            DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

            context.editor.EnterEditMode(sceneToEdit);
            BIWAnalytics.EnterEditor( Time.realtimeSinceStartup - beginStartFlowTimeStamp);
        }

        internal void ExitEditMode()
        {
            currentState = State.IDLE;
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

            cameraController.TakeSceneScreenshot((sceneSnapshot) =>
            {
                context.editorContext.editorHUD?.NewProjectStart(sceneSnapshot);
            });

        }

        public void TryStartFlow(IParcelScene targetScene, string source)
        {
            if (currentState != State.IDLE || targetScene == null)
                return;

            if (!UserHasPermissionOnParcelScene(targetScene))
            {
                BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_PERMISSIONS_MESSAGE);
                return;
            }
            else if (IsParcelSceneDeployedFromSDK(targetScene))
            {
                BIWUtils.ShowGenericNotification(BIWSettings.LAND_EDITION_NOT_ALLOWED_BY_SDK_LIMITATION_MESSAGE);
                return;
            }
            
            sceneToEdit = targetScene;
            StartFlow(source);
        }

        private void LoadScene()
        {
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

        private IEnumerator CheckLandsAccess()
        {
            while (true)
            {
                UpdateLandsWithAccess();
                yield return WaitForSecondsCache.Get(BIWSettings.REFRESH_LANDS_WITH_ACCESS_INTERVAL);
            }
        }
    }
}