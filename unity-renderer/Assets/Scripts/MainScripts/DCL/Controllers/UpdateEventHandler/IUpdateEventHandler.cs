using System;

public interface IUpdateEventHandler : IDisposable
{
    public enum EventType
    {
        Update,
        LateUpdate,
        FixedUpdate,
        OnGui
    }

    public void AddListener( EventType eventType, Action action );
    public void RemoveListener( EventType eventType, Action action );
}