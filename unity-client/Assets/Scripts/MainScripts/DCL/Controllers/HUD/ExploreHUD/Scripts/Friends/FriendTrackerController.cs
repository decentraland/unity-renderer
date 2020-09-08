using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

internal class FriendTrackerController : IDisposable
{
    Dictionary<IFriendTrackerHandler, TrackedSceneInfo> listeners = new Dictionary<IFriendTrackerHandler, TrackedSceneInfo>();
    Dictionary<string, FriendTracker> friends = new Dictionary<string, FriendTracker>();

    IFriendsController friendsController;
    Color[] friendColors;

    public FriendTrackerController(IFriendsController friendsController, Color[] friendColors)
    {
        this.friendsController = friendsController;

        if (friendColors != null && friendColors.Length > 0)
        {
            this.friendColors = friendColors;
        }
        else
        {
            this.friendColors = new Color[] {Color.white};
        }

        if (!friendsController.isInitialized)
        {
            friendsController.OnInitialized += OnFriendsInitialized;
        }

        friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
    }

    public void Dispose()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;
        friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        listeners.Clear();
        friends.Clear();
    }

    public void AddHandler(IFriendTrackerHandler listener)
    {
        TrackedSceneInfo wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            return;
        }

        wrapper = new TrackedSceneInfo(listener);

        if (friendsController.isInitialized)
        {
            ProcessNewListener(wrapper);
        }

        listeners.Add(listener, wrapper);
    }

    public void RemoveHandler(IFriendTrackerHandler listener)
    {
        TrackedSceneInfo wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            wrapper.Dispose();
            listeners.Remove(listener);
        }
    }

    void OnUpdateUserStatus(string userId, FriendsController.UserStatus status)
    {
        if (!friendsController.isInitialized)
            return;

        FriendTracker friend;
        if (!friends.TryGetValue(userId, out friend))
        {
            friend = new FriendTracker(userId, friendColors[Random.Range(0, friendColors.Length)]);
            friends.Add(userId, friend);
        }

        friend.SetStatus(status);

        if (!friend.IsOnline())
        {
            friend.RemoveAllListeners();
        }
        else
        {
            ProcessFriendLocation(friend, new Vector2Int((int) status.position.x, (int) status.position.y));
        }
    }

    void OnFriendsInitialized()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;

        using (var friendsIterator = friendsController.GetFriends().GetEnumerator())
        {
            while (friendsIterator.MoveNext())
            {
                FriendTracker friend = new FriendTracker(friendsIterator.Current.Key, friendColors[Random.Range(0, friendColors.Length)]);
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

    void ProcessFriendLocation(FriendTracker friend, Vector2Int coords)
    {
        if (!friend.HasChangedLocation(coords))
            return;

        friend.RemoveAllListeners();

        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                if (listenersIterator.Current.Value.ContainCoords(coords))
                {
                    friend.AddListener(listenersIterator.Current.Value);
                }
            }
        }
    }

    void ProcessNewListener(TrackedSceneInfo listener)
    {
        Vector2Int friendCoords = new Vector2Int();
        using (var friendIterator = friends.GetEnumerator())
        {
            while (friendIterator.MoveNext())
            {
                if (!friendIterator.Current.Value.IsOnline())
                {
                    continue;
                }

                friendCoords.x = (int) friendIterator.Current.Value.status.position.x;
                friendCoords.y = (int) friendIterator.Current.Value.status.position.y;

                if (listener.ContainCoords(friendCoords))
                {
                    friendIterator.Current.Value.AddListener(listener);
                }
            }
        }
    }
}