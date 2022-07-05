using System;
using System.Collections.Generic;
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
    private UserProfile ownProfile;
    private UserProfile otherUserProfile;

    [SetUp]
    public void SetUp()
    {
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        otherUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        otherUserProfile.UpdateData(new UserProfileModel {userId = OTHER_USER_ID, name = OTHER_USER_NAME});
        userProfileBridge.Get(OTHER_USER_ID).Returns(otherUserProfile);
        userProfileBridge.GetByName(OTHER_USER_NAME).Returns(otherUserProfile);
        ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownProfile);
        friendsController = Substitute.For<IFriendsController>();
        friendsController.AllocatedFriendCount.Returns(FRIENDS_COUNT);
        dataStore = new DataStore();
        controller = new FriendsHUDController(dataStore,
            friendsController,
            userProfileBridge,
            socialAnalytics,
            Substitute.For<IChatController>());
        view = Substitute.For<IFriendsHUDComponentView>();
        view.FriendRequestCount.Returns(FRIEND_REQUEST_SHOWN);
        controller.Initialize(view);
    }

    [TearDown]
    public void TearDown()
    {
        controller.Dispose();
        UnityEngine.Object.Destroy(ownProfile);
        UnityEngine.Object.Destroy(otherUserProfile);
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
        socialAnalytics.Received(1)
            .SendFriendRequestSent(OWN_USER_ID, OTHER_USER_NAME, 0, PlayerActionSource.FriendsHUD);
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

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_TO,
            Arg.Is<FriendRequestEntryModel>(f => f.isReceived == false));
    }

    [Test]
    public void DisplayFriendActionWhenReceivedRequest()
    {
        view.FriendRequestCount.Returns(5);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM);

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_FROM,
            Arg.Is<FriendRequestEntryModel>(f => f.isReceived == true));
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
        friendsController.TotalFriendRequestCount.Returns(FRIEND_REQUEST_SHOWN);
        view.FriendCount.Returns(FRIENDS_COUNT);
        view.IsActive().Returns(true);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

        Assert.AreEqual(FRIENDS_COUNT, dataStore.friendNotifications.seenFriends.Get());
        Assert.AreEqual(FRIEND_REQUEST_SHOWN, dataStore.friendNotifications.pendingFriendRequestCount.Get());
    }

    [Test]
    public void NotificationsAreUpdatedWhenIsVisible()
    {
        friendsController.TotalFriendRequestCount.Returns(FRIEND_REQUEST_SHOWN);
        view.IsActive().Returns(true);
        view.FriendCount.Returns(FRIENDS_COUNT);

        controller.SetVisibility(true);

        Assert.AreEqual(FRIENDS_COUNT, dataStore.friendNotifications.seenFriends.Get());
        Assert.AreEqual(FRIEND_REQUEST_SHOWN, dataStore.friendNotifications.pendingFriendRequestCount.Get());
    }

    [Test]
    public void DisplayFriendWhenTooManyEntriesButIsAlreadyShown()
    {
        view.ContainsFriend(OTHER_USER_ID).Returns(true);
        view.FriendCount.Returns(10000);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.APPROVED,
            Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
    }

    [Test]
    public void DisplayRequestWhenTooManyEntriesButIsAlreadyShown()
    {
        view.ContainsFriendRequest(OTHER_USER_ID).Returns(true);
        view.FriendRequestCount.Returns(10000);

        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.REQUESTED_TO);

        view.Received(1).Set(OTHER_USER_ID, FriendshipAction.REQUESTED_TO,
            Arg.Is<FriendRequestEntryModel>(f => f.userId == OTHER_USER_ID));
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

        view.Received(1).Set(OTHER_USER_ID, FriendshipStatus.FRIEND,
            Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
    }

    [Test]
    public void UpdateBlockStatus()
    {
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

        ownProfile.UpdateData(new UserProfileModel
        {
            userId = OWN_USER_ID,
            blocked = new List<string> {OTHER_USER_ID}
        });
        
        view.Received(1).Populate(OTHER_USER_ID, Arg.Is<FriendEntryModel>(f => f.blocked));
        
        ownProfile.UpdateData(new UserProfileModel
        {
            userId = OWN_USER_ID,
            blocked = null
        });
        
        view.Received(2).Populate(OTHER_USER_ID, Arg.Is<FriendEntryModel>(f => !f.blocked));
    }

    [Test]
    public void DisplayFriendProfileChanges()
    {
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);
        
        otherUserProfile.UpdateData(new UserProfileModel
        {
            userId = OTHER_USER_ID,
            name = "hehe"
        });
        
        view.Received(1).Populate(OTHER_USER_ID, Arg.Is<FriendEntryModel>(f => f.userName == "hehe"));
    }

    [TestCase(0)]
    [TestCase(7)]
    public void GetFriendsWhenBecomesVisible(int friendCount)
    {
        view.IsFriendListActive.Returns(true);
        view.FriendCount.Returns(friendCount);
        friendsController.IsInitialized.Returns(true);
        
        controller.SetVisibility(true);
        
        friendsController.Received(1).GetFriendsAsync(30, friendCount);
    }
    
    [Test]
    public void GetFriendsWhenSwitchesTabs()
    {
        friendsController.IsInitialized.Returns(true);

        view.OnFriendListDisplayed += Raise.Event<Action>();
        
        friendsController.Received(1).GetFriendsAsync(30, 0);
    }
    
    [Test]
    public void GetFriendRequestsWhenBecomesVisible()
    {
        view.IsRequestListActive.Returns(true);
        friendsController.IsInitialized.Returns(true);
        
        controller.SetVisibility(true);
        
        friendsController.Received(1).GetFriendRequestsAsync(30, Arg.Any<long>(), 30, Arg.Any<long>());
    }
    
    [Test]
    public void GetFriendRequestsWhenSwitchesTabs()
    {
        friendsController.IsInitialized.Returns(true);
        view.FriendRequestCount.Returns(0);

        view.OnRequestListDisplayed += Raise.Event<Action>();
        
        friendsController.Received(1).GetFriendRequestsAsync(30, Arg.Any<long>(), 30, Arg.Any<long>());
    }

    [Test]
    public void HideMoreFriendsToLoadWhenReachedTotalFriends()
    {
        friendsController.TotalFriendCount.Returns(7);
        view.FriendCount.Returns(7);
        view.IsFriendListActive.Returns(true);
        friendsController.IsInitialized.Returns(true);
        view.ClearReceivedCalls();
        
        controller.SetVisibility(true);
        
        view.Received(1).HideMoreFriendsToLoadHint();
    }
    
    [Test]
    public void ShowMoreFriendsToLoadWhenMissingFriends()
    {
        friendsController.TotalFriendCount.Returns(7);
        view.FriendCount.Returns(3);
        view.IsFriendListActive.Returns(true);
        friendsController.IsInitialized.Returns(true);
        view.ClearReceivedCalls();
        
        controller.SetVisibility(true);
        
        view.Received(1).ShowMoreFriendsToLoadHint(4);
    }
    
    [Test]
    public void HideMoreFriendRequestsToLoadWhenReachedTotalFriends()
    {
        friendsController.TotalFriendRequestCount.Returns(16);
        view.FriendRequestCount.Returns(16);
        view.IsRequestListActive.Returns(true);
        view.ClearReceivedCalls();
        friendsController.IsInitialized.Returns(true);
        
        controller.SetVisibility(true);
        
        view.Received(1).HideMoreRequestsToLoadHint();
    }
    
    [Test]
    public void ShowMoreFriendRequestsToLoadWhenMissingRequests()
    {
        friendsController.TotalFriendRequestCount.Returns(17);
        view.FriendRequestCount.Returns(8);
        view.IsRequestListActive.Returns(true);
        view.ClearReceivedCalls();
        friendsController.IsInitialized.Returns(true);
        
        controller.SetVisibility(true);
        
        view.Received(1).ShowMoreRequestsToLoadHint(9);
    }

    [Test]
    public void GetMoreFriends()
    {
        friendsController.IsInitialized.Returns(true);
        view.OnRequireMoreFriends += Raise.Event<Action>();
        
        friendsController.GetFriendsAsync(30, 0);
    }
    
    [TestCase(3)]
    [TestCase(11)]
    public void GetMoreFriendsWhenViewRequests(int friendCount)
    {
        friendsController.IsInitialized.Returns(true);
        view.FriendCount.Returns(friendCount);
        
        view.OnRequireMoreFriends += Raise.Event<Action>();
        
        friendsController.Received(1).GetFriendsAsync(30, friendCount);
    }
    
    [Test]
    public void GetMoreFriendRequests()
    {
        friendsController.IsInitialized.Returns(true);
        view.OnRequireMoreFriendRequests += Raise.Event<Action>();
        
        friendsController.GetFriendRequestsAsync(30, 0, 30, 0);
    }

    [Test]
    public void UpdatePendingRequestCountToDatastoreWhenFriendsInitializes()
    {
        friendsController.TotalFriendRequestCount.Returns(87);
        friendsController.OnInitialized += Raise.Event<Action>();

        Assert.AreEqual(87, dataStore.friendNotifications.pendingFriendRequestCount.Get());
    }

    [TestCase("bleh", 0)]
    [TestCase(OTHER_USER_NAME, 1)]
    public void SearchFriends(string searchText, int expectedCount)
    {
        friendsController.GetUserStatus(OTHER_USER_ID).Returns(new FriendsController.UserStatus
        {
            friendshipStatus = FriendshipStatus.FRIEND,
            userId = OTHER_USER_ID
        });
        friendsController.OnUpdateFriendship +=
            Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);
        
        view.OnSearchFriendsRequested += Raise.Event<Action<string>>(searchText);
        
        friendsController.Received(1).GetFriendsAsync(searchText, 100);
        view.Received(1).FilterFriends(Arg.Is<Dictionary<string, FriendEntryModel>>(d => d.Count == expectedCount));
    }

    [Test]
    public void ClearFriendFilterWhenSearchForAnEmptyString()
    {
        view.OnSearchFriendsRequested += Raise.Event<Action<string>>("");
        
        view.Received(1).ClearFriendFilter();
    }
}