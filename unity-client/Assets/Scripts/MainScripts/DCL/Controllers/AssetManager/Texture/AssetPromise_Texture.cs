using System;
using UnityEngine;
using System.Collections;
using DCL.Helpers;

namespace DCL
{
    public class AssetPromise_Texture : AssetPromise<Asset_Texture>
    {
        const TextureWrapMode DEFAULT_WRAP_MODE = TextureWrapMode.Clamp;
        const FilterMode DEFAULT_FILTER_MODE = FilterMode.Bilinear;

        string url;
        string idWithTexSettings;
        string idWithDefaultTexSettings;
        TextureWrapMode wrapMode;
        FilterMode filterMode;
        Coroutine loadCoroutine;
        bool storeDefaultTextureInAdvance = false;
        bool storeTexAsNonReadable = false;

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

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected override object GetLibraryAssetCheckId()
        {
            return idWithTexSettings;
        }

        protected override void OnCancelLoading()
        {
            ClearLoadCoroutine();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            ClearLoadCoroutine();

            // Reuse the already-stored default texture, we duplicate it and set the needed config afterwards in AddToLibrary()
            if (library.Contains(idWithDefaultTexSettings) && !UsesDefaultWrapAndFilterMode())
                OnSuccess?.Invoke();
            else
                loadCoroutine = CoroutineStarter.Start(DownloadAndStore(OnSuccess, OnFail));
        }

        IEnumerator DownloadAndStore(Action OnSuccess, Action OnFail)
        {
            if (!string.IsNullOrEmpty(url))
            {
                yield return Utils.FetchTexture(url, (tex) =>
                {
                    if (asset != null)
                    {
                        asset.texture = tex;
                        OnSuccess?.Invoke();
                    }
                    else
                    {
                        OnFail?.Invoke();
                    }
                }, (errorMessage) => OnFail?.Invoke());
            }
            else
            {
                OnFail?.Invoke();
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

            asset = library.Get(asset.id);
            return true;
        }

        string ConstructId(string textureUrl, TextureWrapMode textureWrapMode, FilterMode textureFilterMode)
        {
            return ((int)textureWrapMode).ToString() + ((int)textureFilterMode).ToString() + textureUrl;
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

        void ClearLoadCoroutine()
        {
            if (loadCoroutine != null)
            {
                CoroutineStarter.Stop(loadCoroutine);
                loadCoroutine = null;
            }
        }
    }
}