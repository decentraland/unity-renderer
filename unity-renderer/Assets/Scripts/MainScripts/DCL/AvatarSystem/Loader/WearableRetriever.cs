using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Helpers;
using MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB;
using UnityEngine;

namespace AvatarSystem
{
    public class WearableRetriever : IWearableRetriever
    {
        private const string FEATURE_NEW_ASSET_BUNDLES = "ab-new-cdn";
        public Rendereable rendereable { get; private set; }

        private RendereableAssetLoadHelper loaderAssetHelper;

        public async UniTask<Rendereable> Retrieve(GameObject container, WearableItem wearable, string bodyShapeId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            WearableItem.Representation representation = wearable.GetRepresentation(bodyShapeId);
            if (representation == null)
            {
                Debug.Log($"No representation for body shape {bodyShapeId} of wearable {wearable.id}");
                return null;
            }

            var contentProvider = wearable.GetContentProvider(bodyShapeId);
            string baseUrl = wearable.baseUrlBundles;
            string mainFile = representation.mainFile;

            try
            {
                loaderAssetHelper?.Unload();

                // Before loading the asset, we check if asset bundles exist, then we fill the content provider with it
                if (IsNewAssetBundleFlagEnabled())
                {
                    if (string.IsNullOrEmpty(wearable.entityId))
                        Debug.LogError(mainFile + " has no entity ID, check where this wearable was loaded from");
                    else
                    {
                        var sceneAb = await FetchSceneAssetBundles(wearable.entityId, contentProvider.assetBundlesBaseUrl);

                        if (sceneAb != null && sceneAb.IsSceneConverted())
                        {
                            contentProvider.assetBundles = sceneAb.GetConvertedFiles();
                            contentProvider.assetBundlesBaseUrl = sceneAb.GetBaseUrl();
                            contentProvider.assetBundlesVersion = sceneAb.GetVersion();
                        }
#if UNITY_EDITOR
                        else
                        {
                            Debug.Log($"<color=red>Wearable AB FAILED -> {mainFile} {wearable.entityId} use this ID to reconvert</color>");
                        }
#endif

                        contentProvider.assetBundlesFetched = true;
                    }

                    // even if it we not fetch the asset bundle because the wearable has no ID, we set this to true to avoid fetching it later for the same content provider
                    contentProvider.assetBundlesFetched = true;
                }

                loaderAssetHelper = new RendereableAssetLoadHelper(contentProvider, baseUrl);

                loaderAssetHelper.settings.forceNewInstance = false;
                // TODO Review this hardcoded offset and try to solve it by offseting the Avatar container
                loaderAssetHelper.settings.initialLocalPosition = Vector3.up * AvatarSystemUtils.AVATAR_Y_OFFSET;
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
                loaderAssetHelper.Load(mainFile, AvatarSystemUtils.UseAssetBundles() ? RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK : RendereableAssetLoadHelper.LoadingType.GLTF_ONLY);

                // AttachExternalCancellation is needed because a cancelled WaitUntil UniTask requires a frame
                await UniTaskUtils.WaitForBoolean(ref done, cancellationToken: ct).AttachExternalCancellation(ct);

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

        private bool IsNewAssetBundleFlagEnabled() =>
            DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(FEATURE_NEW_ASSET_BUNDLES);

        private async UniTask<Asset_SceneAB> FetchSceneAssetBundles(string sceneId, string dataBaseUrlBundles)
        {
            AssetPromise_SceneAB promiseSceneAb = new AssetPromise_SceneAB(dataBaseUrlBundles, sceneId);
            AssetPromiseKeeper_SceneAB.i.Keep(promiseSceneAb);
            await promiseSceneAb.ToUniTask();
            return promiseSceneAb.asset;
        }

        public void Dispose() { loaderAssetHelper?.Unload(); }
    }
}
