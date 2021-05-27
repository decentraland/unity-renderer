using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCache<T> : IDataCache<T>
{
    private class CacheType
    {
        public T value;
        public float lastUpdate;
        public float maxAge;
        public Coroutine expireCacheRoutine;
    }

    private readonly Dictionary<string, CacheType> cache = new Dictionary<string, CacheType>();

    public void Add(string key, T value, float maxAge)
    {
        if (cache.TryGetValue(key, out CacheType storedCache))
        {
            CoroutineStarter.Stop(storedCache.expireCacheRoutine);
        }
        else
        {
            storedCache = new CacheType();
            cache[key] = storedCache;
        }

        storedCache.value = value;
        storedCache.maxAge = maxAge;
        storedCache.lastUpdate = Time.unscaledTime;
        storedCache.expireCacheRoutine = CoroutineStarter.Start(RemoveCache(key, storedCache.maxAge));
    }

    public bool TryGet(string key, out T value, out float lastUpdate)
    {
        value = default(T);
        lastUpdate = 0;

        if (cache.TryGetValue(key, out CacheType storedCache))
        {
            value = storedCache.value;
            lastUpdate = storedCache.lastUpdate;
            return true;
        }

        return false;
    }

    public void Forget(string key)
    {
        if (cache.TryGetValue(key, out CacheType storedCache))
        {
            CoroutineStarter.Stop(storedCache.expireCacheRoutine);
            cache.Remove(key);
        }
    }

    public void Dispose()
    {
        using (var iterator = cache.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                CoroutineStarter.Stop(iterator.Current.Value.expireCacheRoutine);
            }
        }

        cache.Clear();
    }

    private IEnumerator RemoveCache(string key, float delay)
    {
        yield return WaitForSecondsCache.Get(delay);
        cache?.Remove(key);
    }
}