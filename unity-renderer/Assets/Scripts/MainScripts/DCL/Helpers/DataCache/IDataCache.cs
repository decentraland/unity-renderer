using System;

public interface IDataCache<T> : IDisposable
{
    void Add(string key, T value, float maxAge);
    bool TryGet(string key, out T value, out float lastUpdate);
    void Forget(string key);
}