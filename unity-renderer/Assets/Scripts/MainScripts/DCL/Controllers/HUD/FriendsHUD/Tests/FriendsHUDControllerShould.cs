using System;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections;
using DCL;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendsHUDControllerShould : IntegrationTestSuite_Legacy
{
    private const string OWN_USER_ID = "my-user";
    
    private FriendsHUDController controller;
    private IFriendsHUDComponentView view;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private IFriendsNotificationService friendsNotificationService;
    private IUserProfileBridge userProfileBridge;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        socialAnalytics = Substitute.For<ISocialAnalytics>();
        friendsNotificationService = Substitute.For<IFriendsNotificationService>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownProfile);
        controller = new FriendsHUDController(new DataStore(),
            friendsController,
            userProfileBridge,
            socialAnalytics,
            friendsNotificationService);
        friendsController = Substitute.For<IFriendsController>();
        view = Substitute.For<IFriendsHUDComponentView>();
        controller.Initialize(view);
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void ReactCorrectlyToWhisperClick()
    {
        const string id = "test-id-1";
        
        var pressedWhisper = false;
        controller.OnPressWhisper += (x) => { pressedWhisper = x == id; };
        
        view.OnWhisper += Raise.Event<Action<FriendEntryModel>>(new FriendEntryModel {userId = id});
        
        Assert.IsTrue(pressedWhisper);
    }

    [Test]
    public void HandleUsernameErrorCorrectly()
    {
        friendsController.OnFriendNotFound += Raise.Event<Action<string>>("test");
        
        view.Received(1).DisplayFriendUserNotFound();
    }

    [Test]
    public void SendFriendRequestByNameCorrectly()
    {
        const string userId = "test-id-1";
        const string userName = "waoh";
        friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND).Returns(false);
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel{userId = userId, name = userName});
        userProfileBridge.GetByName(userName).Returns(userProfile);
        userProfileBridge.Get(userId).Returns(userProfile);
        
        view.OnFriendRequestSent += Raise.Event<Action<string>>(userName);
        
        friendsController.Received(1).RequestFriendship(userId);
        socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, userId, 0, PlayerActionSource.FriendsHUD);
        view.Received(1).ShowRequestSendSuccess();
    }
    
    [Test]
    public void SendFriendRequestByIdCorrectly()
    {
        const string userId = "test-id-1";
        const string userName = "waoh";
        friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND).Returns(false);
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel{userId = userId, name = userName});
        userProfileBridge.GetByName(userName).Returns(userProfile);
        userProfileBridge.Get(userId).Returns(userProfile);
        
        view.OnFriendRequestSent += Raise.Event<Action<string>>(userId);
        
        friendsController.Received(1).RequestFriendship(userId);
        socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, userId, 0, PlayerActionSource.FriendsHUD);
        view.Received(1).ShowRequestSendSuccess();
    }
    
    [Test]
    public void FailFriendRequestWhenAlreadyFriends()
    {
        const string userId = "test-id-1";
        const string userName = "waoh";
        friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND).Returns(true);
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel{userId = userId, name = userName});
        userProfileBridge.GetByName(userName).Returns(userProfile);
        userProfileBridge.Get(userId).Returns(userProfile);
        
        view.OnFriendRequestSent += Raise.Event<Action<string>>(userId);

        view.Received(1).ShowRequestSendError(FriendRequestError.AlreadyFriends);
    }

    [TestCase(FriendshipAction.APPROVED)]
    [TestCase(FriendshipAction.REQUESTED_FROM)]
    [TestCase(FriendshipAction.DELETED)]
    [TestCase(FriendshipAction.REJECTED)]
    [TestCase(FriendshipAction.CANCELLED)]
    [TestCase(FriendshipAction.REQUESTED_TO)]
    public void DisplayFriendAction(FriendshipAction friendshipAction)
    {
        const string userId = "test-id-1";
        const int friendCount = 7;
        
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel{userId = userId});
        userProfileBridge.Get(userId).Returns(userProfile);
        view.IsActive().Returns(true);
        friendsController.friendCount.Returns(friendCount);
        view.FriendRequestCount.Returns(5);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(userId, friendshipAction);

        view.Received(1).Set(userId, friendshipAction, Arg.Is<FriendEntryModel>(f => f.userId == userId));
    }

    [Test]
    public void NotificationsAreUpdatedWhenFriendshipActionUpdates()
    {
        const string userId = "test-id-1";
        const int friendCount = 7;
        const int friendRequestCount = 5;
        
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel{userId = userId});
        userProfileBridge.Get(userId).Returns(userProfile);
        view.IsActive().Returns(true);
        friendsController.friendCount.Returns(friendCount);
        view.FriendRequestCount.Returns(friendRequestCount);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(userId, FriendshipAction.APPROVED);

        friendsNotificationService.Received(1).MarkFriendsAsSeen(friendCount);
        friendsNotificationService.Received(1).MarkRequestsAsSeen(friendRequestCount);
        friendsNotificationService.Received(1).UpdateUnseenFriends();
    }
    
    [Test]
    public void NotificationsAreUpdatedWhenIsVisible()
    {
        const int friendCount = 7;
        const int friendRequestCount = 5;
        friendsController.friendCount.Returns(friendCount);
        view.FriendRequestCount.Returns(friendRequestCount);
        
        controller.SetVisibility(true);

        friendsNotificationService.Received(1).MarkFriendsAsSeen(friendCount);
        friendsNotificationService.Received(1).MarkRequestsAsSeen(friendRequestCount);
        friendsNotificationService.Received(1).UpdateUnseenFriends();
    }
}