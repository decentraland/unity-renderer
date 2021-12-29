using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private IPublishProjectController projectPublisher;
        private ILandPublisherController landPublisher;
        private IPublishProgressController progressController;

        private IContext context;

        private BuilderInWorldBridge builderInWorldBridge;
        private float startPublishingTimestamp = 0;

        private IBuilderScene builderSceneToDeploy;
        private PublishInfo publishInfo;

        public void Initialize(IContext context)
        {
            this.context = context;

            projectPublisher = new PublishProjectController();
            landPublisher = new LandPublisherController();
            progressController = new PublishProgressController();

            landPublisher.Initialize();
            projectPublisher.Initialize();
            progressController.Initialize();

            landPublisher.OnPublishPressed += PublishPressedDeployment;
            projectPublisher.OnPublishPressed += ConfirmDeployment;
            progressController.OnConfirm += StartDeployment;

            builderInWorldBridge = context.sceneReferences.biwBridgeGameObject.GetComponent<BuilderInWorldBridge>();

            if (builderInWorldBridge != null)
                builderInWorldBridge.OnPublishEnd += PublishEnd;
        }

        public void Dipose()
        {
            landPublisher.OnPublishPressed -= PublishPressedDeployment;
            projectPublisher.OnPublishPressed -= ConfirmDeployment;
            progressController.OnConfirm -= StartDeployment;

            if (builderInWorldBridge != null)
                builderInWorldBridge.OnPublishEnd -= PublishEnd;

            projectPublisher.Dispose();
            landPublisher.Dispose();
            progressController.Dispose();
        }

        public void StartPublish(IBuilderScene scene)
        {
            switch (scene.sceneType)
            {
                case IBuilderScene.SceneType.PROJECT:
                    projectPublisher.StartPublishFlow(scene);
                    break;
                case IBuilderScene.SceneType.LAND:
                    landPublisher.StartPublishFlow(scene);
                    break;
                default:
                    Debug.Log("This should no appear, the scene should have a know type!");
                    break;
            }
        }

        internal void PublishPressedDeployment(IBuilderScene scene) { ConfirmDeployment(scene, null); }

        internal void ConfirmDeployment(IBuilderScene scene, PublishInfo info)
        {
            builderSceneToDeploy = scene;
            publishInfo = info;
            progressController.ShowConfirmDeploy();
        }

        internal void StartDeployment() { DeployScene(builderSceneToDeploy, publishInfo); }

        internal async void DeployScene(IBuilderScene scene, PublishInfo info)
        {
            builderSceneToDeploy = scene;

            // Prepare the thumbnail
            byte[] base64Thumbnail = scene.sceneScreenshotTexture.EncodeToPNG();

            // Prepare the assets
            List<SceneObject> assets = scene.manifest.scene.assets.Values.ToList();

            // Download the assets files
            Dictionary<string, object> downloadedFiles = await DownloadAssetFiles(assets);

            //Prepare scene.json
            CatalystSceneEntityMetadata sceneJson = CreateSceneJson(scene, info);

            //Group all entities files
            StatelessManifest statelessManifest = ManifestTranslator.ParcelSceneToStatelessManifest(scene.scene);

            Dictionary<string, object> entityFiles = new Dictionary<string, object>
            {
                { BIWSettings.DEPLOYMENT_DEFINITION_FILE, statelessManifest },
                { BIWSettings.DEPLOYMENT_SCENE_FILE, sceneJson },
                { BIWSettings.DEPLOYMENT_ASSETS, assets },
            };

            Dictionary<string, object> entityFilesToDecode = new Dictionary<string, object>
            {
                { BIWSettings.DEPLOYMENT_SCENE_THUMBNAIL, base64Thumbnail },
            };

            foreach (var downloadedFile in downloadedFiles)
                entityFilesToDecode.Add(downloadedFile.Key, downloadedFile.Value);

            //Sent scene to kernel
            StartPublishScene(scene, entityFilesToDecode, entityFiles, sceneJson, statelessManifest);

            //Remove link to a land if exists
            scene.manifest.project.creation_coords = null;

            //Update project on the builder server
            context.builderAPIController.SetManifest(scene.manifest);
        }

        private void StartPublishScene(IBuilderScene scene, Dictionary<string, object > filesToDecode, Dictionary<string, object > files, CatalystSceneEntityMetadata metadata, StatelessManifest statelessManifest )
        {
            startPublishingTimestamp = Time.realtimeSinceStartup;
            BIWAnalytics.StartScenePublish(scene.scene.metricsCounter.GetModel());
            builderInWorldBridge.PublishScene(filesToDecode, files, metadata, statelessManifest);
        }

        private void PublishEnd(bool isOk, string message)
        {
            if (isOk)
                progressController.DeploySuccess(builderSceneToDeploy);
            else
                progressController.DeployError(message);

            string successString = isOk ? "Success" : message;
            BIWAnalytics.EndScenePublish(builderSceneToDeploy.scene.metricsCounter.GetModel(), successString, Time.realtimeSinceStartup - startPublishingTimestamp);
        }

        internal CatalystSceneEntityMetadata CreateSceneJson(IBuilderScene builderScene, PublishInfo info)
        {
            CatalystSceneEntityMetadata sceneJson = new CatalystSceneEntityMetadata();

            //Display info
            sceneJson.display = new CatalystSceneEntityMetadata.Display();
            sceneJson.display.title = builderScene.manifest.project.title;
            sceneJson.display.description = builderScene.manifest.project.description;
            sceneJson.display.description = builderScene.manifest.project.description;
            sceneJson.display.navmapThumbnail = BIWSettings.DEPLOYMENT_SCENE_THUMBNAIL;

            //Owner
            sceneJson.owner = UserProfile.GetOwnUserProfile().ethAddress;

            //Scenes
            sceneJson.scene = new CatalystSceneEntityMetadata.Scene();

            //  Base Parcels 
            string baseParcels = info.landsToPublish[0].baseCoords.x + "," + info.landsToPublish[0].baseCoords.y;
            sceneJson.scene.@base = baseParcels;

            //  All parcels 
            string[] parcels = new string[builderScene.scene.sceneData.parcels.Length];
            int cont = 0;

            foreach (LandWithAccess landWithAccess in info.landsToPublish)
            {
                parcels[cont] = landWithAccess.baseCoords.x + "," + landWithAccess.baseCoords.y;
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
            sceneJson.source.layout.rows = builderScene.manifest.project.rows.ToString();
            sceneJson.source.layout.cols = builderScene.manifest.project.cols.ToString();
            sceneJson.source.point = info.landsToPublish[0].baseCoords;

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