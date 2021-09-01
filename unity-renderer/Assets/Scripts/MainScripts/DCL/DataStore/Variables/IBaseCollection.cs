using System;
using System.Collections.Generic;

public interface IBaseCollection<T>
{
    event Action<IEnumerable<T>> OnSet;
    event Action<T> OnAdded;
    event Action<T> OnRemoved;

    IEnumerable<T> Get();
    void Set(IEnumerable<T> elements);
    void Add(T element);
    bool Remove(T element);
    int Count();
    bool Contains(T element);
}