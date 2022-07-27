using System;
using System.Collections.Generic;
using UnityEngine;
using DCL;

//In the future the AssetManager will do this
public static class ThumbnailsManager
{
#if UNITY_EDITOR
    public static bool bypassRequests = false;
#endif

    private static readonly Queue<EnqueuedThumbnail> promiseQueue = new Queue<EnqueuedThumbnail>();
    private static readonly List<AssetPromise_Texture> progressList = new List<AssetPromise_Texture>();
    private static readonly Dictionary<string, AssetPromise_Texture> textureCache = new Dictionary<string, AssetPromise_Texture>();
    private static readonly Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();
    private const int CONCURRENT_LIMIT = 10;

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

        if (textureCache.ContainsKey(url))
        {
            AssetPromise_Texture assetPromiseTexture = textureCache[url];
            OnComplete(assetPromiseTexture.asset);
            return assetPromiseTexture;
        }
        
        var promise = new AssetPromise_Texture(url);
        AddToQueue(new EnqueuedThumbnail(promise, OnComplete));
        CheckQueue();
        return promise;
    }
    private static void CheckQueue()
    {
        var availableSlots = Mathf.Max(CONCURRENT_LIMIT - progressList.Count, 0);

        for (int i = progressList.Count - 1; i >= 0 ; i--)
        {
            if (progressList[i].state == AssetPromiseState.IDLE_AND_EMPTY)
            {
                progressList.RemoveAt(i);
            }
        }
        
        if (availableSlots > 0)
        {
            var availableDownloads = Mathf.Min(availableSlots, promiseQueue.Count);

            if (availableDownloads > 0)
            {
                for (int i = 0; i < availableDownloads; i++)
                {
                    if (promiseQueue.Count == 0)
                        break;
                    
                    var promise = promiseQueue.Dequeue();
                    AssetPromise_Texture assetPromiseTexture = promise.Promise;
                    BeginDownload(assetPromiseTexture, promise.OnComplete);
                }
            }
        }
    }
    private static void AddToQueue(EnqueuedThumbnail enqueuedThumbnail)
    {
        promiseQueue.Enqueue(enqueuedThumbnail);
    }
    private static void BeginDownload(AssetPromise_Texture promise, Action<Asset_Texture> OnComplete)
    {
        progressList.Add(promise);

        promise.OnSuccessEvent += t =>
        {
            progressList.Remove(promise);
            OnComplete(t);
            CheckQueue();
        };

        promise.OnFailEvent += (x, error) =>
        {
            progressList.Remove(promise);
            Debug.Log($"Error downloading: {promise.Url}, Exception: {error}"); 
            CheckQueue();
        };
        
        AssetPromiseKeeper_Texture.i.Keep(promise);
    }

    public static Sprite GetOrCreateSpriteFromTexture(string url)
    {
        return GetOrCreateSpriteFromTexture(textureCache[url].asset.texture);
    }
    
    public static Sprite GetOrCreateSpriteFromTexture(Texture2D texture)
    {
        if (!spriteCache.ContainsKey(texture))
        {
            spriteCache[texture] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        return spriteCache[texture];
    }

    public static bool IsCached(string url)
    {
        return textureCache.ContainsKey(url);
    }
}

public struct EnqueuedThumbnail
{
    public readonly AssetPromise_Texture Promise;
    public readonly Action<Asset_Texture> OnComplete;
    public EnqueuedThumbnail(AssetPromise_Texture promise, Action<Asset_Texture> complete)
    {
        this.Promise = promise;
        OnComplete = complete;
    }
}