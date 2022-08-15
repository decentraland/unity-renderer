using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RealmTracker
{
    HashSet<TrackedRealmInfo> realmListeners = new HashSet<TrackedRealmInfo>();

    public UserProfile profile { private set; get; }
    public UserStatus status { private set; get; }
    Color backgroundColor;

    public RealmTracker(string userId, Color backgroundColor)
    {
        profile = UserProfileController.userProfilesCatalog.Get(userId);
        this.backgroundColor = backgroundColor;
    }

    public void SetStatus(UserStatus newStatus) { status = newStatus; }

    public void AddListener(TrackedRealmInfo listener)
    {
        listener.OnListenerDisposed += OnListenerDisposed;
        realmListeners.Add(listener);
        listener.OnFriendAdded(profile, backgroundColor);
    }

    public void RemoveListener(TrackedRealmInfo listener)
    {
        OnListenerRemoved(listener);
        realmListeners.Remove(listener);
    }

    public bool HasListeners() { return realmListeners.Count > 0; }

    public bool HasChangedRealm(string serverName)
    {
        if (realmListeners.Count > 0 && realmListeners.First().ContainRealm(serverName))
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

        using (var listenerIterator = realmListeners.GetEnumerator())
        {
            while (listenerIterator.MoveNext())
            {
                OnListenerRemoved(listenerIterator.Current);
            }
        }

        realmListeners.Clear();
    }

    public bool IsOnline()
    {
        if (status.presence != PresenceStatus.ONLINE)
            return false;
        if (status.realm == null)
            return false;
        return !string.IsNullOrEmpty(status.realm.serverName);
    }

    void OnListenerDisposed(TrackedRealmInfo listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        realmListeners.Remove(listener);
    }

    void OnListenerRemoved(TrackedRealmInfo listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        listener.OnFriendRemoved(profile);
    }
}