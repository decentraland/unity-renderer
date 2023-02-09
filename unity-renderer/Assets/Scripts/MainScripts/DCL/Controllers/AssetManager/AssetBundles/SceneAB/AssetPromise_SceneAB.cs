using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB
{
    // this datatype is defined by https://github.com/decentraland/asset-bundle-converter
    [Serializable]
    public class SceneAbDto
    {
        public string version;
        public string[] files;
        public int exitCode;
    }

    public class AssetPromise_SceneAB : AssetPromise_WithUrl<Asset_SceneAB>
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private Service<IWebRequestController> webRequestController;

        private Action onSuccess;

        public AssetPromise_SceneAB(string contentUrl, string hash) : base(contentUrl, hash)
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void OnCancelLoading()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> _)
        {
            onSuccess = OnSuccess;
            AsyncOnLoad().Forget();
        }

        private async UniTaskVoid AsyncOnLoad()
        {
            asset = new Asset_SceneAB
            {
                id = hash,
            };

            var finalUrl = $"{contentUrl}{SceneAssetBundles.MANIFEST_ENDPOINT}{hash}.json";

            try
            {
                var result = await webRequestController.Ref.GetAsync(finalUrl, cancellationToken: cancellationTokenSource.Token);

                if (!string.IsNullOrEmpty(result.error))
                {
                    onSuccess();
                    return;
                }

                string data = result.downloadHandler.text;
                var sceneAb = Utils.SafeFromJson<SceneAbDto>(data);
                asset.Setup(sceneAb, contentUrl);
            }
            catch (OperationCanceledException) { }
            finally { onSuccess(); }
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }
    }
}
