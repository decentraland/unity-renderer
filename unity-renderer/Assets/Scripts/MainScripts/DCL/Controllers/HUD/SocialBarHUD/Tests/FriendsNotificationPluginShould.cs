using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class FriendsNotificationPluginShould
{
    private FriendsNotificationPlugin plugin;
    private IPlayerPrefs playerPrefs;
    private IFriendsController friendsController;
    private FloatVariable pendingFriendRequests;
    private FloatVariable newApprovedFriends;
    private DataStore dataStore;

    [SetUp]
    public void SetUp()
    {
        playerPrefs = Substitute.For<IPlayerPrefs>();
        friendsController = Substitute.For<IFriendsController>();
        friendsController.AllocatedFriendCount.Returns(7);
        pendingFriendRequests = ScriptableObject.CreateInstance<FloatVariable>();
        newApprovedFriends = ScriptableObject.CreateInstance<FloatVariable>();
        dataStore = new DataStore();
        plugin = new FriendsNotificationPlugin(playerPrefs, friendsController,
            pendingFriendRequests, newApprovedFriends, dataStore);
    }

    [TearDown]
    public void TearDown()
    {
        plugin.Dispose();
    }
    
    [Test]
    public void MarkFriendsAsSeen()
    {
        const string seenFriendsPrefsKey = "SeenFriendsCount";
        const int seenFriendCount = 5;
        playerPrefs.GetInt(seenFriendsPrefsKey, Arg.Any<int>()).Returns(seenFriendCount);
            
        dataStore.friendNotifications.seenFriends.Set(seenFriendCount);
        
        playerPrefs.Received(1).Set(seenFriendsPrefsKey, seenFriendCount);
        playerPrefs.Received(1).Save();
        Assert.AreEqual(2, newApprovedFriends.Get());
    }
    
    [Test]
    public void MarkRequestsAsSeen()
    {
        const string seenFriendsPrefsKey = "SeenFriendsCount";
        const int seenRequestsCount = 3;
        playerPrefs.GetInt(seenFriendsPrefsKey, Arg.Any<int>()).Returns(6);
        
        dataStore.friendNotifications.seenRequests.Set(seenRequestsCount);

        Assert.AreEqual(seenRequestsCount, pendingFriendRequests.Get());
        Assert.AreEqual(1, newApprovedFriends.Get());
    }
}