using System;
using System.Collections.Generic;

public class BaseHashSet<T> : IBaseCollection<T>, IEquatable<IEnumerable<T>>
{
    public event Action<IEnumerable<T>> OnSet;
    public event Action<T> OnAdded;
    public event Action<T> OnRemoved;

    internal readonly HashSet<T> hashSet = new HashSet<T>();

    public BaseHashSet() { }
    public BaseHashSet(IEnumerable<T> elements) { hashSet = new HashSet<T>(elements); }

    public IEnumerable<T> Get() => hashSet;

    public void Set(IEnumerable<T> elements)
    {
        hashSet.Clear();
        hashSet.UnionWith(elements);

        OnSet?.Invoke(hashSet);
    }

    public void Add(T element)
    {
        hashSet.Add(element);
        OnAdded?.Invoke(element);
    }

    public void Add(IEnumerable<T> elements)
    {
        foreach ( T e in elements )
        {
            Add(e);
        }
    }

    public bool Remove(T element)
    {
        if (!hashSet.Remove(element))
            return false;

        OnRemoved?.Invoke(element);
        return true;
    }

    public void Remove(IEnumerable<T> elements)
    {
        foreach ( T e in elements )
        {
            Remove(e);
        }
    }

    public int Count() => hashSet.Count;

    public bool Contains(T element) => hashSet.Contains(element);

    public virtual bool Equals(IEnumerable<T> other) { return EqualityComparer<IEnumerable<T>>.Default.Equals(hashSet, other); }
}