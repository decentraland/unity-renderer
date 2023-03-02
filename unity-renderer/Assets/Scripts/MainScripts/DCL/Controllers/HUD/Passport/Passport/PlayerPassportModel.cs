using DCL.Social.Friends;
using System;

[Serializable]
public record PlayerPassportModel
{
    public string name;
    public string userId;
    public PresenceStatus presenceStatus;
    public bool isGuest;
    public bool isBlocked;
    public bool hasBlocked;
    public FriendshipStatus friendshipStatus;
    public bool isFriendshipVisible;
}
