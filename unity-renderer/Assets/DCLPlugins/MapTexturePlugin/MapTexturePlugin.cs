using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

public class MapTexturePlugin : IPlugin
{
    private readonly IAddressableResourceProvider resourceProvider;

    private const int RETRY_TIME = 10;
    private const string MAIN_TEXTURE_URL = "https://api.decentraland.org/v1/minimap.png";
    private const string ESTATES_TEXTURE_URL = "https://api.decentraland.org/v1/estatemap.png";

    private CancellationTokenSource cts;

    public MapTexturePlugin(IAddressableResourceProvider resourceProvider)
    {
        cts = new CancellationTokenSource();

        this.resourceProvider = resourceProvider;

        DownloadTexture(MAIN_TEXTURE_URL, DataStore.i.HUDs.mapMainTexture, cts.Token, "MapDefault").Forget();
        DownloadTexture(ESTATES_TEXTURE_URL, DataStore.i.HUDs.mapEstatesTexture, cts.Token, "MapDefaultEstates").Forget();
    }

    private async UniTaskVoid DownloadTexture(string url, IBaseVariable<Texture> textureVariable, CancellationToken ct, string textureName)
    {
        var texture = await resourceProvider.GetAddressable<Texture2D>(textureName, ct);
        textureVariable.Set(texture);

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
                    Texture2D tex = new Texture2D(0, 0, GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, false), TextureCreationFlags.None)
                        {
                            filterMode = FilterMode.Point,
                            wrapMode = TextureWrapMode.Clamp,
                        };

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
    }
}
