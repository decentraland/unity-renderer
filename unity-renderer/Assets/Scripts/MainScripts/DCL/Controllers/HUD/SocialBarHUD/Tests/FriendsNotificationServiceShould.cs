using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class FriendsNotificationServiceShould
{
    private FriendsNotificationService service;
    private IPlayerPrefs playerPrefs;
    private IFriendsController friendsController;
    private FloatVariable pendingFriendRequests;
    private FloatVariable newApprovedFriends;

    [SetUp]
    public void SetUp()
    {
        playerPrefs = Substitute.For<IPlayerPrefs>();
        friendsController = Substitute.For<IFriendsController>();
        pendingFriendRequests = ScriptableObject.CreateInstance<FloatVariable>();
        newApprovedFriends = ScriptableObject.CreateInstance<FloatVariable>();
        service = new FriendsNotificationService(playerPrefs, friendsController,
            pendingFriendRequests, newApprovedFriends);
    }
    
    [Test]
    public void MarkFriendsAsSeen()
    {
        service.MarkFriendsAsSeen(5);
        
        playerPrefs.Received(1).Set("SeenFriendsCount", 5);
        playerPrefs.Received(1).Save();
    }
    
    [Test]
    public void MarkRequestsAsSeen()
    {
        service.MarkRequestsAsSeen(3);

        Assert.AreEqual(3, pendingFriendRequests.Get());
    }
    
    [Test]
    public void UpdateUnseenFriends()
    {
        playerPrefs.GetInt("SeenFriendsCount").Returns(3);
        friendsController.friendCount.Returns(7);
            
        service.UpdateUnseenFriends();

        Assert.AreEqual(4, newApprovedFriends.Get());
    }
}