using DCL.Helpers;

public class FriendsNotificationService : IFriendsNotificationService
{
    private const string PLAYER_PREFS_SEEN_FRIEND_COUNT = "SeenFriendsCount";
    
    private readonly IPlayerPrefs playerPrefs;
    private readonly IFriendsController friendsController;
    private readonly FloatVariable memoryPendingFriendRequestsRepository;
    private readonly FloatVariable memoryNewApprovedFriendsRepository;

    public FriendsNotificationService(IPlayerPrefs playerPrefs,
        IFriendsController friendsController,
        FloatVariable memoryPendingFriendRequestsRepository,
        FloatVariable memoryNewApprovedFriendsRepository)
    {
        this.playerPrefs = playerPrefs;
        this.friendsController = friendsController;
        this.memoryPendingFriendRequestsRepository = memoryPendingFriendRequestsRepository;
        this.memoryNewApprovedFriendsRepository = memoryNewApprovedFriendsRepository;
    }
    
    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public void MarkFriendsAsSeen(int count)
    {
        playerPrefs.Set(PLAYER_PREFS_SEEN_FRIEND_COUNT, count);
        playerPrefs.Save();
    }

    public void MarkRequestsAsSeen(int count)
    {
        // var pendingFriendRequestsSO = NotificationScriptableObjects.pendingFriendRequests;
        memoryPendingFriendRequestsRepository.Set(count);
    }

    public void UpdateUnseenFriends()
    {
        var seenFriendsCount = playerPrefs.GetInt(PLAYER_PREFS_SEEN_FRIEND_COUNT);
        var friendsCount = friendsController.friendCount;
        var newFriends = friendsCount - seenFriendsCount;

        //NOTE(Brian): If someone deletes you, don't show badge notification
        if (newFriends < 0)
            newFriends = 0;

        // var newApprovedFriendsSO = NotificationScriptableObjects.newApprovedFriends;
        memoryNewApprovedFriendsRepository.Set(newFriends);
    }
}