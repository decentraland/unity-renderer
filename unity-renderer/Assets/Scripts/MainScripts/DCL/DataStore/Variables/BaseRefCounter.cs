using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRefCounter<TKey> : IEnumerable<KeyValuePair<TKey, int>>
{
    public event Action<TKey> OnAdded;
    public event Action<TKey> OnRemoved;

    internal readonly Dictionary<TKey, int> dictionary = new Dictionary<TKey, int>();

    public int this[TKey key] { get => dictionary[key]; set => dictionary[key] = value; }

    public IEnumerable<KeyValuePair<TKey, int>> Get()
    {
        return dictionary;
    }

    public int Get(TKey key)
    {
        if ( !dictionary.ContainsKey(key))
            return 0;

        return dictionary[key];
    }

    /// <summary>
    /// Adds one to the reference count for the given key. 
    /// </summary>
    /// <param name="key">A given key to be ref counted.</param>
    public void AddRefCount(TKey key)
    {
        if ( key == null )
            return;

        if ( !dictionary.ContainsKey(key) )
        {
            dictionary.Add(key, 0);
            OnAdded?.Invoke(key);
        }

        dictionary[key]++;
    }

    /// <summary>
    /// Adds one to the reference count for the given key. 
    /// </summary>
    /// <param name="key">A given key to be ref counted.</param>
    public void AddRefCount(IEnumerable<TKey> keys)
    {
        foreach ( TKey key in keys )
        {
            AddRefCount(key);
        }
    }

    /// <summary>
    /// Adds one to the reference count for the given keys. 
    /// </summary>
    /// <param name="keys">A given set of keys to be ref counted.</param>
    public void RemoveRefCount(IEnumerable<TKey> keys)
    {
        foreach ( TKey key in keys )
        {
            RemoveRefCount(key);
        }
    }

    /// <summary>
    /// Removes the reference count for the given key. 
    /// </summary>
    /// <param name="key">A given key to be ref counted.</param>
    /// <returns>true if the element count reached zero and was removed.</returns>
    public bool RemoveRefCount(TKey key)
    {
        if ( key == null || !dictionary.ContainsKey(key) )
            return false;

        dictionary[key]--;

        if ( dictionary[key] != 0 )
            return false;

        dictionary.Remove(key);
        OnRemoved?.Invoke(key);
        return true;
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public int Count()
    {
        return dictionary.Count;
    }

    public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }
}