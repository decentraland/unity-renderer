using System.Collections.Generic;
using UnityEngine;

public class ListVariable<T> : ScriptableObject
{
    public delegate void Added(T addedValue);
    public delegate void Removed(T removedValue);

    private event Added OnAddedElementValue;
    private event Removed OnRemovedElementValue;

    public virtual event Added OnAdded
    {
        add => OnAddedElementValue += value;
        remove => OnAddedElementValue -= value;
    }

    public virtual event Removed OnRemoved
    {
        add => OnRemovedElementValue += value;
        remove => OnRemovedElementValue -= value;
    }

    [SerializeField] protected List<T> list = new List<T>();

    public void Add(T newValue)
    {
        list.Add(newValue);
        OnAddedElementValue?.Invoke(newValue);
    }

    public void Add(T[] newValues)
    {
        int count = newValues.Length;
        for (int i = 0; i < count; ++i)
        {
            Add(newValues[i]);
        }
    }

    public void Remove(T value)
    {
        if (!list.Contains(value)) return;

        list.Remove(value);
        OnRemovedElementValue?.Invoke(value);
    }

    public void Remove(T[] values)
    {
        int count = values.Length;
        for (int i = 0; i < count; ++i)
        {
            Remove(values[i]);
        }
    }

    public T Get(int index)
    {
        return list.Count >= index + 1 ? list[index] : default(T);
    }

    public List<T> GetList()
    {
        return list;
    }

    public void Clear()
    {
        for (int i = 0; i < list.Count; i++)
        {
            OnRemovedElementValue?.Invoke(list[i]);
        }

        list.Clear();
    }

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
