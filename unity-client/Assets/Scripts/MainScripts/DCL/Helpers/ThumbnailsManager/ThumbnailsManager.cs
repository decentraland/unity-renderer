using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//In the future the AssetManager will do this
public static class ThumbnailsManager
{
#if UNITY_EDITOR
    public static bool bypassRequests = false;
#endif

    static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
    static Dictionary<string, List<Action<Sprite>>> waitingCallbacks = new Dictionary<string, List<Action<Sprite>>>();

    public static void PreloadThumbnail(string url)
    {
        RequestThumbnail(url, null);
    }

    public static void RequestThumbnail(string url, Action<Sprite> callback)
    {
#if UNITY_EDITOR
        if (bypassRequests) return;
#endif

        if (string.IsNullOrEmpty(url))
            return;

        if (loadedSprites.ContainsKey(url))
        {
            callback?.Invoke(loadedSprites[url]);
            return;
        }

        if (!waitingCallbacks.ContainsKey(url))
        {
            waitingCallbacks.Add(url, new List<Action<Sprite>>());
        }

        if (callback != null)
            waitingCallbacks[url].Add(callback);

        CoroutineStarter.Start(Download(url));
    }

    public static void CancelRequest(string url, Action<Sprite> callback)
    {
        if (!string.IsNullOrEmpty(url) && waitingCallbacks.ContainsKey(url))
        {
            waitingCallbacks[url].Remove(callback);
        }
    }

    private static IEnumerator Download(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        Sprite sprite;
        if (!www.isNetworkError && !www.isHttpError)
        {
            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            texture.Compress(false);
            texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            AddOrUpdateSprite(url, sprite);
        }
        else
        {
            Debug.Log($"Error downloading: {url} {www.error}");
            // No point on making a fancy error because this will be replaced by AssetManager. By now let's use null as fallback value.
            sprite = null;
        }

        if (waitingCallbacks.ContainsKey(url))
        {
            var callbacks = waitingCallbacks[url];
            waitingCallbacks.Remove(url);

            int count = callbacks.Count;
            for (int i = 0; i < count; i++)
            {
                callbacks[i]?.Invoke(sprite);
            }
        }
    }

    private static void AddOrUpdateSprite(string key, Sprite sprite)
    {
        //Alex (╯°□°)╯︵ ┻━┻
        if (loadedSprites.ContainsKey(key))
        {
            loadedSprites[key] = sprite;
        }
        else
        {
            loadedSprites.Add(key, sprite);
        }
    }
}
