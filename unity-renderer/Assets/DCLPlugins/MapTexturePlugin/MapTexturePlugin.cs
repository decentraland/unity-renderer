using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

public class MapTexturePlugin : IPlugin
{
    private const int RETRY_TIME = 10;
    private const string MAIN_TEXTURE_URL = "www.fakeUrlForMainTexture.com";
    private const string ESTATES_TEXTURE_URL = "www.fakeUrlForEstatesTexture.com";

    private readonly AssetPromise_Texture mainTexturePromise;
    private readonly AssetPromise_Texture estateTexturePromise;
    private CancellationTokenSource cts;

    public MapTexturePlugin()
    {
        mainTexturePromise = new AssetPromise_Texture(MAIN_TEXTURE_URL, TextureWrapMode.Clamp, FilterMode.Point);
        estateTexturePromise = new AssetPromise_Texture(ESTATES_TEXTURE_URL, TextureWrapMode.Clamp, FilterMode.Point);
        cts = new CancellationTokenSource();

        DataStore.i.HUDs.mapMainTexture.Set(Resources.Load<Texture2D>("MapDefault"));
        DataStore.i.HUDs.mapEstatesTexture.Set(Resources.Load<Texture2D>("MapDefaultEstates"));

        DownloadTexture(mainTexturePromise, DataStore.i.HUDs.mapMainTexture, OnTextureFail, cts.Token);
        DownloadTexture(estateTexturePromise, DataStore.i.HUDs.mapEstatesTexture, OnTextureFail, cts.Token);
    }

    private static async UniTaskVoid DownloadTexture(AssetPromise_Texture promise, BaseVariable<Texture> textureVariable, Action<Asset_Texture, Exception> OnFail, CancellationToken ct)
    {
        while (true)
        {
            promise.OnFailEvent += OnFail; //We can subscribe on every attempt because promise events are cleaned up no fail
            AssetPromiseKeeper_Texture.i.Keep(promise);

            await promise.WithCancellation(ct).AttachExternalCancellation(ct);

            // We've got the texture.
            if (promise.asset?.texture != null)
            {
                textureVariable.Set(promise.asset.texture);
                return;
            }

            // We retry in 10 seconds
            await UniTask.Delay(TimeSpan.FromSeconds(RETRY_TIME), cancellationToken: ct).AttachExternalCancellation(ct);
        }
    }

    private void OnTextureFail(Asset_Texture _, Exception exception) { Debug.LogException(exception); }

    public void Dispose()
    {
        AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);
        AssetPromiseKeeper_Texture.i.Forget(estateTexturePromise);
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}