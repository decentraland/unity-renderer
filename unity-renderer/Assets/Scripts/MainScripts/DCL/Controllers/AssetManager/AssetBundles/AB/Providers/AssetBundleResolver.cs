using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Providers
{
    public class AssetBundleResolver : AssetResolverBase, IAssetBundleResolver
    {
        private readonly IReadOnlyDictionary<AssetSource, IAssetBundleProvider> providers;
        private readonly IAssetBundleProvider editorProvider;

        public AssetBundleResolver(IReadOnlyDictionary<AssetSource, IAssetBundleProvider> providers, IAssetBundleProvider editorProvider, DataStore_FeatureFlag featureFlags)
            : base(featureFlags)
        {
            this.providers = providers;
            this.editorProvider = editorProvider;
        }

        public async UniTask<AssetBundle> GetAssetBundleAsync(AssetSource permittedSources, string contentUrl,
            string hash, CancellationToken cancellationToken = default)
        {
            Exception lastException = null;

            using var permittedProvidersPool = GetPermittedProviders(this.providers, permittedSources);
            var permittedProviders = permittedProvidersPool.GetList();

#if UNITY_EDITOR
            permittedProviders.Insert(0, editorProvider);
#endif

            foreach (var provider in permittedProviders)
            {
                try
                {
                    var assetBundle = await provider.GetAssetBundleAsync(contentUrl, hash, cancellationToken);

                    if (assetBundle)
                    {
                        AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Asset Bundle {hash} loaded from {provider}");
                        return assetBundle;
                    }
                }

                // Propagate `OperationCanceledException` further as there is no reason to iterate
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    AssetResolverLogger.LogVerbose(featureFlags, e);
                    lastException = e;
                }
            }

            lastException ??= new AssetNotFoundException(permittedSources, hash);
            throw lastException;
        }
    }
}
