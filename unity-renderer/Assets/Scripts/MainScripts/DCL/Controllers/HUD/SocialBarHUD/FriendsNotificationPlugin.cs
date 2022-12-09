using DCL;
using DCL.Helpers;
using DCL.Social.Friends;

public class FriendsNotificationPlugin : IPlugin
{
    private const string PLAYER_PREFS_SEEN_FRIEND_COUNT = "SeenFriendsCount";

    private readonly IPlayerPrefs playerPrefs;
    private readonly IFriendsController friendsController;
    private readonly FloatVariable memoryPendingFriendRequestsRepository;
    private readonly FloatVariable memoryNewApprovedFriendsRepository;
    private readonly DataStore dataStore;

    public FriendsNotificationPlugin(IPlayerPrefs playerPrefs,
        IFriendsController friendsController,
        FloatVariable memoryPendingFriendRequestsRepository,
        FloatVariable memoryNewApprovedFriendsRepository,
        DataStore dataStore)
    {
        this.playerPrefs = playerPrefs;
        this.friendsController = friendsController;
        this.memoryPendingFriendRequestsRepository = memoryPendingFriendRequestsRepository;
        this.memoryNewApprovedFriendsRepository = memoryNewApprovedFriendsRepository;
        this.dataStore = dataStore;

        dataStore.friendNotifications.seenFriends.OnChange += MarkFriendsAsSeen;
        dataStore.friendNotifications.pendingFriendRequestCount.OnChange += UpdatePendingFriendRequests;
    }

    public void Dispose()
    {
        dataStore.friendNotifications.seenFriends.OnChange -= MarkFriendsAsSeen;
        dataStore.friendNotifications.pendingFriendRequestCount.OnChange -= UpdatePendingFriendRequests;
    }

    private void MarkFriendsAsSeen(int current, int previous)
    {
        playerPrefs.Set(PLAYER_PREFS_SEEN_FRIEND_COUNT, current);
        playerPrefs.Save();
        UpdateNewApprovedFriends();
    }

    private void UpdatePendingFriendRequests(int current, int previous) =>
        memoryPendingFriendRequestsRepository.Set(current);

    private void UpdateNewApprovedFriends()
    {
        var seenFriendsCount = playerPrefs.GetInt(PLAYER_PREFS_SEEN_FRIEND_COUNT);
        var friendsCount = friendsController.AllocatedFriendCount;
        var newFriends = friendsCount - seenFriendsCount;

        //NOTE(Brian): If someone deletes you, don't show badge notification
        if (newFriends < 0)
            newFriends = 0;

        memoryNewApprovedFriendsRepository.Set(newFriends);
    }
}
