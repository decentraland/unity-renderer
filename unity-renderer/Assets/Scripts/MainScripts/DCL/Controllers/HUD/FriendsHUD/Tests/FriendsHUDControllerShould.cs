using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCl.Social.Friends
{
    public class FriendsHUDControllerShould
    {
        private const string OWN_USER_ID = "my-user";
        private const string OTHER_USER_ID = "0x33e8c8a39b71d7a002d5037de1be4de8f0a6a358";
        private const string OTHER_USER_NAME = "woah";
        private const int FRIENDS_COUNT = 7;
        private const int FRIEND_REQUEST_SHOWN = 5;
        private const string FRIEND_REQUEST_ID = "requestId";

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
            otherUserProfile.UpdateData(new UserProfileModel { userId = OTHER_USER_ID, name = OTHER_USER_NAME });
            userProfileBridge.Get(OTHER_USER_ID).Returns(otherUserProfile);
            userProfileBridge.GetByName(OTHER_USER_NAME, Arg.Any<bool>()).Returns(otherUserProfile);
            ownProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownProfile.UpdateData(new UserProfileModel { userId = OWN_USER_ID });
            userProfileBridge.GetOwn().Returns(ownProfile);
            friendsController = Substitute.For<IFriendsController>();
            friendsController.AllocatedFriendCount.Returns(FRIENDS_COUNT);
            friendsController.GetFriendsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(UniTask.FromResult(new string[0]));
            friendsController.GetFriendsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(UniTask.FromResult((IReadOnlyList<string>)Array.Empty<string>()));
            dataStore = new DataStore();

            controller = new FriendsHUDController(dataStore,
                friendsController,
                userProfileBridge,
                socialAnalytics,
                Substitute.For<IChatController>(),
                Substitute.For<IMouseCatcher>());

            view = Substitute.For<IFriendsHUDComponentView>();
            view.FriendRequestCount.Returns(FRIEND_REQUEST_SHOWN);
            controller.Initialize(view, isVisible: false);

            // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["new_friend_requests"] = true } });
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            Object.Destroy(ownProfile);
            Object.Destroy(otherUserProfile);
        }

        [Test]
        public void ReactCorrectlyToWhisperClick()
        {
            const string id = "test-id-1";

            var pressedWhisper = false;
            controller.OnPressWhisper += x => pressedWhisper = x == id;

            view.OnWhisper += Raise.Event<Action<FriendEntryModel>>(new FriendEntryModel { userId = id });

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
            friendsController.RequestFriendshipAsync(OTHER_USER_ID, "", Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(
                                  new FriendRequest(FRIEND_REQUEST_ID, new DateTime(100), OWN_USER_ID, OTHER_USER_ID, "")));

            friendsController.ContainsStatus(OTHER_USER_ID, FriendshipStatus.FRIEND).Returns(false);

            view.OnFriendRequestSent += Raise.Event<Action<string>>(OTHER_USER_NAME);

            friendsController.Received(1).RequestFriendshipAsync(OTHER_USER_ID, "", Arg.Any<CancellationToken>());

            socialAnalytics.Received(1)
                           .SendFriendRequestSent(OWN_USER_ID, OTHER_USER_ID, 0, PlayerActionSource.FriendsHUD, FRIEND_REQUEST_ID);

            view.Received(1).ShowRequestSendSuccess();
        }

        [Test]
        public void SendFriendRequestByIdCorrectly()
        {
            friendsController.RequestFriendshipAsync(OTHER_USER_ID, "", Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(
                                  new FriendRequest(FRIEND_REQUEST_ID, new DateTime(100), OWN_USER_ID, OTHER_USER_ID, "")));

            friendsController.ContainsStatus(OTHER_USER_ID, FriendshipStatus.FRIEND).Returns(false);

            view.OnFriendRequestSent += Raise.Event<Action<string>>(OTHER_USER_ID);

            friendsController.Received(1).RequestFriendshipAsync(OTHER_USER_ID, "", Arg.Any<CancellationToken>());
            socialAnalytics.Received(1).SendFriendRequestSent(OWN_USER_ID, OTHER_USER_ID, 0, PlayerActionSource.FriendsHUD, FRIEND_REQUEST_ID);
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
        public void DisplayFriend(FriendshipAction friendshipAction)
        {
            friendsController.OnUpdateFriendship +=
                Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, friendshipAction);

            view.Received(1).Set(OTHER_USER_ID, Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
        }

        [TestCase(FriendshipAction.DELETED)]
        [TestCase(FriendshipAction.REJECTED)]
        [TestCase(FriendshipAction.CANCELLED)]
        public void RemoveFriend(FriendshipAction friendshipAction)
        {
            friendsController.OnUpdateFriendship +=
                Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, friendshipAction);

            view.Received(1).Remove(OTHER_USER_ID);
        }

        [Test]
        public void DisplayFriendActionWhenSentRequest()
        {
            view.FriendRequestCount.Returns(5);

            friendsController.IsInitialized.Returns(true);

            friendsController
               .GetFriendRequestsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(UniTask.FromResult((IReadOnlyList<FriendRequest>)new List<FriendRequest> { new FriendRequest("test", new DateTime(0), OWN_USER_ID, OTHER_USER_ID, "test message") }));

            view.OnRequireMoreFriendRequests += Raise.Event<Action>();

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendRequestEntryModel>(f => f.isReceived == false));
        }

        [Test]
        public void DisplayReceivedRequestWhenInitializes()
        {
            view.FriendRequestCount.Returns(5);

            friendsController.IsInitialized.Returns(true);

            friendsController
               .GetFriendRequestsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(UniTask.FromResult((IReadOnlyList<FriendRequest>)new List<FriendRequest> { new FriendRequest("test", new DateTime(0), OTHER_USER_ID, OWN_USER_ID, "test message") }));

            view.OnRequireMoreFriendRequests += Raise.Event<Action>();

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendRequestEntryModel>(f => f.isReceived == true));
        }

        [TestCase("test-id-1", 43, 72, PresenceStatus.ONLINE, FriendshipStatus.FRIEND, "rl", "svn")]
        public void UpdateFriendUserStatus(string userId, float positionX, float positionY, PresenceStatus presence,
            FriendshipStatus friendshipStatus,
            string realmLayer, string serverName)
        {
            var position = new Vector2(positionX, positionY);

            var status = new UserStatus
            {
                position = position,
                presence = presence,
                friendshipStatus = friendshipStatus,
                realm = new UserStatus.Realm { layer = realmLayer, serverName = serverName },
                userId = userId
            };

            friendsController.OnUpdateUserStatus +=
                Raise.Event<Action<string, UserStatus>>(userId, status);

            view.Received(1)
                .Set(userId,
                     Arg.Is<FriendEntryModel>(f => f.blocked == false
                                                   && f.coords.Equals(position)
                                                   && f.realm ==
                                                   $"{serverName.ToUpperFirst()} {realmLayer.ToUpperFirst()}"
                                                   && f.status == presence
                                                   && f.userId == userId));
        }

        [Test]
        public void UpdateUserStatusWhenRequestSent()
        {
            friendsController.OnFriendRequestReceived +=
                Raise.Event<Action<FriendRequest>>(new FriendRequest("test", new DateTime(0), OWN_USER_ID, OTHER_USER_ID, "test"));

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendRequestEntryModel>(f => f.isReceived == false));
        }

        [Test]
        public void UpdateUserStatusWhenRequestReceived()
        {
            friendsController.OnFriendRequestReceived +=
                Raise.Event<Action<FriendRequest>>(new FriendRequest("test", new DateTime(0), OTHER_USER_ID, OWN_USER_ID, "test"));

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendRequestEntryModel>(f => f.isReceived == true));
        }

        [Test]
        public void NotificationsAreUpdatedWhenFriendshipActionUpdates()
        {
            friendsController.ReceivedRequestCount.Returns(FRIEND_REQUEST_SHOWN);
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
            friendsController.ReceivedRequestCount.Returns(FRIEND_REQUEST_SHOWN);
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

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
        }

        [Test]
        public void DisplayRequestWhenTooManyEntriesButIsAlreadyShown()
        {
            view.ContainsFriendRequest(OTHER_USER_ID).Returns(true);
            view.FriendRequestCount.Returns(10000);

            friendsController.IsInitialized.Returns(true);

            friendsController
               .GetFriendRequestsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(UniTask.FromResult((IReadOnlyList<FriendRequest>)new List<FriendRequest> { new FriendRequest("test", new DateTime(0), OTHER_USER_ID, OWN_USER_ID, "test message") }));

            view.OnRequireMoreFriendRequests += Raise.Event<Action>();

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendRequestEntryModel>(f => f.userId == OTHER_USER_ID));
        }

        [Test]
        public void AlwaysDisplayOnlineUsersWhenTooManyEntries()
        {
            view.FriendCount.Returns(10000);

            var status = new UserStatus
            {
                position = Vector2.zero,
                presence = PresenceStatus.ONLINE,
                friendshipStatus = FriendshipStatus.FRIEND,
                realm = null,
                userId = OTHER_USER_ID
            };

            friendsController.OnUpdateUserStatus +=
                Raise.Event<Action<string, UserStatus>>(OTHER_USER_ID, status);

            view.Received(1)
                .Set(OTHER_USER_ID,
                     Arg.Is<FriendEntryModel>(f => f.userId == OTHER_USER_ID));
        }

        [Test]
        public void UpdateBlockStatus()
        {
            friendsController.OnUpdateFriendship +=
                Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

            view.ClearReceivedCalls();

            ownProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                blocked = new List<string> { OTHER_USER_ID }
            });

            view.Received(1).UpdateBlockStatus(OTHER_USER_ID, true);

            view.ClearReceivedCalls();

            ownProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                blocked = null
            });

            view.Received(1).UpdateBlockStatus(OTHER_USER_ID, false);
        }

        [Test]
        public void DisplayFriendProfileChanges()
        {
            friendsController.GetUserStatus(OTHER_USER_ID)
                             .Returns(new UserStatus
                              {
                                  userId = OTHER_USER_ID,
                                  friendshipStatus = FriendshipStatus.FRIEND
                              });

            friendsController.OnUpdateFriendship +=
                Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

            view.ClearReceivedCalls();

            otherUserProfile.UpdateData(new UserProfileModel
            {
                userId = OTHER_USER_ID,
                name = "hehe"
            });

            view.Received(1).Set(OTHER_USER_ID, Arg.Is<FriendEntryModel>(f => f.userName == "hehe"));
        }

        [Test]
        public void GetFriendsWhenBecomesVisible()
        {
            view.IsFriendListActive.Returns(true);
            friendsController.IsInitialized.Returns(true);

            controller.SetVisibility(true);

            friendsController.Received(1).GetFriendsAsync(30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetFriendsWhenSwitchesTabs()
        {
            friendsController.IsInitialized.Returns(true);

            view.OnFriendListDisplayed += Raise.Event<Action>();

            friendsController.Received(1).GetFriendsAsync(30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetFriendRequestsWhenBecomesVisible()
        {
            view.FriendRequestReceivedCount.Returns(0);
            view.FriendRequestSentCount.Returns(0);
            view.IsRequestListActive.Returns(true);
            friendsController.IsInitialized.Returns(true);

            controller.SetVisibility(true);

            friendsController.Received(1).GetFriendRequestsAsync(30, 0, 30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetFriendRequestsWhenSwitchesTabs()
        {
            friendsController.IsInitialized.Returns(true);
            view.FriendRequestCount.Returns(0);

            view.OnRequestListDisplayed += Raise.Event<Action>();

            friendsController.Received(1).GetFriendRequestsAsync(30, 0, 30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void HideMoreFriendsToLoadWhenReachedTotalFriends()
        {
            friendsController.TotalFriendCount.Returns(7);
            view.IsFriendListActive.Returns(true);
            friendsController.IsInitialized.Returns(true);
            view.ClearReceivedCalls();

            controller.SetVisibility(true);

            view.Received(1).HideMoreFriendsToLoadHint();
        }

        [Test]
        public void ShowMoreFriendsToLoadWhenMissingFriends()
        {
            friendsController.TotalFriendCount.Returns(34);
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
            view.IsRequestListActive.Returns(true);
            friendsController.IsInitialized.Returns(true);
            view.ClearReceivedCalls();

            controller.SetVisibility(true);

            view.Received(1).HideMoreRequestsToLoadHint();
        }

        [Test]
        public void ShowMoreFriendRequestsToLoadWhenMissingRequests()
        {
            friendsController.TotalFriendRequestCount.Returns(39);
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

            friendsController.Received(1).GetFriendsAsync(30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetMoreFriendsWhenViewRequests()
        {
            view.IsFriendListActive.Returns(true);
            friendsController.IsInitialized.Returns(true);
            controller.SetVisibility(true);

            view.OnRequireMoreFriends += Raise.Event<Action>();

            friendsController.Received(1).GetFriendsAsync(30, 30, Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetMoreFriendRequests()
        {
            friendsController.IsInitialized.Returns(true);
            view.OnRequireMoreFriendRequests += Raise.Event<Action>();

            friendsController.GetFriendRequestsAsync(30, 0, 30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void UpdatePendingRequestCountToDatastoreWhenFriendsInitializes()
        {
            friendsController.ReceivedRequestCount.Returns(87);
            friendsController.OnInitialized += Raise.Event<Action>();

            Assert.AreEqual(87, dataStore.friendNotifications.pendingFriendRequestCount.Get());
        }

        [TestCase("bleh", 0)]
        [TestCase(OTHER_USER_NAME, 1)]
        public void SearchFriends(string searchText, int expectedCount)
        {
            friendsController.GetUserStatus(OTHER_USER_ID)
                             .Returns(new UserStatus
                              {
                                  friendshipStatus = FriendshipStatus.FRIEND,
                                  userId = OTHER_USER_ID
                              });

            friendsController.OnUpdateFriendship +=
                Raise.Event<Action<string, FriendshipAction>>(OTHER_USER_ID, FriendshipAction.APPROVED);

            view.OnSearchFriendsRequested += Raise.Event<Action<string>>(searchText);

            friendsController.Received(1).GetFriendsAsync(searchText, 100, Arg.Any<CancellationToken>());
            view.Received(1).EnableSearchMode();
        }

        [Test]
        public void ClearFriendFilterWhenSearchForAnEmptyString()
        {
            view.OnSearchFriendsRequested += Raise.Event<Action<string>>("");

            view.Received(1).DisableSearchMode();
        }

        [Test]
        public void LoadFriendsWhenBecomesVisible()
        {
            friendsController.IsInitialized.Returns(true);
            view.IsActive().Returns(true);
            view.IsFriendListActive.Returns(true);
            controller.SetVisibility(true);
            controller.SetVisibility(false);
            controller.SetVisibility(true);

            friendsController.Received(2).GetFriendsAsync(30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void LoadFriendsRequestsWhenBecomesVisible()
        {
            friendsController.IsInitialized.Returns(true);
            view.IsActive().Returns(true);
            view.IsRequestListActive.Returns(true);
            controller.SetVisibility(true);
            controller.SetVisibility(false);
            controller.SetVisibility(true);

            friendsController.Received(2).GetFriendRequestsAsync(30, 0, 30, 0, Arg.Any<CancellationToken>());
        }

        [Test]
        public void OpenCancelFriendRequestDetails()
        {
            const string FRIEND_REQUEST_ID = "friendRequestId";

            friendsController.GetAllocatedFriendRequestByUser(OTHER_USER_ID)
                             .Returns(new FriendRequest(FRIEND_REQUEST_ID,
                                  new DateTime(100), OWN_USER_ID, OTHER_USER_ID, "hey"));

            view.OnFriendRequestOpened += Raise.Event<Action<string>>(OTHER_USER_ID);

            Assert.AreEqual(FRIEND_REQUEST_ID, dataStore.HUDs.openSentFriendRequestDetail.Get());
        }
    }
}
