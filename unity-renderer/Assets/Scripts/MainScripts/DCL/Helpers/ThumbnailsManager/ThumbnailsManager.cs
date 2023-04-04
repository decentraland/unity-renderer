using System;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using MainScripts.DCL.Controllers.AssetManager;
using Object = UnityEngine.Object;

//In the future the AssetManager will do this
public static class ThumbnailsManager
{
#if UNITY_EDITOR
    public static bool bypassRequests = false;
#endif
    private static readonly Queue<EnqueuedThumbnail> promiseQueue = new Queue<EnqueuedThumbnail>();
    private static readonly List<AssetPromise_Texture> progressList = new List<AssetPromise_Texture>();
    private static readonly Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();
    private static readonly Dictionary<string, AssetPromise_Texture> promiseCache = new Dictionary<string, AssetPromise_Texture>();
    private const int CONCURRENT_LIMIT = 10;

    public static void Clear()
    {
        foreach (EnqueuedThumbnail thumbnail in promiseQueue)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnail.Promise);
        }
        promiseQueue.Clear();

        foreach (AssetPromise_Texture promiseTexture in progressList)
        {
            AssetPromiseKeeper_Texture.i.Forget(promiseTexture);
        }
        progressList.Clear();

        foreach (KeyValuePair<string,AssetPromise_Texture> assetPromiseTexture in promiseCache)
        {
            AssetPromiseKeeper_Texture.i.Forget(assetPromiseTexture.Value);
        }
        promiseCache.Clear();

        foreach (KeyValuePair<Texture2D,Sprite> sprite in spriteCache)
        {
            Object.DestroyImmediate(sprite.Value);
        }
        spriteCache.Clear();
    }

    public static AssetPromise_Texture PreloadThumbnail(string url)
    {
#if UNITY_EDITOR
        if (bypassRequests)
            return null;
#endif
        if (string.IsNullOrEmpty(url))
            return null;

        var promise = new AssetPromise_Texture(url, permittedSources: AssetSource.ALL);
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

    public static void GetThumbnail(string url, Action<Asset_Texture> OnComplete)
    {
#if UNITY_EDITOR
        if (bypassRequests)
            return;
#endif
        if (string.IsNullOrEmpty(url))
            return;

        if (promiseCache.ContainsKey(url))
        {
            var promise = promiseCache[url];

            if (promise.state == AssetPromiseState.FINISHED)
            {
                OnComplete(promise.asset);
            }
            else
            {
                promise.OnSuccessEvent += OnComplete;
            }

            return;
        }

        var newPromise = new AssetPromise_Texture(url, permittedSources: AssetSource.ALL);
        promiseCache.Add(url, newPromise);

        AddToQueue(new EnqueuedThumbnail(newPromise, OnComplete));
        CheckQueue();
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
            Debug.Log($"Error downloading: {promise.url}, Exception: {error}");
            CheckQueue();
        };

        AssetPromiseKeeper_Texture.i.Keep(promise);
    }

    public static Sprite GetOrCreateSpriteFromTexture(Texture2D texture, out bool wasCreated)
    {
        wasCreated = false;
        if (!spriteCache.ContainsKey(texture))
        {
            spriteCache[texture] =
                Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.one, false);
            wasCreated = true;
        }
        return spriteCache[texture];
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
