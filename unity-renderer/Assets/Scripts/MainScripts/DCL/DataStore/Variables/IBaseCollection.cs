using System;
using System.Collections.Generic;

public interface IBaseCollection<T>
{
    event Action<IEnumerable<T>> OnSet;
    event Action<T> OnAdded;
    event Action<T> OnRemoved;

    T this[int index] { get; set; }

    IEnumerable<T> Get();
    void Set(IEnumerable<T> elements);
    void Add(T element);
    bool Remove(T element);
    void RemoveAt(int index);
    T ElementAt(int index);
    int Count();
    bool Contains(T element);
}
