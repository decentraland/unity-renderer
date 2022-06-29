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

        public async UniTask<Rendereable> Retrieve(GameObject container, ContentProvider contentProvider, string baseUrl, string mainFile, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                loaderAssetHelper?.Unload();

                // TODO: use feature flag after the wearable material bug is fixed
                Func<bool> checkIfGltFastIsEnabled = () => false;
                loaderAssetHelper = new RendereableAssetLoadHelper(contentProvider, baseUrl, checkIfGltFastIsEnabled);

                loaderAssetHelper.settings.forceNewInstance = false;
                // TODO Review this hardcoded offset and try to solve it by offseting the Avatar container
                loaderAssetHelper.settings.initialLocalPosition = Vector3.up * AvatarSystemUtils.AVATAR_Y_OFFSET;
                loaderAssetHelper.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
                loaderAssetHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
                loaderAssetHelper.settings.parent = container.transform;
                loaderAssetHelper.settings.layer = container.layer;

                bool done = false;
                Exception exception = null;

                void OnSuccessWrapper(Rendereable rendereable)
                {
                    loaderAssetHelper?.ClearEvents();
                    done = true;
                    this.rendereable = rendereable;
                }

                void OnFailEventWrapper(Exception e)
                {
                    exception = e;
                    loaderAssetHelper?.ClearEvents();
                    done = true;
                    rendereable = null;
                }

                loaderAssetHelper.OnSuccessEvent += OnSuccessWrapper;
                loaderAssetHelper.OnFailEvent += OnFailEventWrapper;
                loaderAssetHelper.Load(mainFile, AvatarSystemUtils.UseAssetBundles() ? RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK : RendereableAssetLoadHelper.LoadingType.OLD_GLTF);

                // AttachExternalCancellation is needed because a cancelled WaitUntil UniTask requires a frame
                await UniTask.WaitUntil(() => done, cancellationToken: ct).AttachExternalCancellation(ct);

                if (exception != null)
                    throw exception;

                if (rendereable == null)
                    throw new Exception($"Couldnt retrieve Wearable assets at: {mainFile}");

                return rendereable;
            }
            catch (OperationCanceledException e)
            {
                Dispose();
                throw;
            }
        }

        public void Dispose() { loaderAssetHelper?.Unload(); }
    }
}