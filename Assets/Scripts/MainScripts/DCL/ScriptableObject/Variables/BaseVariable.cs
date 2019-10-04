using System;
using UnityEngine;

public class BaseVariable<T> : ScriptableObject
{
    public delegate void Change(T current, T previous);

    private event Change onChangeValue;
    public virtual event Change onChange
    {
        add => onChangeValue += value;
        remove => onChangeValue -= value;
    }

    [SerializeField] protected T value;

    public void Set(T newValue)
    {
        var previous = value;
        value = newValue;
        onChangeValue?.Invoke(value, previous);
    }
    
    public T Get()
    {
        return value;
    }

    public static implicit operator T(BaseVariable<T> value) => value.value;

#if UNITY_EDITOR
    [ContextMenu("RaiseOnChange")]
    private void RaiseOnChange() => onChangeValue.Invoke(value, value);
#endif

#if UNITY_EDITOR
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