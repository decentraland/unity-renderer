using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Font;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class FontAssetResolver : AssetResolverBase, IFontAssetResolver
    {
        private readonly IReadOnlyDictionary<AssetSource, IFontAssetProvider> providers;

        public FontAssetResolver(IReadOnlyDictionary<AssetSource, IFontAssetProvider> providers, DataStore_FeatureFlag featureFlags)
            : base(featureFlags)
        {
            this.providers = providers;
        }

        public async UniTask<FontResponse> GetFontAsync(AssetSource permittedSources, string url, CancellationToken cancellationToken = default)
        {
            Exception lastException = null;
            TMP_FontAsset font = null;

            using PoolUtils.ListPoolRent<IFontAssetProvider> permittedSourcesRent = GetPermittedProviders(providers, permittedSources);
            List<IFontAssetProvider> permittedProviders = permittedSourcesRent.GetList();

            foreach (IFontAssetProvider provider in permittedProviders)
            {
                try
                {
                    font = await provider.GetFontAsync(url, cancellationToken);

                    if (font)
                    {
                        // The valid font is retrieved
                        AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Font {url} loaded from {provider}");
                        break;
                    }

                    AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Font {url} loaded from {provider} is null");
                }
                catch (Exception e)
                {
                    lastException = e;

                    AssetResolverLogger.LogVerbose(featureFlags, e);

                    // Propagate `OperationCanceledException` further as there is no reason to iterate
                    if (e is OperationCanceledException)
                        break;
                }
            }

            if (font)
                return new FontResponse(new FontSuccessResponse(font));

            lastException ??= new AssetNotFoundException(permittedSources, url);
            return new FontResponse(new FontFailResponse(lastException));
        }
    }
}
