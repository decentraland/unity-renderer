using System;

[Serializable]
public class AddFriendsPayload
{
    public string[] currentFriends;
}

[Serializable]
public class AddFriendRequestsPayload
{
    public string[] requestedTo;
    public string[] requestedFrom;
}

[Serializable]
public class AddFriendsWithDirectMessagesPayload
{
    public string[] currentFriendsWithDirectMessages;
}