using System;

[Serializable]
public class FriendshipUpdateStatusMessage
{
    public string userId;
    public FriendshipAction action;
}