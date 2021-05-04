using System;
using System.Collections.Generic;

public delegate void Change<T>(T current, T previous);

public class BaseVariable<T> : IBaseVariable<T>, IEquatable<T>
{
    public event Change<T> OnChange;

    protected T value;

    public BaseVariable() { value = default; }
    public BaseVariable(T defaultValue) { value = defaultValue; }

    public T Get() { return value; }

    public void Set(T newValue) { Set(newValue, !Equals(newValue)); }

    public void Set(T newValue, bool notifyEvent)
    {
        var previous = value;
        value = newValue;

        if (notifyEvent)
        {
            OnChange?.Invoke(value, previous);
        }
    }

    public virtual bool Equals(T other) { return EqualityComparer<T>.Default.Equals(value, other); }
}