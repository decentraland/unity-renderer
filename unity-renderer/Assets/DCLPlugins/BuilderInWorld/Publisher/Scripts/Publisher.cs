using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private IPublishProjectController projectPublisher;
        private ILandPublisherController landPublisher;

        private IContext context;
        public void Initialize(IContext context)
        {
            this.context = context;

            projectPublisher = new PublishProjectController();
            landPublisher = new LandPublisherController();

            landPublisher.Initialize();
            projectPublisher.Initialize();

            landPublisher.OnConfirm += DeployScene;
            projectPublisher.OnConfirm += DeployScene;
        }

        public void Dipose()
        {
            projectPublisher.Dispose();
            landPublisher.Dispose();
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

        internal async void DeployScene(IBuilderScene scene)
        {
            // Prepare the thumbnail
            byte[] thumbnailBlob = scene.sceneScreenshotTexture.EncodeToPNG();

            // Prepare the assets
            List<SceneObject> assets = scene.manifest.scene.assets.Values.ToList();

            // Download the assets files
            Dictionary<string, byte[]> downloadedFiles = await DownloadAssetFiles(assets);

            // Generate game file

        }

        internal async UniTask<Dictionary<string, byte[]>> DownloadAssetFiles(List<SceneObject> assetsToDownload)
        {
            Dictionary<string, byte[]> downloadedAssets = new Dictionary<string, byte[]>();
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
                            downloadedAssets.Add("models/" + assetsContent.Key, byteArray);
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