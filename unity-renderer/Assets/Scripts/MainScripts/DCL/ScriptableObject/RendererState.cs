using UnityEngine;
using System;

[CreateAssetMenu(fileName = "RendererState", menuName = "RendererState")]
public class RendererState : BooleanVariable
{
    public event Action<object> OnLockAdded;
    public event Action<object> OnLockRemoved;

    public void AddLock(object id)
    {
        OnLockAdded?.Invoke(id);
    }

    public void RemoveLock(object id)
    {
        OnLockRemoved?.Invoke(id);
    }
}
