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

    static Dictionary<string, Coroutine> downloadingCoroutines = new Dictionary<string, Coroutine>();
    static Dictionary<string, int> spritesUses = new Dictionary<string, int>();
    static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
    static Dictionary<string, List<Action<Sprite>>> waitingCallbacks = new Dictionary<string, List<Action<Sprite>>>();
    static Dictionary<string, AssetPromise_Texture> texturePromises = new Dictionary<string, AssetPromise_Texture>();

    public static void PreloadThumbnail(string url)
    {
        GetThumbnail(url, null);
    }

    public static void GetThumbnail(string url, Action<Sprite> callback)
    {
#if UNITY_EDITOR
        if (bypassRequests) return;
#endif

        if (string.IsNullOrEmpty(url))
            return;

        IncreaseUses(url);
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

        if (!downloadingCoroutines.ContainsKey(url))
        {
            downloadingCoroutines.Add(url, CoroutineStarter.Start(Download(url)));
        }
    }

    public static void ForgetThumbnail(string url, Action<Sprite> callback)
    {
        if (!string.IsNullOrEmpty(url) && waitingCallbacks.ContainsKey(url))
        {
            waitingCallbacks[url].Remove(callback);
            if (waitingCallbacks[url].Count == 0 && downloadingCoroutines.ContainsKey(url))
            {
                CoroutineStarter.Stop(downloadingCoroutines[url]);
                downloadingCoroutines.Remove(url);
            }
        }

        if (texturePromises.ContainsKey(url))
        {
            AssetPromiseKeeper_Texture.i.Forget(texturePromises[url]);
            texturePromises.Remove(url);
        }

        DecreaseUses(url);
    }

    private static IEnumerator Download(string url)
    {
        Sprite sprite = null;

        if (texturePromises.ContainsKey(url))
        {
            AssetPromiseKeeper_Texture.i.Forget(texturePromises[url]);
            texturePromises.Remove(url);
        }

        texturePromises[url] = new AssetPromise_Texture(url);
        texturePromises[url].OnSuccessEvent += (x) =>
        {
            sprite = Sprite.Create(x.texture, new Rect(0, 0, x.texture.width, x.texture.height), Vector2.zero);
            AddOrUpdateSprite(url, sprite);
        };

        texturePromises[url].OnFailEvent += (x) =>
        {
            Debug.Log($"Error downloading: {url}");

            sprite = null;
        };

        AssetPromiseKeeper_Texture.i.Keep(texturePromises[url]);
        yield return texturePromises[url];

        downloadingCoroutines.Remove(url);

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

    private static void IncreaseUses(string url)
    {
        if (!spritesUses.ContainsKey(url))
        {
            spritesUses.Add(url, 0);
        }
        spritesUses[url]++;
    }

    private static void DecreaseUses(string url)
    {
        if (!spritesUses.ContainsKey(url))
        {
            return;
        }

        if (!loadedSprites.ContainsKey(url))
        {
            spritesUses.Remove(url);
            return;
        }

        spritesUses[url]--;

        if (spritesUses[url] <= 0)
        {
            var sprite = loadedSprites[url];
            spritesUses.Remove(url);
            loadedSprites.Remove(url);
            if (sprite != null)
            {
                UnityEngine.Object.Destroy(sprite.texture);
                UnityEngine.Object.Destroy(sprite);
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
