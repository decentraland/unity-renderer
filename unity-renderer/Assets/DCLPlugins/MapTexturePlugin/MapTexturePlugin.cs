using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

public class MapTexturePlugin : IPlugin
{
    private const int RETRY_TIME = 10;
    private const string MAIN_TEXTURE_URL = "https://api.decentraland.org/v1/minimap.png";
    private const string ESTATES_TEXTURE_URL = "https://api.decentraland.org/v1/estatemap.png";

    private CancellationTokenSource cts;

    public MapTexturePlugin()
    {
        cts = new CancellationTokenSource();

        DataStore.i.HUDs.mapMainTexture.Set(Resources.Load<Texture2D>("MapDefault"));
        DataStore.i.HUDs.mapEstatesTexture.Set(Resources.Load<Texture2D>("MapDefaultEstates"));

        DownloadTexture(MAIN_TEXTURE_URL, DataStore.i.HUDs.mapMainTexture, cts.Token);
        DownloadTexture(ESTATES_TEXTURE_URL, DataStore.i.HUDs.mapEstatesTexture, cts.Token);
    }

    private static async UniTaskVoid DownloadTexture(string url, BaseVariable<Texture> textureVariable, CancellationToken ct)
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                await www.SendWebRequest();
                if (www.error != null)
                {
                    Debug.LogException(new Exception(www.error));
                }
                else
                {
                    Texture2D tex = new Texture2D(0, 0, GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, false), TextureCreationFlags.None);
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.LoadImage(www.downloadHandler.data);
                    tex.Apply(false, false);
                    textureVariable.Set(tex);
                    return;
                }
            }

            // We retry in 10 seconds
            await UniTask.Delay(TimeSpan.FromSeconds(RETRY_TIME), cancellationToken: ct).AttachExternalCancellation(ct);
        }
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
