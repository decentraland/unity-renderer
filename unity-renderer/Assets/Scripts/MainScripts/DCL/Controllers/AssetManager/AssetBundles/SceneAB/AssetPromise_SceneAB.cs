using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB
{
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
        private Action<Exception> onFail;

        public AssetPromise_SceneAB(string contentUrl, string hash) : base(contentUrl, hash)
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void OnCancelLoading()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            onFail = OnFail;
            onSuccess = OnSuccess;

            var kernelConfigPromise = KernelConfig.i.EnsureConfigInitialized();
            kernelConfigPromise.Catch(error => onFail(new Exception(error)));
            kernelConfigPromise.Then(k => AsyncOnLoad(k).Forget());
        }

        private async UniTaskVoid AsyncOnLoad(KernelConfigModel kernelConfigModel)
        {
            var tld = kernelConfigModel.network == "mainnet" ? ".org/" : ".zone/";
            asset = new Asset_SceneAB();
            asset.id = hash;
            var finalUrl = $"{contentUrl}{tld}{SceneAssetBundles.MANIFEST_ENDPOINT}{hash}.json";

            Debug.Log($"[Asset Bundle] {finalUrl}");

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
                asset.Setup(sceneAb, tld);
            }
            catch (OperationCanceledException) { }
            finally
            {
                onSuccess();
            }
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }
    }
}
