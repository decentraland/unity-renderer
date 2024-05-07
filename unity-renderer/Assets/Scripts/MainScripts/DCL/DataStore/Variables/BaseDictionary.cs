using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BaseDictionary<TKey, TValue> : IBaseDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
{
    public event Action<IEnumerable<KeyValuePair<TKey, TValue>>> OnSet;
    public event Action<TKey, TValue> OnAdded;
    public event Action<TKey, TValue> OnRemoved;

    internal readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public TValue this[TKey key] { get => dictionary[key]; set => dictionary[key] = value; }

    public BaseDictionary() { }

    public BaseDictionary(IEnumerable<(TKey, TValue)> elements)
    {
        dictionary = new Dictionary<TKey, TValue>();
        foreach ((TKey key, TValue value) in elements)
        {
            dictionary.Add(key, value);
        }
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> Get() => dictionary;

    public TValue Get(TKey key) => dictionary[key];

    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

    public void Set(IEnumerable<(TKey, TValue)> elements)
    {
        dictionary.Clear();
        foreach ((TKey key, TValue value) in elements)
        {
            dictionary.Add(key, value);
        }

        OnSet?.Invoke(dictionary);
    }

    public void AddOrSet(TKey key, TValue value)
    {
        if ( !dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;

        OnAdded?.Invoke(key, value);
    }

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
        OnAdded?.Invoke(key, value);
    }

    public bool Remove(TKey key)
    {
        if (!dictionary.ContainsKey(key))
            return false;

        TValue value = dictionary[key];
        dictionary.Remove(key);
        OnRemoved?.Invoke(key, value);
        return true;
    }

    public void Clear()
    {
        dictionary.Clear();
        OnSet?.Invoke(new Dictionary<TKey, TValue>());
    }

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
    public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);

    public int Count() => dictionary.Count;

    public List<TValue> GetValues() => dictionary.Values.ToList();

    public IEnumerable<TKey> GetKeys() =>
        dictionary.Keys;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }
}
