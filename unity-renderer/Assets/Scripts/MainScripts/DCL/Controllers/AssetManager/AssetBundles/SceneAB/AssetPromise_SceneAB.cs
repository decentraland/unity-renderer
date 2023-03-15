﻿using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Text.RegularExpressions;
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
        private const string URN_PREFIX = "urn:decentraland:entity:";
        private readonly CancellationTokenSource cancellationTokenSource;
        private Service<IWebRequestController> webRequestController;

        private Action onSuccess;

        public AssetPromise_SceneAB(string contentUrl, string sceneId) : base(contentUrl, sceneId)
        {
            cancellationTokenSource = new CancellationTokenSource();
            this.hash = GetEntityIdFromSceneId(sceneId);
        }

        private string GetEntityIdFromSceneId(string sceneId)
        {
            // This case happens when loading worlds
            if (sceneId.StartsWith(URN_PREFIX))
            {
                int prefixLength = URN_PREFIX.Length;
                return sceneId.Substring(prefixLength, sceneId.IndexOf("?", StringComparison.Ordinal) - prefixLength);
            }

            return sceneId;
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

            var finalUrl = $"{contentUrl}manifest/{hash}.json";

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
            catch (Exception)
            {
                if (!IsEmptyScene())
                    Debug.LogError("No Asset Bundles for scene " + finalUrl);
            }
            finally { onSuccess(); }
        }

        private bool IsEmptyScene() => hash.Contains(",");

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }
    }
}
