using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;

namespace AvatarSystem
{
    public class WearableRetriever : IWearableRetriever
    {
        public Rendereable rendereable { get; private set; }

        private RendereableAssetLoadHelper loaderAssetHelper;

        public async UniTask<Rendereable> Retrieve(   GameObject container, ContentProvider contentProvider, string baseUrl, string mainFile, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return null;
            }

            loaderAssetHelper?.Unload();

            loaderAssetHelper = new RendereableAssetLoadHelper(contentProvider, baseUrl);

            loaderAssetHelper.settings.forceNewInstance = false;
            loaderAssetHelper.settings.initialLocalPosition = Vector3.up * 0.75f;
            loaderAssetHelper.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
            loaderAssetHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
            loaderAssetHelper.settings.parent = container.transform;
            loaderAssetHelper.settings.layer = container.layer;

            bool done = false;

            void OnSuccessWrapper(Rendereable rendereable)
            {
                loaderAssetHelper?.ClearEvents();
                done = true;
                this.rendereable = rendereable;
            }

            void OnFailEventWrapper(Exception e)
            {
                //TODO handle exception
                loaderAssetHelper?.ClearEvents();
                done = true;
                rendereable = null;
            }

            loaderAssetHelper.OnSuccessEvent += OnSuccessWrapper;
            loaderAssetHelper.OnFailEvent += OnFailEventWrapper;
            loaderAssetHelper.Load(mainFile, AvatarSystemUtils.UseAssetBundles() ? RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK : RendereableAssetLoadHelper.LoadingType.GLTF_ONLY);
            await UniTask.WaitUntil(() => done, cancellationToken: ct).SuppressCancellationThrow();
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return null;
            }
            return rendereable;
        }

        public void Dispose() { loaderAssetHelper?.Unload(); }
    }
}