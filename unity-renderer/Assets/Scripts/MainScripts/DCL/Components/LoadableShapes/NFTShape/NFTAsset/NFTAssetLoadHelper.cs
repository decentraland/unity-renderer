﻿using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using NFTShape_Internal;
using UnityEngine.Networking;

namespace DCL
{
    public interface INFTAssetLoadHelper : IDisposable
    {
        IEnumerator LoadNFTAsset(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail);
    }

    public class NFTAssetLoadHelper : INFTAssetLoadHelper
    {
        private AssetPromise_Texture imagePromise = null;
        private AssetPromise_Gif gifPromise = null;

        public IEnumerator LoadNFTAsset(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail)
        {
            if (string.IsNullOrEmpty(url))
            {
                OnFail?.Invoke(new Exception($"Image url is null!"));
                yield break;
            }

            HashSet<string> headers = new HashSet<string>() {"Content-Type", "Content-Length"};
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            yield return GetHeaders(url, headers, result => responseHeaders = result, null);

            string contentType = responseHeaders["Content-Type"];
            long contentLength = long.Parse(responseHeaders["Content-Length"]);
            bool isGif = contentType == "image/gif";

            if (isGif)
            {
                yield return FetchGif(url,
                    OnSuccess: (promise) =>
                    {
                        UnloadPromises();
                        this.gifPromise = promise;
                        INFTAsset nftAsset = NFTAssetFactory.CreateGifAsset(promise.asset);
                        OnSuccess?.Invoke(nftAsset);
                    },
                    OnFail: (exception) => { OnFail?.Invoke(exception); }
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
                    UnloadPromises();
                    this.imagePromise = promise;
                    INFTAsset nftAsset = NFTAssetFactory.CreateImageAsset(promise.asset);
                    OnSuccess?.Invoke(nftAsset);
                },
                OnFail: (exc) => { OnFail?.Invoke(exc); });
        }

        public void Dispose()
        {
            UnloadPromises();
        }

        private IEnumerator GetHeaders(string url, HashSet<string> headerField,
            Action<Dictionary<string, string>> OnSuccess, Action<string> OnFail)
        {
            using (var request = UnityWebRequest.Head(url))
            {
                yield return request.SendWebRequest();

                if (request.WebRequestSucceded())
                {
                    var result = new Dictionary<string, string>();

                    foreach (var key in headerField)
                    {
                        result.Add(key, request.GetResponseHeader(key));
                    }

                    OnSuccess?.Invoke(result);
                }
                else
                {
                    OnFail?.Invoke(request.error);
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

        void UnloadPromises()
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