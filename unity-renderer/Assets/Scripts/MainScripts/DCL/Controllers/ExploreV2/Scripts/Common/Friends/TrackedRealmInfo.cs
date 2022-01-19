using System;
using UnityEngine;

public class TrackedRealmInfo : IDisposable
{
    IRealmTrackerHandler view;

    public event Action<TrackedRealmInfo> OnListenerDisposed;

    public TrackedRealmInfo(IRealmTrackerHandler view) { this.view = view; }

    public void OnFriendAdded(UserProfile profile, Color backgroundColor) { view.OnFriendAdded(profile, backgroundColor); }

    public void OnFriendRemoved(UserProfile profile) { view.OnFriendRemoved(profile); }

    public bool ContainRealm(string serverName) { return view.ContainRealm(serverName); }

    public void Dispose() { OnListenerDisposed?.Invoke(this); }
}