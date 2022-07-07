using System;
using System.Collections.Generic;
using DCL.Helpers;

internal class UsersSearchFriendsHandler : IDisposable
{
    public event Action<string> OnFriendRemoved;
    public bool isFriendlistDirty { private set; get; }

    private readonly IFriendsController friendsController;

    private bool waitingFriendsInitialize;
    private Promise<Dictionary<string, UserStatus>> friendListPromise;

    public UsersSearchFriendsHandler(IFriendsController friendsController)
    {
        this.friendsController = friendsController;
        isFriendlistDirty = true;
        waitingFriendsInitialize = true;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += OnUpdateFriendship;
        }
    }

    public Promise<Dictionary<string, UserStatus>> GetFriendList()
    {
        if (friendListPromise == null)
        {
            friendListPromise = new Promise<Dictionary<string, UserStatus>>();
        }

        if (friendsController == null)
        {
            return friendListPromise;
        }

        if (isFriendlistDirty && friendsController.IsInitialized)
        {
            isFriendlistDirty = false;
            waitingFriendsInitialize = false;
            friendListPromise.Resolve(friendsController.GetAllocatedFriends());
        }

        if (waitingFriendsInitialize)
        {
            waitingFriendsInitialize = false;
            friendsController.OnInitialized += OnFriendsInitialized;
        }

        return friendListPromise;
    }

    public void Dispose()
    {
        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship -= OnUpdateFriendship;
            friendsController.OnInitialized -= OnFriendsInitialized;
        }

        friendListPromise?.Dispose();
        friendListPromise = null;
    }

    private void OnFriendsInitialized()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;
        isFriendlistDirty = false;
        friendListPromise?.Resolve(friendsController.GetAllocatedFriends());
    }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        if (!friendsController.IsInitialized)
            return;

        if (friendshipAction == FriendshipAction.APPROVED)
        {
            isFriendlistDirty = true;
            return;
        }

        if (friendshipAction == FriendshipAction.DELETED)
        {
            isFriendlistDirty = true;
            OnFriendRemoved?.Invoke(userId);
        }
    }
}