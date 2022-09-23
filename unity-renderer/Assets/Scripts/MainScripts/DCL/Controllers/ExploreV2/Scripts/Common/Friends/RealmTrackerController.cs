using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RealmTrackerController : IDisposable
{
    Dictionary<IRealmTrackerHandler, TrackedRealmInfo> listeners = new Dictionary<IRealmTrackerHandler, TrackedRealmInfo>();
    Dictionary<string, RealmTracker> friends = new Dictionary<string, RealmTracker>();

    IFriendsController friendsController;
    Color[] friendColors;

    public RealmTrackerController(IFriendsController friendsController, Color[] friendColors)
    {
        this.friendsController = friendsController;

        if (friendColors != null && friendColors.Length > 0)
        {
            this.friendColors = friendColors;
        }
        else
        {
            this.friendColors = new Color[] { Color.white };
        }

        if (friendsController != null)
        {
            if (!friendsController.IsInitialized)
            {
                friendsController.OnInitialized += OnFriendsInitialized;
            }

            friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
        }
    }

    public void Dispose()
    {
        if (friendsController != null)
        {
            friendsController.OnInitialized -= OnFriendsInitialized;
            friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        }
        listeners.Clear();
        friends.Clear();
    }

    public void AddHandler(IRealmTrackerHandler listener)
    {
        TrackedRealmInfo wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            return;
        }

        wrapper = new TrackedRealmInfo(listener);

        if (friendsController != null && friendsController.IsInitialized)
        {
            ProcessNewListener(wrapper);
        }

        listeners.Add(listener, wrapper);
    }

    public void RemoveHandler(IRealmTrackerHandler listener)
    {
        TrackedRealmInfo wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            wrapper.Dispose();
            listeners.Remove(listener);
        }
    }

    public void RemoveAllHandlers()
    {
        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                listenersIterator.Current.Value.Dispose();
            }
        }

        listeners.Clear();
    }

    void OnUpdateUserStatus(string userId, UserStatus status)
    {
        if (!friendsController.IsInitialized)
            return;

        RealmTracker friend;
        if (!friends.TryGetValue(userId, out friend))
        {
            friend = new RealmTracker(userId, friendColors[Random.Range(0, friendColors.Length)]);
            friends.Add(userId, friend);
        }

        friend.SetStatus(status);

        if (!friend.IsOnline())
        {
            friend.RemoveAllListeners();
        }
        else
        {
            ProcessFriendRealm(friend, status.realm?.serverName);
        }
    }

    void OnFriendsInitialized()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;

        using (var friendsIterator = friendsController.GetAllocatedFriends().GetEnumerator())
        {
            while (friendsIterator.MoveNext())
            {
                RealmTracker friend = new RealmTracker(friendsIterator.Current.Key, friendColors[Random.Range(0, friendColors.Length)]);
                friend.SetStatus(friendsIterator.Current.Value);
                friends.Add(friendsIterator.Current.Key, friend);
            }
        }

        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                ProcessNewListener(listenersIterator.Current.Value);
            }
        }
    }

    void ProcessFriendRealm(RealmTracker friend, string serverName)
    {
        if (!friend.HasChangedRealm(serverName))
            return;

        friend.RemoveAllListeners();

        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                if (listenersIterator.Current.Value.ContainRealm(serverName))
                {
                    friend.AddListener(listenersIterator.Current.Value);
                }
            }
        }
    }

    void ProcessNewListener(TrackedRealmInfo listener)
    {
        string friendRealm = string.Empty;
        using (var friendIterator = friends.GetEnumerator())
        {
            while (friendIterator.MoveNext())
            {
                if (!friendIterator.Current.Value.IsOnline())
                {
                    continue;
                }

                friendRealm = friendIterator.Current.Value.status.realm?.serverName;

                if (listener.ContainRealm(friendRealm))
                {
                    friendIterator.Current.Value.AddListener(listener);
                }
            }
        }
    }
}