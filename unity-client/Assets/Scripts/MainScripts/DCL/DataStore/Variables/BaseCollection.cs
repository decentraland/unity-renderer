using System;
using System.Collections.Generic;

public class BaseCollection<T> : IBaseCollection<T>, IEquatable<IEnumerable<T>>
{
    public event Action<IEnumerable<T>> OnSet;
    public event Action<T> OnAdded;
    public event Action<T> OnRemoved;

    internal readonly List<T> list = new List<T>();

    public BaseCollection() { }
    public BaseCollection(IEnumerable<T> elements)
    {
        list = new List<T>(elements);
    }


    public IEnumerable<T> Get() => list;

    public T this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }

    public void Set(IEnumerable<T> elements)
    {
        list.Clear();
        list.AddRange(elements);

        OnSet?.Invoke(list);
    }

    public void Add(T element)
    {
        list.Add(element);
        OnAdded?.Invoke(element);
    }

    public bool Remove(T element)
    {
        if (!list.Remove(element))
            return false;

        OnRemoved?.Invoke(element);
        return true;
    }

    public void RemoveAt(int index)
    {
        T value = list[index];
        list.RemoveAt(index);
        OnRemoved?.Invoke(value);
    }

    public T ElementAt(int index) => list[index];
    public int Count() => list.Count;

    public virtual bool Equals(IEnumerable<T> other)
    {
        return EqualityComparer<IEnumerable<T>>.Default.Equals(list, other);
    }
}