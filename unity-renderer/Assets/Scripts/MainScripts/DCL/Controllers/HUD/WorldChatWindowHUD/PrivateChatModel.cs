using DCL.Interface;

public struct PrivateChatModel
{
    public string userId;
    public string userName;
    public string faceSnapshotUrl;
    public ChatMessage recentMessage;
    public bool isBlocked;
    public bool isOnline;
}
