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
    private const string OTHER_USER_ID = "test-id-1";
    private const string OTHER_USER_NAME = "woah";
    private const int FRIENDS_COUNT = 7;
    private const int FRIEND_REQUEST_SHOWN = 5;

    private FriendsHUDController controller;
    private IFriendsHUDComponentView view;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private IUserProfileBridge userProfileBridge;
    private DataStore dataStore;

    [SetUp]
    public void SetUp()
    {
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var otherUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        otherUserProfile.UpdateData(new UserProfileModel {userId = OTHER_USER_ID, name = OTHER_USER_NAME});
        userProfileBridge.Get(OTHER_USER_ID).Returns(otherUserProfile);
        userProfileBridge.GetByName(OTHER_USER_NAME).Returns(otherUserProfile);
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownProfile);
        friendsController = Substitute.For<IFriendsController>();
        friendsController.friendCount.Returns(FRIENDS_COUNT);
        dataStore = new DataStore();
        controller = new FriendsHUDController(dataStore,
            friendsController,
            userProfileBridge,
            socialAnalytics,
            Substitute.For<IChatController>(),
            Substitute.For<ILastReadMessagesService>());
        view = Substitute.For<IFriendsHUDComponentView>();
        view.FriendRequestCount.Returns(FRIEND_REQUEST_SHOWN);
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
        friendsController.ContainsStatus(OTHER_USER_ID, FriendshipStatus.FRIEND).Returns(false);

        view.OnFriendRequestSent += Raise.Event<Action<string>>(OTHER_USER_NAME);

        friendsController.Received(1).RequestFriendship(OTHER_USER_NAME);
        socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, OTHER_USER_NAME, 0, PlayerActionSource.FriendsHUD);
        view.Received(1).ShowRequestSendSuccess();
    }

    [Test]
    public void SendFriendRequestByIdCorrectly()
    {
        friendsController.ContainsStatus(OTHER_USER_ID, FriendshipStatus.FRIEND).Returns(false);

        view.OnFriendRequestSent += Raise.Event<Action<string>>(OTHER_USER_ID);

        friendsController.Received(1).RequestFriendship(OTHER_USER_ID);
        socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, OTHER_USER_ID, 0, PlayerActionSource.FriendsHUD);
        view.Received(1).ShowRequestSendSuccess();
    }

    [Test]
    public void FailFriendRequestWhenAlreadyFriends()
    {
        friendsController.ContainsStatus(OTHER_USER_ID, FriendshipStatus.FRIEND).Returns(true);

        view.OnFriendRequestSent += Raise.Event<Action<string>>(OTHER_USER_ID);

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
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, friendshipAction);

        view.Received(1).Set(OTHER_USER_ID, friendshipAction, Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
    }

    [Test]
    public void DisplayFriendActionWhenSentRequest()
    {
        view.FriendRequestCount.Returns(5);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_TO);

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_TO, Arg.Is<FriendRequestEntryModel>(f => f.isReceived == false));
    }
    
    [Test]
    public void DisplayFriendActionWhenReceivedRequest()
    {
        view.FriendRequestCount.Returns(5);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM);

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM, Arg.Is<FriendRequestEntryModel>(f => f.isReceived == true));
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
    public void UpdateUserStatusWhenRequestSent()
    {
        var status = new FriendsController.UserStatus
        {
            position = Vector2.zero,
            presence = PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.REQUESTED_TO,
            realm = null,
            userId = OTHER_USER_ID,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(OTHER_USER_ID, status);

        view.Received(1).Set(OTHER_USER_ID, FriendshipStatus.REQUESTED_TO,
            Arg.Is<FriendRequestEntryModel>(f => f.isReceived == false));
    }
    
    [Test]
    public void UpdateUserStatusWhenRequestReceived()
    {
        var status = new FriendsController.UserStatus
        {
            position = Vector2.zero,
            presence = PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.REQUESTED_FROM,
            realm = null,
            userId = OTHER_USER_ID,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(OTHER_USER_ID, status);

        view.Received(1).Set(OTHER_USER_ID, FriendshipStatus.REQUESTED_FROM,
            Arg.Is<FriendRequestEntryModel>(f => f.isReceived == true));
    }

    [Test]
    public void NotificationsAreUpdatedWhenFriendshipActionUpdates()
    {
        friendsController.ReceivedRequestCount.Returns(FRIEND_REQUEST_SHOWN);
        view.IsActive().Returns(true);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

        Assert.AreEqual(FRIENDS_COUNT, dataStore.friendNotifications.seenFriends.Get());
        Assert.AreEqual(FRIEND_REQUEST_SHOWN, dataStore.friendNotifications.seenRequests.Get());
    }

    [Test]
    public void NotificationsAreUpdatedWhenIsVisible()
    {
        friendsController.ReceivedRequestCount.Returns(FRIEND_REQUEST_SHOWN);
        view.IsActive().Returns(true);

        controller.SetVisibility(true);

        Assert.AreEqual(FRIENDS_COUNT, dataStore.friendNotifications.seenFriends.Get());
        Assert.AreEqual(FRIEND_REQUEST_SHOWN, dataStore.friendNotifications.seenRequests.Get());
    }

    [Test]
    public void EnqueueFriendWhenTooManyEntriesDisplayed()
    {
        view.FriendCount.Returns(10000);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);
        
        view.DidNotReceiveWithAnyArgs().Set(default, (FriendshipAction) default, default);
        view.Received(1).ShowMoreFriendsToLoadHint(2);
    }
    
    [Test]
    public void EnqueueFriendRequestWhenTooManyEntriesDisplayed()
    {
        view.FriendRequestCount.Returns(10000);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM);
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM);
        
        view.DidNotReceiveWithAnyArgs().Set(default, (FriendshipAction) default, default);
        view.Received(1).ShowMoreRequestsToLoadHint(2);
    }

    [Test]
    public void DisplayFriendWhenTooManyEntriesButIsAlreadyShown()
    {
        view.ContainsFriend(OTHER_USER_ID).Returns(true);
        view.FriendCount.Returns(10000);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);
        
        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.APPROVED, Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
    }
    
    [Test]
    public void DisplayRequestWhenTooManyEntriesButIsAlreadyShown()
    {
        view.ContainsFriendRequest(OTHER_USER_ID).Returns(true);
        view.FriendRequestCount.Returns(10000);
        
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_TO);
        
        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_TO, Arg.Is<FriendRequestEntryModel>(f => f.userId == OTHER_USER_ID));
    }
    
    [Test]
    public void AlwaysDisplayOnlineUsersWhenTooManyEntries()
    {
        view.FriendCount.Returns(10000);
        
        var status = new FriendsController.UserStatus
        {
            position = Vector2.zero,
            presence = PresenceStatus.ONLINE,
            friendshipStatus = FriendshipStatus.FRIEND,
            realm = null,
            userId = OTHER_USER_ID,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(OTHER_USER_ID, status);

        view.Received(1).Set(OTHER_USER_ID, FriendshipStatus.FRIEND, Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
    }
    
    [Test]
    public void EnqueueUserStatusWhenTooManyEntries()
    {
        view.FriendCount.Returns(10000);
        
        var status = new FriendsController.UserStatus
        {
            position = Vector2.zero,
            presence = PresenceStatus.OFFLINE,
            friendshipStatus = FriendshipStatus.FRIEND,
            realm = null,
            userId = OTHER_USER_ID,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(OTHER_USER_ID, status);

        view.DidNotReceiveWithAnyArgs().Set(default, (FriendshipStatus) default, default);
        view.Received(1).ShowMoreFriendsToLoadHint(1);
    }
    
    [Test]
    public void EnqueueUserStatusAsRequestWhenTooManyEntries()
    {
        view.FriendRequestCount.Returns(10000);
        
        var status = new FriendsController.UserStatus
        {
            position = Vector2.zero,
            presence = PresenceStatus.OFFLINE,
            friendshipStatus = FriendshipStatus.REQUESTED_FROM,
            realm = null,
            userId = OTHER_USER_ID,
            friendshipStartedTime = DateTime.UtcNow
        };

        friendsController.OnUpdateUserStatus +=
            Raise.Event<Action<string, FriendsController.UserStatus>>(OTHER_USER_ID, status);

        view.DidNotReceiveWithAnyArgs().Set(default, (FriendshipStatus) default, default);
        view.Received(1).ShowMoreRequestsToLoadHint(1);
    }
}