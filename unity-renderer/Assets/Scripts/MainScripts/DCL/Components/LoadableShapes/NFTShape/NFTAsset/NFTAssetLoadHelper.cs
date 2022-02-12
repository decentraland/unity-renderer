using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using NFTShape_Internal;
using UnityEngine.Networking;

namespace DCL
{
    public interface INftAssetLoadHelper : IDisposable
    {
        IEnumerator Fetch(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail);
    }

    public class NftAssetLoadHelper : INftAssetLoadHelper
    {
        private AssetPromise_Texture imagePromise = null;
        private AssetPromise_Gif gifPromise = null;

        public IEnumerator Fetch(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail)
        {
            if (string.IsNullOrEmpty(url))
            {
                OnFail?.Invoke(new Exception($"Image url is null!"));
                yield break;
            }

            string contentType = null;
            HashSet<string> headers = new HashSet<string>() {"Content-Type", "Content-Length"};
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            yield return GetHeaders(url, headers, result => responseHeaders = result, null);

            contentType = responseHeaders["Content-Type"];
            long contentLength = long.Parse(responseHeaders["Content-Length"]);

            bool isGif = contentType == "image/gif";

            if (isGif)
            {
                yield return FetchGif(url,
                    OnSuccess: (promise) =>
                    {
                        UnloadContents();
                        this.gifPromise = promise;
                        INFTAsset nftAsset = NFTAssetFactory.CreateGifAsset(promise.asset);
                        OnSuccess?.Invoke(nftAsset);
                    },
                    OnFail: (exc) => { OnFail?.Invoke(exc); }
                );

                yield break;
            }

            const long PREVIEW_IMAGE_SIZE_LIMIT = 500000;

            if (contentLength > PREVIEW_IMAGE_SIZE_LIMIT)
            {
                OnFail?.Invoke(new System.Exception($"Image is too big! {contentLength} > {PREVIEW_IMAGE_SIZE_LIMIT}"));
                yield break;
            }

            yield return FetchImage(url,
                OnSuccess: (promise) =>
                {
                    UnloadContents();
                    this.imagePromise = promise;
                    INFTAsset nftAsset = NFTAssetFactory.CreateImageAsset(promise.asset);
                    OnSuccess?.Invoke(nftAsset);
                },
                OnFail: (exc) => { OnFail?.Invoke(exc); });
        }

        public void Dispose()
        {
            UnloadContents();
        }

        private IEnumerator GetHeaders(string url, HashSet<string> headerField,
            Action<Dictionary<string, string>> OnSuccess, Action<string> OnFail)
        {
            using (var headReq = UnityWebRequest.Head(url))
            {
                yield return headReq.SendWebRequest();

                if (headReq.WebRequestSucceded())
                {
                    var result = new Dictionary<string, string>();

                    foreach (var key in headerField)
                    {
                        result.Add(key, headReq.GetResponseHeader(key));
                    }

                    OnSuccess?.Invoke(result);
                }
                else
                {
                    OnFail?.Invoke(headReq.error);
                }
            }
        }

        private IEnumerator FetchGif(string url, Action<AssetPromise_Gif> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Gif gifPromise = new AssetPromise_Gif(url);
            gifPromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(gifPromise); };
            gifPromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Gif.i.Keep(gifPromise);

            yield return gifPromise;
        }

        private IEnumerator FetchImage(string url, Action<AssetPromise_Texture> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
            texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texturePromise); };
            texturePromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Texture.i.Keep(texturePromise);

            yield return texturePromise;
        }

        void UnloadContents()
        {
            if (gifPromise != null)
                AssetPromiseKeeper_Gif.i.Forget(gifPromise);

            if (imagePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(imagePromise);

            gifPromise = null;
            imagePromise = null;
        }
    }
}