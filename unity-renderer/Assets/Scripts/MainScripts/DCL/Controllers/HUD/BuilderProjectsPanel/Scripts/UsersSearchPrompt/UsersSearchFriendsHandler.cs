using System;
using System.Collections.Generic;
using DCL.Helpers;

internal class UsersSearchFriendsHandler : IDisposable
{
    public event Action<string> OnFriendRemoved;
    public bool isFriendlistDirty { private set; get; }

    private readonly IFriendsController friendsController;
    
    private bool waitingFriendsInitialize;
    private Promise<Dictionary<string, FriendsController.UserStatus>> friendListPromise;

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

    public Promise<Dictionary<string, FriendsController.UserStatus>> GetFriendList()
    {
        if (friendListPromise == null)
        {
            friendListPromise = new Promise<Dictionary<string, FriendsController.UserStatus>>();
        }

        if (friendsController == null)
        {
            return friendListPromise;
        }

        if (isFriendlistDirty && friendsController.isInitialized)
        {
            isFriendlistDirty = false;
            waitingFriendsInitialize = false;
            friendListPromise.Resolve(friendsController.GetFriends());
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
        friendListPromise?.Resolve(friendsController.GetFriends());
    }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        if (!friendsController.isInitialized)
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
