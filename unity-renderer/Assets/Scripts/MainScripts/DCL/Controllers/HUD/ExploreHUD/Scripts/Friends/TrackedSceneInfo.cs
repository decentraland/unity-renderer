using System;
using UnityEngine;

class TrackedSceneInfo : IDisposable
{
    IFriendTrackerHandler view;

    public event Action<TrackedSceneInfo> OnListenerDisposed;

    public TrackedSceneInfo(IFriendTrackerHandler view)
    {
        this.view = view;
    }

    public void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        view.OnFriendAdded(profile, backgroundColor);
    }

    public void OnFriendRemoved(UserProfile profile)
    {
        view.OnFriendRemoved(profile);
    }

    public bool ContainCoords(Vector2Int coords)
    {
        return view.ContainCoords(coords);
    }

    public void Dispose()
    {
        OnListenerDisposed?.Invoke(this);
    }
}