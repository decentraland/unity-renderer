using System;
using System.Collections;
using System.Collections.Generic;

public class BaseRefCountedCollection<T> : IEnumerable<KeyValuePair<T, int>>
{
    public event Action<T, int> OnRefCountUpdated;

    internal readonly Dictionary<T, int> dictionary = new Dictionary<T, int>();

    public int this[T key] { get => dictionary[key]; set => dictionary[key] = value; }

    public BaseRefCountedCollection() { }

    public BaseRefCountedCollection(IEnumerable<(T, int)> elements)
    {
        dictionary = new Dictionary<T, int>();
        foreach ((T key, int refCount) in elements)
        {
            dictionary.Add(key, refCount);
        }
    }

    public IEnumerable<KeyValuePair<T, int>> GetAllRefCounts() => dictionary;

    public int GetRefCount(T key)
    {
        if (!dictionary.ContainsKey(key))
            return 0;
        return dictionary[key];
    }

    public void SetRefCounts(IEnumerable<(T, int)> elements)
    {
        dictionary.Clear();
        foreach ((T key, int value) in elements)
        {
            SetRefCount(key, value);
        }
    }

    private void SetRefCount(T key, int count)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key] = count;
        else
            dictionary.Add(key, count);

        OnRefCountUpdated?.Invoke(key, count);
    }

    public void IncreaseRefCount(T key)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key]++;
        else
            dictionary.Add(key, 1);

        OnRefCountUpdated?.Invoke(key, dictionary[key]);
    }

    public void DecreaseRefCount(T key)
    {
        if (!dictionary.ContainsKey(key))
            return;

        int newCount = Math.Max(0, dictionary[key] - 1);
        dictionary[key] = newCount;

        OnRefCountUpdated?.Invoke(key, newCount);
    }

    public void Clear()
    {
        foreach (T key in dictionary.Keys)
        {
            SetRefCount(key, 0);
        }

        dictionary.Clear();
    }

    public int Count() => dictionary.Count;

    public IEnumerator<KeyValuePair<T, int>> GetEnumerator() { return dictionary.GetEnumerator(); }

    IEnumerator IEnumerable.GetEnumerator() { return dictionary.GetEnumerator(); }
}