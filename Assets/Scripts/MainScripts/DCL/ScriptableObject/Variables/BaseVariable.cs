using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseVariable<T> : ScriptableObject, IEquatable<T>
{
    public delegate void Change(T current, T previous);

    public event Change OnChange;

    [SerializeField] protected T value;

    public void Set(T newValue)
    {
        if (Equals(newValue))
            return;

        var previous = value;
        value = newValue;
        OnChange?.Invoke(value, previous);
    }

    public T Get()
    {
        return value;
    }

    public static implicit operator T(BaseVariable<T> value) => value.value;

    public virtual bool Equals(T other)
    {
        //NOTE(Brian): According to benchmarks I made, this statement costs about twice than == operator for structs.
        //             However, its way more costly for primitives (tested only against float).

        //             Left here only for fallback purposes. Optimally this method should be always overriden.
        return EqualityComparer<T>.Default.Equals(value, other);
    }

#if UNITY_EDITOR
    [ContextMenu("RaiseOnChange")]
    private void RaiseOnChange() => OnChange.Invoke(value, value);

    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        Resources.UnloadAsset(this);
    }
#endif
}
