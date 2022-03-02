using System;
using System.Collections;
using DCL.Helpers;
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

        WebRequestAsyncOperation webRequestOp = null;

        public AssetPromise_Texture(string textureUrl, TextureWrapMode textureWrapMode = DEFAULT_WRAP_MODE, FilterMode textureFilterMode = DEFAULT_FILTER_MODE, bool storeDefaultTextureInAdvance = false, bool storeTexAsNonReadable = true)
        {
            url = textureUrl;
            wrapMode = textureWrapMode;
            filterMode = textureFilterMode;
            this.storeDefaultTextureInAdvance = storeDefaultTextureInAdvance;
            this.storeTexAsNonReadable = storeTexAsNonReadable;
            idWithDefaultTexSettings = ConstructId(url, DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE);
            idWithTexSettings = UsesDefaultWrapAndFilterMode() ? idWithDefaultTexSettings : ConstructId(url, wrapMode, filterMode);
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override object GetLibraryAssetCheckId() { return idWithTexSettings; }

        protected override void OnCancelLoading()
        {
            if (webRequestOp != null)
                webRequestOp.Dispose();
            
            if (delayedLoadSuccessCallRoutine != null) 
                CoroutineStarter.Stop(delayedLoadSuccessCallRoutine);
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
                            asset.texture = DownloadHandlerTexture.GetContent(webRequestResult.webRequest);
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
                byte[] decodedTexture = Convert.FromBase64String(url.Substring(PLAIN_BASE64_PROTOCOL.Length));
                asset.texture = new Texture2D(1, 1);
                asset.texture.LoadImage(decodedTexture);
                OnSuccess?.Invoke();
            }
        }

        protected override void OnLoadSuccess()
        {
            if (CanCompressTextureThisFrame())
            {
                // Debug.Log($"AP_Texture - OnLoadSuccess() - libraryProcessingPromise is null, processing: {url} - frame: {Time.frameCount}");
                base.OnLoadSuccess();
            }
            else if (delayedLoadSuccessCallRoutine == null)
            {
                delayedLoadSuccessCallRoutine = CoroutineStarter.Start(DelayLoadSuccessCall());
            }
        }

        private Coroutine delayedLoadSuccessCallRoutine;
        IEnumerator DelayLoadSuccessCall()
        {
            // Debug.Log($"AP_Texture - DelayLoadSuccessCall() 1 - libraryProcessingPromise is occupied, delaying: {url} - frame: {Time.frameCount}");
            yield return new WaitUntil(CanCompressTextureThisFrame);
            // Debug.Log($"AP_Texture - DelayLoadSuccessCall() 2 - libraryProcessingPromise has been freed! now processing: {url} - frame: {Time.frameCount}");
            
            base.OnLoadSuccess();
            
            delayedLoadSuccessCallRoutine = null;
        }

        private bool CanCompressTextureThisFrame()
        {
            return (Time.frameCount - lastTexCompressionFrame) > MIN_FRAMES_FOR_TEX_COMPRESSION;
        }

        private static int lastTexCompressionFrame = 0;
        const int MIN_FRAMES_FOR_TEX_COMPRESSION = 10;
        protected override bool AddToLibrary()
        {   
            if (storeDefaultTextureInAdvance && !UsesDefaultWrapAndFilterMode())
            {
                if (!library.Contains(idWithDefaultTexSettings))
                {
                    // Save default texture asset
                    asset.id = idWithDefaultTexSettings;
                    asset.ConfigureTexture(DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE, false);
                    lastTexCompressionFrame = Time.frameCount;

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
            lastTexCompressionFrame = Time.frameCount;

            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        string ConstructId(string textureUrl, TextureWrapMode textureWrapMode, FilterMode textureFilterMode) { return ((int)textureWrapMode).ToString() + ((int)textureFilterMode).ToString() + textureUrl; }

        public override object GetId()
        {
            // We only use the id-with-settings when storing/reading from the library
            return idWithDefaultTexSettings;
        }

        public bool UsesDefaultWrapAndFilterMode() { return wrapMode == DEFAULT_WRAP_MODE && filterMode == DEFAULT_FILTER_MODE; }
    }
}