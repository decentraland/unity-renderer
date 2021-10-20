﻿using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine.Networking;

namespace DCL
{
    public interface IWrappedTextureHelper
    {
        IEnumerator GetHeader(string url, string headerField, Action<string> OnSuccess, Action<string> OnFail);
        IEnumerator Fetch(string url, Action<IPromiseLike_TextureAsset> OnSuccess, Action OnFail = null);
        IEnumerator Fetch(string contentType, string url, Action<IPromiseLike_TextureAsset> OnSuccess, Action OnFail = null);
    }

    public class WrappedTextureUtils : IWrappedTextureHelper
    {
        public IEnumerator GetHeader(string url, string headerField, Action<string> OnSuccess, Action<string> OnFail)
        {
            using (var headReq = UnityWebRequest.Head(url))
            {
                yield return headReq.SendWebRequest();

                if (headReq.WebRequestSucceded())
                {
                    OnSuccess?.Invoke(headReq.GetResponseHeader(headerField));
                }
                else
                {
                    OnFail?.Invoke(headReq.error);
                }
            }
        }

        public IEnumerator Fetch(string url, Action<IPromiseLike_TextureAsset> OnSuccess, Action OnFail = null)
        {
            string contentType = null;
            yield return GetHeader(url, "Content-Type", type => contentType = type, null);
            yield return Fetch(contentType, url, OnSuccess, OnFail);
        }

        public IEnumerator Fetch(string contentType, string url, Action<IPromiseLike_TextureAsset> OnSuccess, Action OnFail = null)
        {
            if (contentType == "image/gif")
            {
                AssetPromise_Gif gifPromise = new AssetPromise_Gif(url);
                gifPromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(new PromiseLike_Gif(gifPromise)); };
                gifPromise.OnFailEvent += (x) => OnFail?.Invoke();

                AssetPromiseKeeper_Gif.i.Keep(gifPromise);

                yield return gifPromise;
            }
            else
            {
                AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
                texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(new PromiseLike_Texture(texturePromise)); };
                texturePromise.OnFailEvent += (x) => OnFail?.Invoke();

                AssetPromiseKeeper_Texture.i.Keep(texturePromise);

                yield return texturePromise;
            }
        }
    }
}