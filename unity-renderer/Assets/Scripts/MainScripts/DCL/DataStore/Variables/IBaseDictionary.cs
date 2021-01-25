using System;
using System.Collections.Generic;

public interface IBaseDictionary<TKey, TValue>
{
    event Action<IEnumerable<KeyValuePair<TKey, TValue>>> OnSet;
    event Action<TKey, TValue> OnAdded;
    event Action<TKey, TValue> OnRemoved;

    TValue this[TKey key] { get; set; }

    IEnumerable<KeyValuePair<TKey, TValue>> Get();
    TValue Get(TKey key);
    bool TryGetValue(TKey key, out TValue value);
    void Set(IEnumerable<(TKey, TValue)> elements);
    void Add(TKey key, TValue value);
    bool Remove(TKey key);
    bool ContainsKey(TKey key);
    bool ContainsValue(TValue value);

    int Count();
}