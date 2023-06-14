using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     The HintTextureRequestHandler class is responsible for downloading textures for loading screen hints.
    /// </summary>
    public class HintTextureRequestHandler : IHintTextureRequestHandler
    {
        public async UniTask<Texture2D> DownloadTexture(string url, CancellationToken ctx, int timeout = 2)
        {
            Texture2D tex = null;
            UnityWebRequest www = null;

            try
            {
                www = UnityWebRequest.Get(url);

                www.timeout = timeout;

                UnityWebRequestAsyncOperation operation = www.SendWebRequest();

                while (!operation.isDone)
                {
                    await UniTask.Yield();

                    if (ctx.IsCancellationRequested)
                    {
                        Debug.LogWarning("Hint DownloadTexture interrupted");
                        www.Abort();
                        return null;
                    }
                }

                if (www.result != UnityWebRequest.Result.Success) { Debug.LogWarning("Hint DownloadTexture Error: " + www.error); }
                else
                {
                    tex = new Texture2D(2, 2, GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, false), TextureCreationFlags.None)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                    };

                    tex.LoadImage(www.downloadHandler.data);
                    tex.Apply(false, false);
                }
            }
            catch (Exception ex) { Debug.LogWarning("DownloadTexture Exception: " + ex.Message); }
            finally
            {
                if (www != null) { www.Dispose(); }
            }

            return tex;
        }
    }
}
