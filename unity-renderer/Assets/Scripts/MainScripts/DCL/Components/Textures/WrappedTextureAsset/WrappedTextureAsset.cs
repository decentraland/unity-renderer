using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers.Gif;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public static class WrappedTextureUtils
    {
        public static IEnumerator Fetch(string url, Action<ITexture, AssetPromise_Texture> OnSuccess, Asset_Gif.MaxSize maxTextureSize = Asset_Gif.MaxSize.DONT_RESIZE)
        {
            string contentType = null;

            var headReq = UnityWebRequest.Head(url);

            yield return headReq.SendWebRequest();

            if (headReq.WebRequestSucceded())
            {
                contentType = headReq.GetResponseHeader("Content-Type");
            }

            yield return Create(contentType, url, maxTextureSize, OnSuccess);
        }

        private static IEnumerator Create(string contentType, string url, Asset_Gif.MaxSize maxTextureSize,
                                        Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
        {
            if (contentType != "image/gif")
            {
                AssetPromise_Texture texturePromise = new AssetPromise_Texture(url, storeTexAsNonReadable: false);
                texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texture, texturePromise); };
                texturePromise.OnFailEvent += (x) => OnFail?.Invoke();

                AssetPromiseKeeper_Texture.i.Keep(texturePromise);

                yield return texturePromise;

                yield break;
            }

            var gif = new Asset_Gif(url, maxTextureSize, OnSuccess);

            yield return gif.Load();
        }
    }
}