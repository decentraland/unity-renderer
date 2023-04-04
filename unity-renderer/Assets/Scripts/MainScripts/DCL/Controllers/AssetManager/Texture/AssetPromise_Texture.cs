using Cysharp.Threading.Tasks;
using System;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using MainScripts.DCL.Controllers.AssetManager;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Texture : AssetPromise<Asset_Texture>
    {
        private const TextureWrapMode DEFAULT_WRAP_MODE = TextureWrapMode.Clamp;
        private const FilterMode DEFAULT_FILTER_MODE = FilterMode.Bilinear;

        private readonly string idWithTexSettings;
        private readonly string idWithDefaultTexSettings;
        private readonly TextureWrapMode wrapMode;
        private readonly FilterMode filterMode;
        private readonly bool storeDefaultTextureInAdvance = false;
        private readonly bool storeTexAsNonReadable = false;
        private readonly bool? overrideCompression;
        private readonly bool linear;
        private readonly bool useGPUCopy;
        private readonly int maxTextureSize;
        private readonly AssetSource permittedSources;

        private CancellationTokenSource cancellationTokenSource;

        private Service<ITextureAssetResolver> textureResolver;

        public string url { get; }

        public AssetPromise_Texture(string textureUrl, TextureWrapMode textureWrapMode = DEFAULT_WRAP_MODE, FilterMode textureFilterMode = DEFAULT_FILTER_MODE,
            bool storeDefaultTextureInAdvance = false, bool storeTexAsNonReadable = true,
            int? overrideMaxTextureSize = null, AssetSource permittedSources = AssetSource.WEB,
            bool? overrideCompression = null, bool linear = false, bool useGPUCopy = false)
        {
            url = textureUrl;
            wrapMode = textureWrapMode;
            filterMode = textureFilterMode;
            this.storeDefaultTextureInAdvance = storeDefaultTextureInAdvance;
            this.storeTexAsNonReadable = storeTexAsNonReadable;
            this.overrideCompression = overrideCompression;
            this.linear = linear;
            this.useGPUCopy = useGPUCopy;
            maxTextureSize = overrideMaxTextureSize ?? DataStore.i.textureConfig.generalMaxSize.Get();
            idWithDefaultTexSettings = ConstructId(url, DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE, maxTextureSize);
            idWithTexSettings = UsesDefaultWrapAndFilterMode() ? idWithDefaultTexSettings : ConstructId(url, wrapMode, filterMode, maxTextureSize);
            this.permittedSources = permittedSources;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override object GetLibraryAssetCheckId()
        {
            return idWithTexSettings;
        }

        protected override void OnCancelLoading()
        {
            cancellationTokenSource?.Cancel();
        }

        protected override void OnLoad(Action onSuccess, Action<Exception> onFail)
        {
            // Reuse the already-stored default texture, we duplicate it and set the needed config afterwards in AddToLibrary()
            if (library.Contains(idWithDefaultTexSettings) && !UsesDefaultWrapAndFilterMode())
            {
                onSuccess?.Invoke();
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            async UniTaskVoid LaunchRequest()
            {
                MainScripts.DCL.Controllers.AssetManager.Texture.TextureResponse result = await textureResolver.Ref.GetTextureAsync(permittedSources, url,
                    maxTextureSize, linear, useGPUCopy, cancellationTokenSource.Token);

                if (result.IsSuccess)
                {
                    var successResponse = result.GetSuccessResponse();
                    asset.texture = successResponse.Texture;
                    asset.resizingFactor = successResponse.ResizingFactor;
                    onSuccess?.Invoke();
                }
                else
                {
                    onFail?.Invoke(result.GetFailResponse().Exception);
                }
            }

            LaunchRequest().Forget();
        }

        protected override bool AddToLibrary()
        {
            if (storeDefaultTextureInAdvance && !UsesDefaultWrapAndFilterMode())
            {
                if (!library.Contains(idWithDefaultTexSettings))
                {
                    // Save default texture asset
                    asset.id = idWithDefaultTexSettings;
                    asset.ConfigureTexture(DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE, overrideCompression, false);

                    if (!library.Add(asset))
                    {
                        Debug.Log("add to library fail?");
                        return false;
                    }
                }

                // By always using library.Get() for the default tex we have stored, we increase its references counter,
                // that will come in handy for removing that default tex when there is no one using it
                var defaultTexAsset = library.Get(idWithDefaultTexSettings);
                asset = defaultTexAsset.Clone() as Asset_Texture;
                asset.dependencyAsset = defaultTexAsset;
                asset.texture = TextureHelpers.CopyTexture(defaultTexAsset.texture);
            }

            asset.id = idWithTexSettings;
            asset.ConfigureTexture(wrapMode, filterMode, overrideCompression, storeTexAsNonReadable);

            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            PerformanceAnalytics.PromiseTextureTracker.Track();

            asset = library.Get(asset.id);
            return true;
        }

        string ConstructId(string textureUrl, TextureWrapMode textureWrapMode, FilterMode textureFilterMode, int maxSize)
        {
            return $"{((int)textureWrapMode)}{((int)textureFilterMode)}{textureUrl}{maxSize}";
        }

        public override object GetId()
        {
            // We only use the id-with-settings when storing/reading from the library
            return idWithDefaultTexSettings;
        }

        public bool UsesDefaultWrapAndFilterMode()
        {
            return wrapMode == DEFAULT_WRAP_MODE && filterMode == DEFAULT_FILTER_MODE;
        }
    }
}
