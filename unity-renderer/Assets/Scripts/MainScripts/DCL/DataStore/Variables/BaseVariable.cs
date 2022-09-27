using System;
using System.Collections.Generic;

public delegate void Change<T>(T current, T previous);

public delegate void ChangeWithSenderInfo<T>(BaseVariable<T> sender, T current, T previous);

public class BaseVariable<T> : IBaseVariable<T>, IEquatable<T>
{
    protected T value;

    public BaseVariable() => value = default;
    public BaseVariable(T defaultValue) => value = defaultValue;

    public event Change<T> OnChange;

    public T Get() => value;

    public void Set(T newValue) =>
        Set(newValue, !Equals(newValue));

    public virtual bool Equals(T other) =>
        EqualityComparer<T>.Default.Equals(value, other);

    public event ChangeWithSenderInfo<T> OnChangeWithSenderInfo;

    public void Set(T newValue, bool notifyEvent)
    {
        var previous = value;
        value = newValue;

        if (notifyEvent)
        {
            OnChange?.Invoke(value, previous);
            OnChangeWithSenderInfo?.Invoke(this, value, previous);
        }
    }

    public int OnChangeListenersCount() =>
        OnChange == null ? 0 : OnChange.GetInvocationList().Length;
}