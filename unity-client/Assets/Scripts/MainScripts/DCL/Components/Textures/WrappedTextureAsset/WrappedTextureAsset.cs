using System;
using System.Collections;
using DCL.Controllers.Gif;
using DCL.Helpers;
using UnityEngine.Networking;

namespace DCL
{
    public static class WrappedTextureUtils
    {
        public static IEnumerator GetHeader(string url, string headerField, Action<string> OnSuccess, Action<string> OnFail)
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

        public static IEnumerator Fetch(string url, Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
        {
            string contentType = null;
            yield return GetHeader(url, "Content-Type", type => contentType = type, null);
            yield return Fetch(contentType, url, OnSuccess, OnFail);
        }

        public static IEnumerator Fetch(string contentType, string url, Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
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

            var gif = new Asset_Gif(url, OnSuccess);

            yield return gif.Load();
        }
    }
}