﻿using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using NFTShape_Internal;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public interface INFTAssetLoadHelper : IDisposable
    {
        IEnumerator LoadNFTAsset(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail);
    }

    public class NFTAssetLoadHelper : INFTAssetLoadHelper
    {
        protected AssetPromise_Texture imagePromise = null;
        protected AssetPromise_Gif gifPromise = null;

        public IEnumerator LoadNFTAsset(string url, Action<INFTAsset> OnSuccess, Action<Exception> OnFail)
        {
            if (string.IsNullOrEmpty(url))
            {
                OnFail?.Invoke(new Exception($"Image url is null!"));
                yield break;
            }

            const string CONTENT_TYPE = "Content-Type";
            const string CONTENT_LENGTH = "Content-Length";
            const string CONTENT_TYPE_GIF = "image/gif";
            const long PREVIEW_IMAGE_SIZE_LIMIT = 500000;

            HashSet<string> headers = new HashSet<string>() {CONTENT_TYPE, CONTENT_LENGTH};
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
            string headerRequestError = string.Empty;

            yield return GetHeaders(url, headers, result => responseHeaders = result, (x) => headerRequestError = x);

            if (!string.IsNullOrEmpty(headerRequestError))
            {
                OnFail?.Invoke(new Exception($"Error fetching headers! ({headerRequestError})"));
                yield break;
            }

            string contentType = responseHeaders[CONTENT_TYPE];
            long.TryParse(responseHeaders[CONTENT_LENGTH], out long contentLength);
            bool isGif = contentType == CONTENT_TYPE_GIF;

            if (isGif)
            {
                yield return FetchGif(url,
                    OnSuccess: (promise) =>
                    {
                        UnloadPromises();
                        this.gifPromise = promise;
                        OnSuccess?.Invoke(new NFTAsset_Gif(promise.asset));
                    },
                    OnFail: (exception) => { OnFail?.Invoke(exception); }
                );

                yield break;
            }
            
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
                    OnSuccess?.Invoke(new NFTAsset_Image(promise.asset));
                },
                OnFail: (exc) => { OnFail?.Invoke(exc); });
        }

        public void Dispose()
        {
            UnloadPromises();
        }

        protected virtual IEnumerator GetHeaders(string url, HashSet<string> headerField,
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

        protected virtual IEnumerator FetchGif(string url, Action<AssetPromise_Gif> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Gif gifPromise = new AssetPromise_Gif(url);
            gifPromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(gifPromise); };
            gifPromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Gif.i.Keep(gifPromise);

            yield return gifPromise;
        }

        protected virtual IEnumerator FetchImage(string url, Action<AssetPromise_Texture> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
            texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texturePromise); };
            texturePromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Texture.i.Keep(texturePromise);

            yield return texturePromise;
        }

        protected virtual void UnloadPromises()
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