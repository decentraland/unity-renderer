using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class FriendTracker
{
    HashSet<TrackedSceneInfo> friendListeners = new HashSet<TrackedSceneInfo>();

    public UserProfile profile { private set; get; }
    public UserStatus status { private set; get; }
    Color backgroundColor;

    public FriendTracker(string userId, Color backgroundColor)
    {
        profile = UserProfileController.userProfilesCatalog.Get(userId);
        this.backgroundColor = backgroundColor;
    }

    public void SetStatus(UserStatus newStatus) { status = newStatus; }

    public void AddListener(TrackedSceneInfo listener)
    {
        listener.OnListenerDisposed += OnListenerDisposed;
        friendListeners.Add(listener);
        listener.OnFriendAdded(profile, backgroundColor);
    }

    public void RemoveListener(TrackedSceneInfo listener)
    {
        OnListenerRemoved(listener);
        friendListeners.Remove(listener);
    }

    public bool HasListeners() { return friendListeners.Count > 0; }

    public bool HasChangedLocation(Vector2Int coords)
    {
        if (friendListeners.Count > 0 && friendListeners.First().ContainCoords(coords))
        {
            return false;
        }

        return true;
    }

    public void RemoveAllListeners()
    {
        if (!HasListeners())
        {
            return;
        }

        using (var listenerIterator = friendListeners.GetEnumerator())
        {
            while (listenerIterator.MoveNext())
            {
                OnListenerRemoved(listenerIterator.Current);
            }
        }

        friendListeners.Clear();
    }

    public bool IsOnline()
    {
        if (status.presence != PresenceStatus.ONLINE)
            return false;
        if (status.realm == null)
            return false;
        return !string.IsNullOrEmpty(status.realm.serverName);
    }

    void OnListenerDisposed(TrackedSceneInfo listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        friendListeners.Remove(listener);
    }

    void OnListenerRemoved(TrackedSceneInfo listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        listener.OnFriendRemoved(profile);
    }
}