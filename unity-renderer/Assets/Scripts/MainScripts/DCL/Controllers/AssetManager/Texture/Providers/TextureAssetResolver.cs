using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Texture;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// A higher level abstraction to load a texture from either source and apply additional constraints
    /// </summary>
    public class TextureAssetResolver : AssetResolverBase, ITextureAssetResolver
    {
        private const string PLAIN_BASE64_PROTOCOL = "data:text/plain;base64,";

        private readonly IReadOnlyDictionary<AssetSource, ITextureAssetProvider> providers;
        private readonly BaseVariable<bool> isDebugMode;

        public TextureAssetResolver(IReadOnlyDictionary<AssetSource, ITextureAssetProvider> providers, DataStore_FeatureFlag featureFlags) : base(featureFlags)
        {
            this.providers = providers;
        }

        public async UniTask<TextureResponse> GetTextureAsync(AssetSource permittedSources, string url, int maxTextureSize, bool linear = false, bool useGPUCopy = true,
            CancellationToken cancellationToken = default)
        {
            Exception lastException = null;
            Texture2D texture = null;

            if (!url.StartsWith(PLAIN_BASE64_PROTOCOL))
            {
                using var permittedSourcesRent = GetPermittedProviders(this.providers, permittedSources);
                var permittedProviders = permittedSourcesRent.GetList();

                foreach (var provider in permittedProviders)
                {
                    try
                    {
                        texture = await provider.GetTextureAsync(url, cancellationToken);

                        if (texture)
                        {
                            if (TextureUtils.IsQuestionMarkPNG(texture))
                            {
                                lastException = new Exception("The texture is a question mark");
                                continue;
                            }

                            // The valid texture is retrieved
                            AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Texture {url} loaded from {provider}");
                            break;
                        }

                        AssetResolverLogger.LogVerbose(featureFlags, LogType.Log, $"Texture {url} loaded from {provider} is null");
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
            }
            else
            {
                try { texture = LoadFromBase64(url); }
                catch (Exception e) { lastException = e; }
            }

            if (texture)
            {
                var clampedTexture = TextureHelpers.ClampSize(texture, maxTextureSize, linear: linear, useGPUCopy: useGPUCopy);

                var resizingFactor = Mathf.Min(1,
                    TextureHelpers.GetScalingFactor(texture.width, texture.height, maxTextureSize));

                return new TextureResponse(new TextureSuccessResponse(clampedTexture, resizingFactor));
            }

            lastException ??= new AssetNotFoundException(permittedSources, url);
            return new TextureResponse(new TextureFailResponse(lastException));
        }

        //For Base64 protocols we just take the bytes and create the texture
        //to avoid Unity's web request issue with large URLs
        private Texture2D LoadFromBase64(string url)
        {
            string substring = url.Substring(PLAIN_BASE64_PROTOCOL.Length);
            byte[] decodedTexture = Convert.FromBase64String(substring);
            var texture = new Texture2D(1, 1);
            texture.LoadImage(decodedTexture);
            return texture;
        }
    }
}
