using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    public class HintTextureRequest
    {
        public async UniTask<Texture2D> DownloadTexture(string url, CancellationToken ctx, int timeout = 2)
        {
            using UnityWebRequest www = UnityWebRequest.Get(url);

            Texture2D tex = new Texture2D(1, 1, GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, false), TextureCreationFlags.None)
            {
                wrapMode = TextureWrapMode.Clamp,
            };

            if (ctx.IsCancellationRequested)
            {
                return tex;
            }

            www.timeout = timeout;

            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await UniTask.Yield();
                if (ctx.IsCancellationRequested)
                {
                    www.Abort();
                    return tex;
                }
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogException(new Exception(www.error));
                return tex;
            }
            else
            {
                tex.LoadImage(www.downloadHandler.data);
                tex.Apply(false, false);

                return tex;
            }
        }
    }
}
