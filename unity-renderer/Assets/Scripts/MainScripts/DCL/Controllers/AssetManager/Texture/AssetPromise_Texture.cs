using System;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_Texture : AssetPromise<Asset_Texture>
    {
        const TextureWrapMode DEFAULT_WRAP_MODE = TextureWrapMode.Clamp;
        const FilterMode DEFAULT_FILTER_MODE = FilterMode.Bilinear;
        private const string PLAIN_BASE64_PROTOCOL = "data:text/plain;base64,";

        string url;
        string idWithTexSettings;
        string idWithDefaultTexSettings;
        TextureWrapMode wrapMode;
        FilterMode filterMode;
        bool storeDefaultTextureInAdvance = false;
        bool storeTexAsNonReadable = false;
        private readonly int maxTextureSize;

        WebRequestAsyncOperation webRequestOp = null;

        public AssetPromise_Texture(string textureUrl, TextureWrapMode textureWrapMode = DEFAULT_WRAP_MODE, FilterMode textureFilterMode = DEFAULT_FILTER_MODE, bool storeDefaultTextureInAdvance = false, bool storeTexAsNonReadable = true, int? overrideMaxTextureSize = null)
        {
            url = textureUrl;
            wrapMode = textureWrapMode;
            filterMode = textureFilterMode;
            this.storeDefaultTextureInAdvance = storeDefaultTextureInAdvance;
            this.storeTexAsNonReadable = storeTexAsNonReadable;
            maxTextureSize = overrideMaxTextureSize ?? DataStore.i.textureSize.generalMaxSize.Get();
            idWithDefaultTexSettings = ConstructId(url, DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE, maxTextureSize);
            idWithTexSettings = UsesDefaultWrapAndFilterMode() ? idWithDefaultTexSettings : ConstructId(url, wrapMode, filterMode, maxTextureSize);
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override object GetLibraryAssetCheckId() { return idWithTexSettings; }

        protected override void OnCancelLoading()
        {
            if (webRequestOp != null)
                webRequestOp.Dispose();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            // Reuse the already-stored default texture, we duplicate it and set the needed config afterwards in AddToLibrary()
            if (library.Contains(idWithDefaultTexSettings) && !UsesDefaultWrapAndFilterMode())
            {
                OnSuccess?.Invoke();
                return;
            }

            if (!url.StartsWith(PLAIN_BASE64_PROTOCOL))
            {
                webRequestOp = DCL.Environment.i.platform.webRequest.GetTexture(
                    url: url,
                    OnSuccess: (webRequestResult) =>
                    {
                        if (asset != null)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(webRequestResult.webRequest);
                            asset.texture = TextureHelpers.ClampSize(texture, maxTextureSize, useGPUCopy: false);
                            asset.resizingFactor = Mathf.Min(1, TextureHelpers.GetScalingFactor(texture.width, texture.height, maxTextureSize));
                            if (TextureUtils.IsQuestionMarkPNG(asset.texture))
                                OnFail?.Invoke(new Exception("The texture is a question mark"));
                            else
                                OnSuccess?.Invoke();
                        }
                        else
                        {
                            OnFail?.Invoke(new Exception("The texture asset is null"));
                        }
                    },
                    OnFail: (webRequestResult) =>
                    {
                        OnFail?.Invoke(new Exception($"Texture promise failed: {webRequestResult?.webRequest?.error}"));
                    });
            }
            else
            {
                //For Base64 protocols we just take the bytes and create the texture
                //to avoid Unity's web request issue with large URLs
                try {
                    string substring = url.Substring(PLAIN_BASE64_PROTOCOL.Length);
                    byte[] decodedTexture = Convert.FromBase64String(substring);
                    asset.texture = new Texture2D(1, 1);
                    asset.texture.LoadImage(decodedTexture);
                    asset.resizingFactor = TextureHelpers.GetScalingFactor(asset.texture.width, asset.texture.height, maxTextureSize);
                    asset.texture = TextureHelpers.ClampSize(asset.texture, maxTextureSize);
                    OnSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    OnFail?.Invoke(e);
                }
                
            }
        }

        protected override bool AddToLibrary()
        {
            if (storeDefaultTextureInAdvance && !UsesDefaultWrapAndFilterMode())
            {
                if (!library.Contains(idWithDefaultTexSettings))
                {
                    // Save default texture asset
                    asset.id = idWithDefaultTexSettings;
                    asset.ConfigureTexture(DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE, false);

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
            asset.ConfigureTexture(wrapMode, filterMode, storeTexAsNonReadable);

            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }
            
            PerformanceAnalytics.PromiseTextureTracker.Track();

            asset = library.Get(asset.id);
            return true;
        }

        string ConstructId(string textureUrl, TextureWrapMode textureWrapMode, FilterMode textureFilterMode, int maxSize) { return $"{((int)textureWrapMode)}{((int)textureFilterMode)}{textureUrl}{maxSize}"; }

        public override object GetId()
        {
            // We only use the id-with-settings when storing/reading from the library
            return idWithDefaultTexSettings;
        }

        public bool UsesDefaultWrapAndFilterMode() { return wrapMode == DEFAULT_WRAP_MODE && filterMode == DEFAULT_FILTER_MODE; }
    }
}