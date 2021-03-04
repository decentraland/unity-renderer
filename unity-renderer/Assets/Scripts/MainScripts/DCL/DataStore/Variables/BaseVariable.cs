using System;
using System.Collections.Generic;

public delegate void Change<T>(T current, T previous);

public class BaseVariable<T> : IBaseVariable<T>, IEquatable<T>
{
    public event Change<T> OnChange;

    protected T value;

    public T Get()
    {
        return value;
    }

    public void Set(T newValue)
    {
        if (Equals(newValue))
            return;

        var previous = value;
        value = newValue;
        OnChange?.Invoke(value, previous);
    }

    public virtual bool Equals(T other)
    {
        return EqualityComparer<T>.Default.Equals(value, other);
    }
}
