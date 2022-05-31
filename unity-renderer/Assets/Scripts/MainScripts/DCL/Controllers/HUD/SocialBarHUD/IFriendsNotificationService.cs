using DCL;

public interface IFriendsNotificationService : IService
{
    void MarkFriendsAsSeen(int friendsControllerFriendCount);
    void MarkRequestsAsSeen(int count);
    void UpdateUnseenFriends();
}