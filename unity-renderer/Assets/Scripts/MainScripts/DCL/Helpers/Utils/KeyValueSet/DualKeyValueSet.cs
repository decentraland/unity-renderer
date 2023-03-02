using System;
using System.Collections;
using System.Collections.Generic;

public readonly struct KeyValueSetTriplet<TKey1, TKey2, TValue>
{
    public readonly TKey1 key1;
    public readonly TKey2 key2;
    public readonly TValue value;

    public KeyValueSetTriplet(TKey1 key1, TKey2 key2, TValue value)
    {
        this.key1 = key1;
        this.key2 = key2;
        this.value = value;
    }
}

public interface IReadOnlyDualKeyValueSet<TKey1, TKey2, TValue> : IEnumerable<KeyValueSetTriplet<TKey1, TKey2, TValue>>
{
    TValue this[TKey1 key1, TKey2 key2] { get; }
    int Count { get; }
    IReadOnlyList<KeyValueSetTriplet<TKey1, TKey2, TValue>> Pairs { get; }
    bool ContainsKey(TKey1 key1, TKey2 key2);
    bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value);
}

public class DualKeyValueSet<TKey1, TKey2, TValue> : IReadOnlyDualKeyValueSet<TKey1, TKey2, TValue>
{
    private readonly struct KeyPair
    {
        public readonly TKey1 key1;
        public readonly TKey2 key2;

        public KeyPair(TKey1 key1, TKey2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
    }

    private readonly Dictionary<KeyPair, int> indexer;
    private readonly List<KeyValueSetTriplet<TKey1, TKey2, TValue>> pairs;

    public IReadOnlyList<KeyValueSetTriplet<TKey1, TKey2, TValue>> Pairs => pairs;

    public int Count => pairs.Count;

    public TValue this[TKey1 key1, TKey2 key2]
    {
        set => AddInternal(key1, key2, value, false);
        get
        {
            TryGetValue(key1, key2, out TValue value);
            return value;
        }
    }

    public DualKeyValueSet() : this(0) { }

    public DualKeyValueSet(int capacity)
    {
        indexer = new Dictionary<KeyPair, int>(capacity);
        pairs = new List<KeyValueSetTriplet<TKey1, TKey2, TValue>>(capacity);
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2)
    {
        KeyPair keyPair = new KeyPair(key1, key2);
        return indexer.ContainsKey(keyPair);
    }

    public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
    {
        KeyPair keyPair = new KeyPair(key1, key2);
        if (!indexer.TryGetValue(keyPair, out int index))
        {
            value = default;
            return false;
        }
        value = pairs[index].value;
        return true;
    }

    public void Add(TKey1 key1, TKey2 key2, TValue value)
    {
        AddInternal(key1, key2, value, true);
    }

    public void RemoveAt(int index)
    {
        int count = Count;
        if (index >= count || index < 0)
        {
            throw new IndexOutOfRangeException();
        }

        TKey1 key1 = pairs[index].key1;
        TKey2 key2 = pairs[index].key2;

        if (count > 1 && index < count - 1)
        {
            int lastValueIndex = count - 1;
            pairs[index] = pairs[lastValueIndex];
            KeyPair newKeyPair = new KeyPair(pairs[lastValueIndex].key1, pairs[lastValueIndex].key2);
            indexer[newKeyPair] = index;
            pairs.RemoveAt(lastValueIndex);
        }
        else
        {
            pairs.RemoveAt(index);
        }

        KeyPair removeKeyPair = new KeyPair(key1, key2);
        indexer.Remove(removeKeyPair);
    }

    public bool Remove(TKey1 key1, TKey2 key2)
    {
        KeyPair keyPair = new KeyPair(key1, key2);

        if (!indexer.TryGetValue(keyPair, out int index))
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

    public IEnumerator<KeyValueSetTriplet<TKey1, TKey2, TValue>> GetEnumerator()
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

    private void AddInternal(TKey1 key1, TKey2 key2, TValue value, bool add)
    {
        KeyPair keyPair = new KeyPair(key1, key2);
        if (!add && indexer.TryGetValue(keyPair, out int index))
        {
            pairs[index] = new KeyValueSetTriplet<TKey1, TKey2, TValue>(keyPair.key1, keyPair.key2, value);
            return;
        }
        indexer.Add(keyPair, pairs.Count);
        pairs.Add(new KeyValueSetTriplet<TKey1, TKey2, TValue>(keyPair.key1, keyPair.key2, value));
    }
}