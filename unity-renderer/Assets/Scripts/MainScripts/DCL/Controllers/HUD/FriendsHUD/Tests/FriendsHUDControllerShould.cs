using System;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using DCL;
using DCL.Helpers;
using UnityEngine;

public class FriendsHUDControllerShould
{
    private const string OWN_USER_ID = "my-user";

    private FriendsHUDController controller;
    private IFriendsHUDComponentView view;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private IFriendsNotificationService friendsNotificationService;
    private IUserProfileBridge userProfileBridge;

    [SetUp]
    public void SetUp()
    {
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        friendsNotificationService = Substitute.For<IFriendsNotificationService>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownProfile);
        friendsController = Substitute.For<IFriendsController>();
        controller = new FriendsHUDController(new DataStore(),
            friendsController,
            userProfileBridge,
            socialAnalytics,
            friendsNotificationService);
        view = Substitute.For<IFriendsHUDComponentView>();
        controller.Initialize(view);
    }

    [TearDown]
    public void TearDown()
    {
        controller.Dispose();
    }

    [Test]
    public void ReactCorrectlyToWhisperClick()
    {
        const string id = "test-id-1";

        var pressedWhisper = false;
        controller.OnPressWhisper += x => pressedWhisper = x == id;

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
        userProfile.UpdateData(new UserProfileModel {userId = userId, name = userName});
        userProfileBridge.GetByName(userName).Returns(userProfile);
        userProfileBridge.Get(userId).Returns(userProfile);

        view.OnFriendRequestSent += Raise.Event<Action<string>>(userName);

        friendsController.Received(1).RequestFriendship(userName);
        socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, userName, 0, PlayerActionSource.FriendsHUD);
        view.Received(1).ShowRequestSendSuccess();
    }

    [Test]
    public void SendFriendRequestByIdCorrectly()
    {
        const string userId = "test-id-1";
        const string userName = "waoh";
        friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND).Returns(false);
        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel {userId = userId, name = userName});
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
        userProfile.UpdateData(new UserProfileModel {userId = userId, name = userName});
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
        userProfile.UpdateData(new UserProfileModel {userId = userId});
        userProfileBridge.Get(userId).Returns(userProfile);
        view.IsActive().Returns(true);
        friendsController.friendCount.Returns(friendCount);
        view.FriendRequestCount.Returns(5);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(userId, friendshipAction);

        view.Received(1).Set(userId, friendshipAction, Arg.Is<FriendEntryModel>(f => f.userId == userId));
    }

    [TestCase("test-id-1", 43, 72, PresenceStatus.ONLINE, FriendshipStatus.FRIEND, "rl", "svn")]
    [TestCase("test-id-2", 23, 23, PresenceStatus.OFFLINE, FriendshipStatus.REQUESTED_TO, "rl", "svn")]
    [TestCase("test-id-3", 12, 263, PresenceStatus.ONLINE, FriendshipStatus.REQUESTED_FROM, "rl", "svn")]
    public void UpdateUserStatus(string userId, float positionX, float positionY, PresenceStatus presence,
        FriendshipStatus friendshipStatus,
        string realmLayer, string serverName)
    {
        var position = new Vector2(positionX, positionY);
        var status = new FriendsController.UserStatus
        {
            position = position,
            presence = presence,
            friendshipStatus = friendshipStatus,
            realm = new FriendsController.UserStatus.Realm {layer = realmLayer, serverName = serverName},
            userId = userId,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(userId, status);

        view.Received(1).Set(userId, friendshipStatus, Arg.Is<FriendEntryModel>(f => f.blocked == false
            && f.coords.Equals(position)
            && f.realm == $"{serverName.ToUpperFirst()} {realmLayer.ToUpperFirst()}"
            && f.status == presence
            && f.userId == userId));
    }

    [Test]
    public void NotificationsAreUpdatedWhenFriendshipActionUpdates()
    {
        const string userId = "test-id-1";
        const int friendCount = 7;
        const int friendRequestCount = 5;

        var userProfile = ScriptableObject.CreateInstance<UserProfile>();
        userProfile.UpdateData(new UserProfileModel {userId = userId});
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
        view.IsActive().Returns(true);

        controller.SetVisibility(true);

        friendsNotificationService.Received(1).MarkFriendsAsSeen(friendCount);
        friendsNotificationService.Received(1).MarkRequestsAsSeen(friendRequestCount);
        friendsNotificationService.Received(1).UpdateUnseenFriends();
    }
}