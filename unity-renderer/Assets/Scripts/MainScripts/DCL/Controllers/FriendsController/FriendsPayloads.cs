using System;

[Serializable]
public class AddFriendsPayload
{
    public string[] currentFriends;
    public int totalFriends;
}

[Serializable]
public class AddFriendRequestsPayload
{
    public string[] requestedTo;
    public string[] requestedFrom;
    public int totalFriendRequests;
}

[Serializable]
public class AddFriendsWithDirectMessagesPayload
{
    public FriendWithDirectMessages[] currentFriendsWithDirectMessages;
    public int totalFriendsWithDirectMessages;
}

[Serializable]
public class FriendWithDirectMessages
{
    public string userId;
    public string lastMessageBody;
    public long lastMessageTimestamp;
}