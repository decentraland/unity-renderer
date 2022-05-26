using DCL.Interface;

public struct PrivateChatModel
{
    public UserProfile user;
    public ChatMessage recentMessage;
    public bool isBlocked;
    public bool isOnline;
}