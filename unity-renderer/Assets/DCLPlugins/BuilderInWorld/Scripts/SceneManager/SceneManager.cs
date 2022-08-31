using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Camera;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DCL.Builder.Manifest;
using DCL.Components;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            LOADING_SCENE = 3,
            SCENE_LOADED = 4,
            EDITING = 5
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
        internal IBuilderScene sceneToEdit;
        private BiwSceneMetricsAnalyticsHelper sceneMetricsAnalyticsHelper;
        private InputController inputController;
        internal BuilderInWorldBridge builderInWorldBridge;
        internal IInitialStateManager initialStateManager;
        internal IBuilderInWorldLoadingController initialLoadingController;
        private float beginStartFlowTimeStamp = 0;
        internal bool catalogLoaded = false;
        internal List<string> portableExperiencesToResume = new List<string>();
        private BuilderInWorldBridge bridge;

        public void Initialize(IContext context)
        {
            this.context = context;
            editModeChangeInputAction = context.inputsReferencesAsset.editModeChangeInputAction;
            editModeChangeInputAction.OnTriggered += ChangeEditModeStatusByShortcut;
            inputController = context.sceneReferences.inputController;

            builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
            userProfile = UserProfile.GetOwnUserProfile();

            bridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();
            
            context.editorContext.editorHUD.OnStartExitAction += StartExitMode;
            context.editorContext.editorHUD.OnLogoutAction += ExitEditMode;

            BIWTeleportAndEdit.OnTeleportEnd += OnPlayerTeleportedToEditScene;
            context.builderAPIController.OnWebRequestCreated += WebRequestCreated;

            initialStateManager = new InitialStateManager();

            ConfigureLoadingController();
        }

        public void Dispose()
        {
            if (context.editorContext.editorHUD != null)
            {
                context.editorContext.editorHUD.OnStartExitAction -= StartExitMode;
                context.editorContext.editorHUD.OnLogoutAction -= ExitEditMode;
            }

            sceneMetricsAnalyticsHelper?.Dispose();

            initialLoadingController?.Dispose();

            Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;
            Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;
            BIWTeleportAndEdit.OnTeleportEnd -= OnPlayerTeleportedToEditScene;
            DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;

            if (sceneToEdit?.scene != null)
                sceneToEdit.scene.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;

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

                case State.LOADING_SCENE:
                    LoadScene();
                    break;

                case State.SCENE_LOADED:
                    EnterEditMode();
                    break;
            }
        }

        private void SendManifestToScene()
        {
            //We remove the old assets to they don't collide with the new ones
            BIWUtils.RemoveAssetsFromCurrentScene();

            //We add the assets from the scene to the catalog
            var assets = sceneToEdit.manifest.scene.assets.Values.ToArray();
            AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(assets);

            //We prepare the mappings to the scenes
            Dictionary<string, string> contentDictionary = new Dictionary<string, string>();

            foreach (var sceneObject in assets)
            {
                foreach (var content in sceneObject.contents)
                {
                    if (!contentDictionary.ContainsKey(content.Key))
                        contentDictionary.Add(content.Key, content.Value);
                }
            }

            //We add the mappings to the scene
            BIWUtils.AddSceneMappings(contentDictionary, BIWUrlUtils.GetUrlSceneObjectContent(), sceneToEdit.scene.sceneData);

            // We iterate all the entities to create the entity in the scene
            foreach (BuilderEntity builderEntity in sceneToEdit.manifest.scene.entities.Values)
            {
                var entity = sceneToEdit.scene.CreateEntity(builderEntity.id.GetHashCode());

                bool nameComponentFound = false;
                // We iterate all the id of components in the entity, to add the component 
                foreach (string idComponent in builderEntity.components)
                {
                    //This shouldn't happen, the component should be always in the scene, but just in case
                    if (!sceneToEdit.manifest.scene.components.ContainsKey(idComponent))
                        continue;

                    // We get the component from the scene and create it in the entity
                    BuilderComponent component = sceneToEdit.manifest.scene.components[idComponent];

                    switch (component.type)
                    {
                        case "Transform":
                            DCLTransform.Model model;
                            
                            try
                            {
                                // This is a very ugly way to handle the things because some times the data can come serialize and other times it wont 
                                // It will be deleted when we create the new builder server that only have 1 way to handle everything
                                model = JsonConvert.DeserializeObject<DCLTransform.Model>(component.data.ToString());
                            }
                            catch (Exception e)
                            {
                                // We may have create the component so de data is not serialized
                                model = JsonConvert.DeserializeObject<DCLTransform.Model>(JsonConvert.SerializeObject(component.data));
                            }
                            
                            EntityComponentsUtils.AddTransformComponent(sceneToEdit.scene, entity, model);
                            break;

                        case "GLTFShape":
                            LoadableShape.Model gltfModel;
                            
                            try
                            {
                                // This is a very ugly way to handle the things because some times the data can come serialize and other times it wont
                                // It will be deleted when we create the new builder server that only have 1 way to handle everything
                                gltfModel = JsonConvert.DeserializeObject<LoadableShape.Model>(component.data.ToString());
                            }
                            catch (Exception e)
                            {
                                // We may have create the component so de data is not serialized
                                gltfModel = (GLTFShape.Model)component.data;
                            }
                            
                            EntityComponentsUtils.AddGLTFComponent(sceneToEdit.scene, entity, gltfModel, component.id);
                            break;

                        case "NFTShape":
                            //Builder use a different way to load the NFT so we convert it to our system
                            JObject jObject = JObject.Parse(component.data.ToString());
                         
                            string url = jObject["url"].ToString();
                            string assedId = url.Replace(BIWSettings.NFT_ETHEREUM_PROTOCOL, "");
                            int index = assedId.IndexOf("/", StringComparison.Ordinal);
                            string partToremove = assedId.Substring(index);
                            assedId = assedId.Replace(partToremove, "");

                            NFTShape.Model nftModel = new NFTShape.Model();
                            nftModel.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
                            nftModel.src = url;
                            nftModel.assetId = assedId;

                            EntityComponentsUtils.AddNFTShapeComponent(sceneToEdit.scene, entity, nftModel, component.id);
                            break;

                        case "Name":
                            nameComponentFound = true;
                            DCLName.Model nameModel = JsonConvert.DeserializeObject<DCLName.Model>(component.data.ToString());
                            nameModel.builderValue = builderEntity.name;
                            EntityComponentsUtils.AddNameComponent(sceneToEdit.scene , entity, nameModel, Guid.NewGuid().ToString());
                            break;

                        case "LockedOnEdit":
                            DCLLockedOnEdit.Model lockedModel = JsonConvert.DeserializeObject<DCLLockedOnEdit.Model>(component.data.ToString());
                            EntityComponentsUtils.AddLockedOnEditComponent(sceneToEdit.scene , entity, lockedModel, Guid.NewGuid().ToString());
                            break;
                        case "Script":
                            SmartItemComponent.Model smartModel = JsonConvert.DeserializeObject<SmartItemComponent.Model>(component.data.ToString());
                            sceneToEdit.scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, smartModel);
                            break;
                    }
                }

                // We need to mantain the builder name of the entity, so we create the equivalent part in biw. We do this so we can maintain the smart-item references
                if (!nameComponentFound)
                {
                    DCLName.Model nameModel = new DCLName.Model();
                    nameModel.value = builderEntity.name;
                    nameModel.builderValue = builderEntity.name;
                    EntityComponentsUtils.AddNameComponent(sceneToEdit.scene , entity, nameModel, Guid.NewGuid().ToString());
                }
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
            StartFlowFromLandWithPermission(Environment.i.world.state.GetScene(coords), SOURCE_BUILDER_PANEl);
        }

        public void StartFlowFromProject(Manifest.Manifest manifest)
        {
            bool hasBeenCreatedThisSession = !string.IsNullOrEmpty(DataStore.i.builderInWorld.lastProjectIdCreated.Get()) && DataStore.i.builderInWorld.lastProjectIdCreated.Get() == manifest.project.id;
            DataStore.i.builderInWorld.lastProjectIdCreated.Set("");
            
            BuilderScene builderScene = new BuilderScene(manifest, IBuilderScene.SceneType.PROJECT,hasBeenCreatedThisSession);
            StartFlow(builderScene, SOURCE_BUILDER_PANEl);
        }

        public void StartFlowFromLandCoords(Vector2Int coords)
        {
            Scene deployedScene = GetDeployedSceneFromParcel(coords);
            Vector2Int parcelSize = BIWUtils.GetSceneSize(deployedScene.parcels);

            StartFromLand(deployedScene.@base, deployedScene, parcelSize, SOURCE_BUILDER_PANEl);
        }

        public void ShowBuilderLoading() 
        {    
            initialLoadingController.Show();
            initialLoadingController.SetPercentage(0f);
        }

        public void HideBuilderLoading()
        {
            initialLoadingController.Hide();
        }

        public void StartExitMode()
        {
            context.cameraController.TakeSceneScreenshotFromResetPosition((sceneSnapshot) =>
            {
                if (sceneSnapshot != null)
                {
                    sceneToEdit.sceneScreenshotTexture = sceneSnapshot;

                    if (sceneToEdit.manifest != null)
                        context.builderAPIController.SetThumbnail(sceneToEdit.manifest.project.id, sceneSnapshot);
                }
            });
            
            if (context.editorContext.editorHUD == null )
                return;
            
            if (context.editorContext.publishController.HasUnpublishChanges() && sceneToEdit.sceneType == IBuilderScene.SceneType.LAND)
            {
                if (sceneToEdit.sceneType == IBuilderScene.SceneType.LAND)
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

        public IParcelScene FindSceneToEdit()
        {
            foreach (IParcelScene scene in Environment.i.world.state.GetScenesSortedByDistance())
            {
                if (WorldStateUtils.IsCharacterInsideScene(scene))
                    return scene;
            }

            return null;
        }

        private void UpdateLandsWithAccess()
        {
            ICatalyst catalyst = Environment.i.platform.serviceProviders.catalyst;
            ITheGraph theGraph = Environment.i.platform.serviceProviders.theGraph;

            DeployedScenesFetcher.FetchLandsFromOwner(
                                     catalyst,
                                     theGraph,
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

        internal void StartFlow(IBuilderScene targetScene, string source)
        {
            if (currentState != State.IDLE || targetScene == null)
                return;

            DataStore.i.exploreV2.isOpen.Set(false);

            sceneToEditId = targetScene.manifest.project.scene_id;
            sceneToEdit = targetScene;

            NotificationsController.i.allowNotifications = false;
            CommonScriptableObjects.allUIHidden.Set(true);
            NotificationsController.i.allowNotifications = true;
            inputController.inputTypeMode = InputTypeMode.BUILD_MODE_LOADING;
            
            // We prepare the bridge for the current scene
            bridge.SetScene(targetScene);

            // We configure the loading part
            ShowBuilderLoading();

            DataStore.i.common.appMode.Set(AppMode.BUILDER_IN_WORLD_EDITION);
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
            BIWAnalytics.StartEditorFlow(source);
            beginStartFlowTimeStamp = Time.realtimeSinceStartup;

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
            StartFlowFromLandWithPermission(scene, SOURCE_SHORTCUT);
        }

        internal void NewSceneAdded(IParcelScene newScene)
        {
            if (newScene.sceneData.id != sceneToEditId)
                return;

            Environment.i.world.sceneController.OnNewSceneAdded -= NewSceneAdded;

            var scene = Environment.i.world.state.GetScene(sceneToEditId);
            sceneToEdit.SetScene(scene);
            sceneMetricsAnalyticsHelper = new BiwSceneMetricsAnalyticsHelper(sceneToEdit.scene);
            sceneToEdit.scene.OnLoadingStateUpdated += UpdateSceneLoadingProgress;
            SendManifestToScene();
            context.cameraController.ActivateCamera(sceneToEdit.scene);
        }

        private void NewSceneReady(string id)
        {
            if (sceneToEditId != id)
                return;

            sceneToEdit.scene.OnLoadingStateUpdated -= UpdateSceneLoadingProgress;
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

        internal Scene GetDeployedSceneFromParcel(Vector2Int coords)
        {
            List<Scene> allDeployedScenesWithAccess = DataStore.i.builderInWorld.landsWithAccess.Get().SelectMany(land => land.scenes).ToList();
            foreach (Scene scene in allDeployedScenesWithAccess)
            {
                List<Vector2Int> scenes = scene.parcels.ToList();
                foreach (Vector2Int parcel in scenes)
                {
                    if (coords.x == parcel.x && coords.y == parcel.y)
                        return scene;
                }
            }
            return null;
        }
        
        internal Scene GetDeployedSceneFromParcel(IParcelScene sceneToCheck)
        {
            List<Scene> allDeployedScenesWithAccess = DataStore.i.builderInWorld.landsWithAccess.Get().SelectMany(land => land.scenes).ToList();
            foreach (Scene scene in allDeployedScenesWithAccess)
            {
                List<Vector2Int> scenes = scene.parcels.ToList();
                foreach (Vector2Int parcel in scenes)
                {
                    if (sceneToCheck.sceneData.parcels.Any(currentParcel => currentParcel.x == parcel.x && currentParcel.y == parcel.y))
                        return scene;
                }
            }
            return null;
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
            });

            DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

            portableExperiencesToResume = DataStore.i.experiencesViewer.activeExperience.Get();
            WebInterface.SetDisabledPortableExperiences(DataStore.i.experiencesViewer.activeExperience.Get().ToArray());
            
            context.editor.EnterEditMode(sceneToEdit);
            DataStore.i.player.canPlayerMove.Set(false);
            BIWAnalytics.EnterEditor( Time.realtimeSinceStartup - beginStartFlowTimeStamp);
        }

        internal void ExitEditMode()
        {
            currentState = State.IDLE;
            DataStore.i.HUDs.loadingHUD.visible.Set(true);
            initialLoadingController.Hide(true);
            inputController.inputTypeMode = InputTypeMode.GENERAL;
            CommonScriptableObjects.allUIHidden.Set(false);
            context.cameraController.DeactivateCamera();
            context.editor.ExitEditMode();

            builderInWorldBridge.StopIsolatedMode();

            Utils.UnlockCursor();
            
            DataStore.i.player.canPlayerMove.Set(true);
            DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        }

        public void StartFlowFromLandWithPermission(IParcelScene targetScene, string source)
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

            Scene deployedScene = GetDeployedSceneFromParcel(targetScene);
            Vector2Int parcelSize = BIWUtils.GetSceneSize(targetScene);

            StartFromLand(targetScene.sceneData.basePosition, deployedScene, parcelSize, source);
        }

        private void StartFromLand(Vector2Int landCoordsVector, Scene deployedScene, Vector2Int parcelSize, string source)
        {
            string landCoords = landCoordsVector.x + "," + landCoordsVector.y;
            Promise<InitialStateResponse> manifestPromise = initialStateManager.GetInitialManifest(context.builderAPIController, landCoords, deployedScene, parcelSize);

            manifestPromise.Then(response =>
            {
                BuilderScene builderScene = new BuilderScene(response.manifest, IBuilderScene.SceneType.LAND, response.hasBeenCreated);
                builderScene.landCoordsAsociated = landCoordsVector;
                StartFlow(builderScene, source);
            });

            manifestPromise.Catch( error =>
            {
                BIWUtils.ShowGenericNotification(error);
                ExitEditMode();
            });
        }

        private void LoadScene()
        {
            Environment.i.platform.cullingController.Stop();

            // In this point we're sure that the catalog loading (the first half of our progress bar) has already finished
            initialLoadingController.SetPercentage(50f);
            Environment.i.world.sceneController.OnNewSceneAdded += NewSceneAdded;
            Environment.i.world.sceneController.OnReadyScene += NewSceneReady;
            Environment.i.world.blockersController.SetEnabled(false);

            ILand land = BIWUtils.CreateILandFromManifest(sceneToEdit.manifest, DataStore.i.player.playerGridPosition.Get());

            builderInWorldBridge.StartIsolatedMode(land);
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