using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DCL.Builder.Manifest;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private const string BIGGER_LAND_TO_PUBLISH_TEXT = "Your scene is bigger than any Land you own.\nBrowse the Marketplace and buy some new Land where you can deploy.";
        private const string NO_LAND_TO_PUBLISH_TEXT = "To publish a scene first you need a Land where you can deploy it. Browse the marketplace to get some and show the world what you have created.";
        public const string UNPUBLISH_EMPTY_SCENE_NAME = "Empty";
        
        public enum TYPE
        {
            PUBLISH = 0,
            UNPUBLISH = 1
        }
        public event Action<bool> OnPublishFinish;
        
        private IPublishProjectController projectPublisher;
        private ILandPublisherController landPublisher;
        private IPublishProgressController progressController;

        private IContext context;

        private BuilderInWorldBridge builderInWorldBridge;
        private float startPublishingTimestamp = 0;

        private IBuilderScene builderSceneToDeploy;
        private PublishInfo publishInfo;
        private IBuilderScene.SceneType lastTypeWhoStartedPublish;
        private TYPE type = TYPE.PUBLISH;

        private float rotationToApply = 0f;
        
        public void Initialize(IContext context)
        {
            this.context = context;

            projectPublisher = new PublishProjectController();
            landPublisher = new LandPublisherController();
            progressController = new PublishProgressController();

            landPublisher.Initialize();
            projectPublisher.Initialize();
            progressController.Initialize();

            landPublisher.OnPublishPressed += PublishLandScene;
            projectPublisher.OnPublishPressed += ConfirmDeployment;
            
            landPublisher.OnPublishCancel += Canceled;
            projectPublisher.OnPublishCancel += Canceled;
            
            progressController.OnConfirm += StartDeployment;
            progressController.OnBackPressed += BackToPublishInfo;

            builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();

            if (builderInWorldBridge != null)
                builderInWorldBridge.OnPublishEnd += PublishEnd;
        }

        public void Dispose()
        {
            landPublisher.OnPublishPressed -= PublishLandScene;
            projectPublisher.OnPublishPressed -= ConfirmDeployment;
            
            landPublisher.OnPublishCancel -= Canceled;
            projectPublisher.OnPublishCancel -= Canceled;
            
            progressController.OnConfirm -= StartDeployment;
            progressController.OnBackPressed -= BackToPublishInfo;

            if (builderInWorldBridge != null)
                builderInWorldBridge.OnPublishEnd -= PublishEnd;

            projectPublisher.Dispose();
            landPublisher.Dispose();
            progressController.Dispose();
        }

        private void BackToPublishInfo()
        {
            switch (lastTypeWhoStartedPublish)
            {
                case IBuilderScene.SceneType.PROJECT:
                    projectPublisher.SetActive(true);
                    break;
                case IBuilderScene.SceneType.LAND:
                    landPublisher.SetActive(true);
                    break;
            }
        }

        public void StartPublish(IBuilderScene scene)
        {
            if (!HasLands())
            {
                context.commonHUD.GetPopUp()
                       .ShowPopUpWithoutTitle(NO_LAND_TO_PUBLISH_TEXT, "BUY LAND", "BACK", () =>
                       {
                           WebInterface.OpenURL(BIWSettings.MARKETPLACE_URL);
                       }, null);
                return;
            }

            if (!CanPublishInLands(scene))
            {
                context.commonHUD.GetPopUp()
                       .ShowPopUpWithoutTitle(BIGGER_LAND_TO_PUBLISH_TEXT, "BUY LAND", "BACK", () =>
                       {
                           WebInterface.OpenURL(BIWSettings.MARKETPLACE_URL);
                       }, null);
                return;
            }
            
            DataStore.i.builderInWorld.areShortcutsBlocked.Set(true);
            type = TYPE.PUBLISH;
            
            switch (scene.sceneType)
            {
                case IBuilderScene.SceneType.PROJECT:
                    projectPublisher.StartPublishFlow(scene);
                    break;
                case IBuilderScene.SceneType.LAND:
                    landPublisher.StartPublishFlow(scene);
                    break;
            }

            lastTypeWhoStartedPublish = scene.sceneType;
        }

        public async void Unpublish(Vector2Int coords, Vector2Int size)
        {
            type = TYPE.UNPUBLISH;
            
            var manifest = BIWUtils.CreateEmptyDefaultBuilderManifest(size, BIWUtils.Vector2INTToString(coords));
            
            CatalystSceneEntityMetadata sceneJson = new CatalystSceneEntityMetadata();

            //Display info
            sceneJson.display = new CatalystSceneEntityMetadata.Display();
            sceneJson.display.title = UNPUBLISH_EMPTY_SCENE_NAME;

            //Scenes
            sceneJson.scene = new CatalystSceneEntityMetadata.Scene();

            //  Base Parcels 
            string baseParcels = BIWUtils.Vector2INTToString(coords);
            sceneJson.scene.@base = baseParcels;

            //  All parcels 
            string[] parcels = new string[size.x*size.y];
            int count = 0;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    parcels[count] = (coords.x + x) + "," + (coords.y + y);
                    count++;
                }
            }

            sceneJson.scene.parcels = parcels;

            //Main
            sceneJson.main = BIWSettings.DEPLOYMENT_BUNDLED_GAME_FILE;

            //Source
            sceneJson.source = new CatalystSceneEntityMetadata.Source();
            sceneJson.source.origin = BIWSettings.DEPLOYMENT_SOURCE_TYPE;
            sceneJson.source.version = 1;
            sceneJson.source.layout = new CatalystSceneEntityMetadata.Source.Layout();
            sceneJson.source.layout.rows = size.x;
            sceneJson.source.layout.cols = size.y;
            sceneJson.source.point = new CatalystSceneEntityMetadata.Vector2IntRepresentantion(coords);
            sceneJson.source.isEmpty = true;
            sceneJson.source.isEmpty = true;

            // Prepare the assets
            List<SceneObject> assets = manifest.scene.assets.Values.ToList();

            // Download the assets files
            Dictionary<string, object> downloadedFiles = await DownloadAssetFiles(assets);
            
            // This files are not encoded
            Dictionary<string, object> entityFiles = new Dictionary<string, object>
            {
                { BIWSettings.DEPLOYMENT_SCENE_FILE, sceneJson },
                { BIWSettings.DEPLOYMENT_ASSETS, assets },
            };
            
            // Prepare the stateless manifest
            StatelessManifest statelessManifest = ManifestTranslator.WebBuilderSceneToStatelessManifest(manifest.scene);
            
            // Sent scene to kernel
            StartUnpublishScene(downloadedFiles,entityFiles,statelessManifest, sceneJson);
        }

        internal void Canceled()
        {
            DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);
        }

        internal bool HasLands() { return DataStore.i.builderInWorld.landsWithAccess.Get().Length > 0; }

        internal bool CanPublishInLands(IBuilderScene scene)
        {
            List<Vector2Int> availableLandsToPublish = BIWUtils.GetLandsToPublishProject(DataStore.i.builderInWorld.landsWithAccess.Get(), scene);
            return availableLandsToPublish.Count > 0;
        }

        internal void PublishLandScene(IBuilderScene scene)
        {
            PublishInfo publishInfo = new PublishInfo();
            publishInfo.rotation = PublishInfo.ProjectRotation.NORTH;
            publishInfo.coordsToPublish = scene.landCoordsAsociated;
            ConfirmDeployment(scene, publishInfo);
        }

        internal void ConfirmDeployment(IBuilderScene scene, PublishInfo info)
        {
            builderSceneToDeploy = scene;
            publishInfo = info;
            progressController.SetInfoToPublish(scene, info);
            progressController.ShowConfirmDeploy();
        }

        internal void StartDeployment() { DeployScene(builderSceneToDeploy, publishInfo); }

        internal void ApplyRotation(IBuilderScene scene, PublishInfo.ProjectRotation rotation)
        {
            rotationToApply = 90f * (int)rotation;
            RotationScene(scene, rotationToApply);
        }

        internal void UndoRotationToScene(IBuilderScene scene)
        {
            RotationScene(scene, -rotationToApply);
        }

        internal void RotationScene(IBuilderScene scene, float amount)
        {
            // Note: if the aerialscreenshot is not available, this means that the scene has been deployed
            // from the panel instead of the editor, so we don't have the scene to rotate, we publish it to north directly
            if (scene.aerialScreenshotTexture == null)
                return;
            
            var transform = scene.scene.GetSceneTransform();
            Vector3 middlePoint = BIWUtils.CalculateUnityMiddlePoint(scene.scene);

            transform.RotateAround(middlePoint,Vector3.up,amount);
            scene.UpdateManifestFromScene();
        }

        internal async void DeployScene(IBuilderScene scene, PublishInfo info)
        {
            try
            {
                // We assign the scene to deploy
                builderSceneToDeploy = scene;

                // Prepare the thumbnail
                byte[] thumbnail = scene.sceneScreenshotTexture.EncodeToPNG();

                // Prepare the assets
                List<SceneObject> assets = scene.manifest.scene.assets.Values.ToList();

                // Download the assets files
                Dictionary<string, object> downloadedFiles = await DownloadAssetFiles(assets);

                // Prepare scene.json
                CatalystSceneEntityMetadata sceneJson = CreateSceneJson(scene, info);

                // We apply the rotation of the scene so the entities are rotated
                ApplyRotation(scene, info.rotation);
                
                // Group all entities files
                StatelessManifest statelessManifest = ManifestTranslator.WebBuilderSceneToStatelessManifest(scene.manifest.scene);

                // We undo the rotation of the scene
                UndoRotationToScene(scene);
                
                // This files are not encoded
                Dictionary<string, object> entityFiles = new Dictionary<string, object>
                {
                    { BIWSettings.DEPLOYMENT_DEFINITION_FILE, statelessManifest },
                    { BIWSettings.DEPLOYMENT_SCENE_FILE, sceneJson },
                    { BIWSettings.DEPLOYMENT_ASSETS, assets },
                };

                // This file will be encoded automatically
                Dictionary<string, object> entityFilesToDecode = new Dictionary<string, object>
                {
                    { BIWSettings.DEPLOYMENT_SCENE_THUMBNAIL, thumbnail },
                };

                foreach (var downloadedFile in downloadedFiles)
                    entityFilesToDecode.Add(downloadedFile.Key, downloadedFile.Value);

                // Sent scene to kernel
                StartPublishScene(scene, entityFilesToDecode, entityFiles, sceneJson, statelessManifest);
            }
            catch (Exception e)
            {
                // If there is a problem while are preparing the files we end the publishing with an error
                PublishEnd(false, e.Message);
            }
        }

        private void StartUnpublishScene(Dictionary<string, object > filesToDecode, Dictionary<string, object > files,StatelessManifest statelessManifest, CatalystSceneEntityMetadata metadata)
        {
            builderInWorldBridge.PublishScene(filesToDecode, files, metadata, statelessManifest, true);
        }
        
        private void StartPublishScene(IBuilderScene scene, Dictionary<string, object > filesToDecode, Dictionary<string, object > files, CatalystSceneEntityMetadata metadata, StatelessManifest statelessManifest )
        {
            startPublishingTimestamp = Time.realtimeSinceStartup;
            BIWAnalytics.StartScenePublish(scene.scene.metricsCounter.currentCount);
            
            // Note: if the aerialscreenshot is not available, this means that the scene has been deployed
            // from the panel instead of the editor, so we don't have the scene to rotate, we publish it to north directly
            bool publishFromPanel = scene.aerialScreenshotTexture == null && scene.sceneType == IBuilderScene.SceneType.PROJECT;
            
            builderInWorldBridge.PublishScene(filesToDecode, files, metadata, statelessManifest, publishFromPanel);
        }

        private void PublishEnd(bool isOk, string message)
        {
            if (type == TYPE.UNPUBLISH)
            {
                PublishSceneResultPayload payload = new PublishSceneResultPayload();
                payload.ok = isOk;
                payload.error = message;
                DataStore.i.builderInWorld.unpublishSceneResult.Set(payload);
            }
            else
            {
                if (isOk)
                {
                    // We notify the success of the deployment
                    progressController.DeploySuccess();

                    // Remove link to a land if exists
                    builderSceneToDeploy.manifest.project.creation_coords = null;

                    // Update project on the builder server
                    context.builderAPIController.SetManifest(builderSceneToDeploy.manifest);
                }
                else
                {
                    progressController.DeployError(message);
                }
                string successString = isOk ? "Success" : message;
                BIWAnalytics.EndScenePublish(builderSceneToDeploy.scene.metricsCounter.currentCount, successString, Time.realtimeSinceStartup - startPublishingTimestamp);

                if (isOk)
                    builderSceneToDeploy = null;

                DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);

                OnPublishFinish?.Invoke(isOk);
            }
        }

        internal CatalystSceneEntityMetadata CreateSceneJson(IBuilderScene builderScene, PublishInfo info)
        {
            CatalystSceneEntityMetadata sceneJson = new CatalystSceneEntityMetadata();

            //  Display info
            sceneJson.display = new CatalystSceneEntityMetadata.Display();
            sceneJson.display.title = builderScene.manifest.project.title;
            sceneJson.display.description = builderScene.manifest.project.description;
            sceneJson.display.navmapThumbnail = BIWSettings.DEPLOYMENT_SCENE_THUMBNAIL;
            
            //  Contact
            sceneJson.contact = new CatalystSceneEntityMetadata.Contact();
            sceneJson.contact.name =  UserProfile.GetOwnUserProfile().name;
            
            //  Tags
            sceneJson.tags = Array.Empty<string>();
            
            //  Spawn points
            sceneJson.spawnPoints = Array.Empty<CatalystSceneEntityMetadata.SpawnPoint>();

            //  Owner
            sceneJson.owner = UserProfile.GetOwnUserProfile().ethAddress;

            //  Scenes
            sceneJson.scene = new CatalystSceneEntityMetadata.Scene();

            //  Base Parcels 
            string baseParcels = info.coordsToPublish.x + "," + info.coordsToPublish.y;
            sceneJson.scene.@base = baseParcels;

            //  All parcels 
            string[] parcels = new string[builderScene.scene.sceneData.parcels.Length];
            int cont = 0;

            // Size
            Vector2Int sceneSize = BIWUtils.GetSceneSize(builderScene.scene);
            for (int x = 0; x < sceneSize.x; x++)
            {
                for (int y = 0; y < sceneSize.y; y++)
                {
                    parcels[cont] = (info.coordsToPublish.x + x) + "," + (info.coordsToPublish.y + y);
                    cont++;
                }
            }

            sceneJson.scene.parcels = parcels;

            //Main
            sceneJson.main = BIWSettings.DEPLOYMENT_BUNDLED_GAME_FILE;

            //Source
            sceneJson.source = new CatalystSceneEntityMetadata.Source();
            sceneJson.source.origin = BIWSettings.DEPLOYMENT_SOURCE_TYPE;
            sceneJson.source.version = 1;
            sceneJson.source.projectId = builderScene.manifest.project.id;
            sceneJson.source.rotation = info.rotation.ToString().ToLowerInvariant();
            sceneJson.source.layout = new CatalystSceneEntityMetadata.Source.Layout();
            sceneJson.source.layout.rows = builderScene.manifest.project.rows;
            sceneJson.source.layout.cols = builderScene.manifest.project.cols;
            sceneJson.source.point = new CatalystSceneEntityMetadata.Vector2IntRepresentantion(info.coordsToPublish);

            return sceneJson;
        }

        internal async UniTask<Dictionary<string, object>> DownloadAssetFiles(List<SceneObject> assetsToDownload)
        {
            Dictionary<string, object> downloadedAssets = new Dictionary<string, object>();
            List<WebRequestAsyncOperation> asyncOperations = new List<WebRequestAsyncOperation>();

            foreach (SceneObject sceneObject in assetsToDownload)
            {
                //We download each assets needed for the SceneObject
                foreach (KeyValuePair<string, string> assetsContent in sceneObject.contents)
                {
                    string url = sceneObject.GetBaseURL() + assetsContent.Value;
                    var asyncOperation = Environment.i.platform.webRequest.Get(
                        url: url,
                        OnSuccess: (webRequestResult) =>
                        {
                            byte[] byteArray = webRequestResult.GetResultData();

                            downloadedAssets.Add(BIWSettings.DEPLOYMENT_MODELS_FOLDER + "/" + assetsContent.Key, byteArray);
                        },
                        OnFail: (webRequestResult) => { });
                    asyncOperations.Add((WebRequestAsyncOperation)asyncOperation);
                }
            }

            //We wait for all assets
            foreach (WebRequestAsyncOperation operation in asyncOperations)
            {
                await operation;
            }

            return downloadedAssets;
        }
    }
}