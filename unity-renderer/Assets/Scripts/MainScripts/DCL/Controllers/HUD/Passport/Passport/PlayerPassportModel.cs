using DCL.Social.Friends;
using System;

[Serializable]
public class PlayerPassportModel : BaseComponentModel
{
    public string name;
    public string userId;
    public PresenceStatus presenceStatus;
    public bool isGuest;
    public bool isBlocked;
    public bool hasBlocked;
    public FriendshipStatus friendshipStatus;
}
