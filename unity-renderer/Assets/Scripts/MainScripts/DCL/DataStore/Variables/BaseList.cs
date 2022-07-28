using System;
using System.Collections;
using System.Collections.Generic;

public class BaseList<TValue> : IList<TValue>, IReadOnlyList<TValue>
{
    private readonly List<TValue> list;

    public event Action<TValue> OnAdded;
    public event Action<TValue> OnRemoved;

    public TValue this[int index] { get => list[index]; set => list[index] = value; }

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public BaseList() : this(0) { }

    public BaseList(int capacity)
    {
        list = new List<TValue>(capacity);
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(TValue item)
    {
        list.Add(item);
        OnAdded?.Invoke(item);
    }

    public void Clear()
    {
        list.Clear();
    }

    public bool Contains(TValue item)
    {
        return list.Contains(item);
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(TValue item)
    {
        if (!list.Remove(item))
            return false;

        OnRemoved?.Invoke(item);
        return true;
    }

    public int IndexOf(TValue item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index, TValue item)
    {
        list.Insert(index, item);
        OnAdded?.Invoke(item);
    }

    public void RemoveAt(int index)
    {
        TValue item = list[index];
        list.RemoveAt(index);
        OnRemoved?.Invoke(item);
    }
}