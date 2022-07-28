using System;
using System.Collections;
using System.Collections.Generic;

/*
 * `KeyValueSet` works as an alternative of `Dictionary` when a iteration through all it values is performed frequently.
 * CONS: It's much slower to modify the data compared to a `Dictionary`
 * PRO: It's much faster to iterate through all the data compared to a `Dictionary` and
 *      it doesn't alloc extra memory while doing so.
 * Benchmark against a `Dictionary`:
 * Adding element: ~32% slower
 * Removing element: ~164% slower
 * Iterating through all elements: ~34% faster
 */

public readonly struct KeyValueSetPair<TKey, TValue>
{
    public readonly TKey key;
    public readonly TValue value;

    public KeyValueSetPair(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}

public interface IReadOnlyKeyValueSet<TKey, TValue> : IEnumerable<KeyValueSetPair<TKey, TValue>>
{
    TValue this[TKey key] { get; }
    int Count { get; }
    IReadOnlyList<KeyValueSetPair<TKey, TValue>> Pairs { get; }
    bool ContainsKey(TKey key);
    bool TryGetValue(TKey key, out TValue value);
}

public class KeyValueSet<TKey, TValue> : IReadOnlyKeyValueSet<TKey, TValue>
{
    private readonly Dictionary<TKey, int> indexer;
    private readonly List<KeyValueSetPair<TKey, TValue>> pairs;

    public IReadOnlyList<KeyValueSetPair<TKey, TValue>> Pairs => pairs;

    public int Count => pairs.Count;

    public TValue this[TKey key]
    {
        set => AddInternal(key, value, false);
        get
        {
            TryGetValue(key, out TValue value);
            return value;
        }
    }

    public KeyValueSet() : this(0) { }

    public KeyValueSet(int capacity)
    {
        indexer = new Dictionary<TKey, int>(capacity);
        pairs = new List<KeyValueSetPair<TKey, TValue>>(capacity);
    }

    public bool ContainsKey(TKey key)
    {
        return indexer.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!indexer.TryGetValue(key, out int index))
        {
            value = default;
            return false;
        }
        value = pairs[index].value;
        return true;
    }

    public void Add(TKey key, TValue value)
    {
        AddInternal(key, value, true);
    }

    public void RemoveAt(int index)
    {
        int count = Count;
        if (index >= count || index < 0)
        {
            throw new IndexOutOfRangeException();
        }

        TKey key = pairs[index].key;

        if (count > 1 && index < count - 1)
        {
            int lastValueIndex = count - 1;
            pairs[index] = pairs[lastValueIndex];
            indexer[pairs[lastValueIndex].key] = index;
            pairs.RemoveAt(lastValueIndex);
        }
        else
        {
            pairs.RemoveAt(index);
        }

        indexer.Remove(key);
    }

    public bool Remove(TKey key)
    {
        if (!indexer.TryGetValue(key, out int index))
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void Clear()
    {
        indexer.Clear();
        pairs.Clear();
    }

    public IEnumerator<KeyValueSetPair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return pairs[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void AddInternal(TKey key, TValue value, bool add)
    {
        if (!add && indexer.TryGetValue(key, out int index))
        {
            pairs[index] = new KeyValueSetPair<TKey, TValue>(key, value);
            return;
        }
        indexer.Add(key, pairs.Count);
        pairs.Add(new KeyValueSetPair<TKey, TValue>(key, value));
    }
}