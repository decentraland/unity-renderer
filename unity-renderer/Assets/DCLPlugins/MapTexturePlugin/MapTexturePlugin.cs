using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class MapTexturePlugin : IPlugin
{
    private const int RETRY_TIME = 10;
    private const string MAIN_TEXTURE_URL = "https://api.decentraland.org/v1/minimap.png";
    private const string ESTATES_TEXTURE_URL = "https://api.decentraland.org/v1/estatemap.png";

    private CancellationTokenSource cts;

    private static readonly TextureFormat?[] PRIORITIZED_FORMATS =
    {
        // available for iOS/Android WebGL player
        TextureFormat.ETC2_RGBA8,
        TextureFormat.BC7,
        TextureFormat.DXT5,
        TextureFormat.RGBA32
    };

    public MapTexturePlugin()
    {
        cts = new CancellationTokenSource();

        DataStore.i.HUDs.mapMainTexture.Set(Resources.Load<Texture2D>("MapDefault"));
        DataStore.i.HUDs.mapEstatesTexture.Set(Resources.Load<Texture2D>("MapDefaultEstates"));

        var textureFormat = PRIORITIZED_FORMATS.FirstOrDefault(f => SystemInfo.SupportsTextureFormat(f.Value));

        if (textureFormat == null)
            return;

        DownloadTexture(MAIN_TEXTURE_URL, DataStore.i.HUDs.mapMainTexture, textureFormat.Value, cts.Token);
        DownloadTexture(ESTATES_TEXTURE_URL, DataStore.i.HUDs.mapEstatesTexture, textureFormat.Value, cts.Token);
    }

    private static async UniTaskVoid DownloadTexture(string url, BaseVariable<Texture> textureVariable, TextureFormat textureFormat, CancellationToken ct)
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
                    Texture2D tex = new Texture2D(0, 0, TextureFormat.ETC2_RGBA8, false, true);
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
