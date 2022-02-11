using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine.Networking;

namespace DCL
{
    public interface IWrappedTextureHelper
    {
        IEnumerator GetHeaders(string url, HashSet<string> headerField, Action<Dictionary<string, string>> OnSuccess,
            Action<string> OnFail);

        IEnumerator FetchGif(string url, Action<AssetPromise_Gif> OnSuccess,
            Action<Exception> OnFail = null);

        IEnumerator FetchImage(string url, Action<AssetPromise_Texture> OnSuccess,
            Action<Exception> OnFail = null);
    }

    public class WrappedTextureUtils : IWrappedTextureHelper
    {
        public IEnumerator GetHeaders(string url, HashSet<string> headerField,
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

        public IEnumerator FetchGif(string url, Action<AssetPromise_Gif> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Gif gifPromise = new AssetPromise_Gif(url);
            gifPromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(gifPromise); };
            gifPromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Gif.i.Keep(gifPromise);

            yield return gifPromise;
        }

        public IEnumerator FetchImage(string url, Action<AssetPromise_Texture> OnSuccess,
            Action<Exception> OnFail = null)
        {
            AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
            texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texturePromise); };
            texturePromise.OnFailEvent += (x, error) => OnFail?.Invoke(error);

            AssetPromiseKeeper_Texture.i.Keep(texturePromise);

            yield return texturePromise;
        }
    }
}