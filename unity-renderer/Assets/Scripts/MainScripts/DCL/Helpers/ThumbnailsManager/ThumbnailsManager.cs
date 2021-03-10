using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DCL;

//In the future the AssetManager will do this
public static class ThumbnailsManager
{
#if UNITY_EDITOR
    public static bool bypassRequests = false;
#endif
    static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();

    public static AssetPromise_Texture PreloadThumbnail(string url)
    {
#if UNITY_EDITOR
        if (bypassRequests)
            return null;
#endif
        if (string.IsNullOrEmpty(url))
            return null;

        var promise = new AssetPromise_Texture(url);
        AssetPromiseKeeper_Texture.i.Keep(promise);

        return promise;
    }

    public static void ForgetThumbnail(AssetPromise_Texture promise)
    {
        if (promise == null)
            return;

        //Debug.Log("Forget thumbnail " + promise.asset.id);
        AssetPromiseKeeper_Texture.i.Forget(promise);
    }

    public static AssetPromise_Texture GetThumbnail(string url, Action<Asset_Texture> OnComplete)
    {
#if UNITY_EDITOR
        if (bypassRequests)
            return null;
#endif
        if (string.IsNullOrEmpty(url))
            return null;

        var promise = new AssetPromise_Texture(url);

        promise.OnSuccessEvent += OnComplete;
        promise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(promise);
        return promise;
    }

    public static Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return sprite;
    }
}