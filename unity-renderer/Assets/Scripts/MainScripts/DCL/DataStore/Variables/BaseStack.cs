using System;
using System.Collections.Generic;

public class BaseStack<T> : IBaseCollection<T>, IEquatable<IEnumerable<T>>
{
    public event Action<IEnumerable<T>> OnSet;
    public event Action<T> OnAdded;
    public event Action<T> OnRemoved;

    internal readonly Stack<T> stack = new Stack<T>();

    public BaseStack() { stack = new Stack<T>(); }
    public BaseStack(IEnumerable<T> elements) { stack = new Stack<T>(elements); }

    public IEnumerable<T> Get() => stack;

    private T Peek()
    {
        return stack.Peek();
    }

   public void Set(IEnumerable<T> elements)
    {
        stack.Clear();
        foreach (T element in elements)
        {
            stack.Push(element);
        }

        OnSet?.Invoke(stack);
    }

    public void Add(T element)
    {
        stack.Push(element);
        OnAdded?.Invoke(element);
    }

    public bool Remove(T element)
    {
        if (Count() == 0)
            return false;

        OnRemoved?.Invoke(stack.Pop());
        return true;
    }

    public int Count() => stack.Count;

    public bool Contains(T element) => stack.Contains(element);

    public virtual bool Equals(IEnumerable<T> other) { return EqualityComparer<IEnumerable<T>>.Default.Equals(stack, other); }
}