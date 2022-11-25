using System;
using System.Collections.Generic;
using DCl.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class FriendsControllerShould
    {
        private IFriendsApiBridge apiBridge;
        private FriendsController controller;

        [SetUp]
        public void SetUp()
        {
            apiBridge = Substitute.For<IFriendsApiBridge>();
            controller = new FriendsController(apiBridge);
        }

        [Test]
        public void Initialize()
        {
            var called = false;
            controller.OnInitialized += () => called = true;

            apiBridge.OnInitialized += Raise.Event<Action<FriendshipInitializationMessage>>(
                new FriendshipInitializationMessage
                {
                    totalReceivedRequests = 4
                });

            Assert.AreEqual(4, controller.TotalReceivedFriendRequestCount);
            Assert.IsTrue(called);
        }

        [Test]
        public void AddFriends()
        {
            var updatedFriends = new Dictionary<string, FriendshipAction>();
            var totalFriends = 0;
            controller.OnUpdateFriendship += (s, action) => updatedFriends[s] = action;
            controller.OnTotalFriendsUpdated += i => totalFriends = i;

            apiBridge.OnFriendsAdded += Raise.Event<Action<AddFriendsPayload>>(
                new AddFriendsPayload
                {
                    totalFriends = 2,
                    friends = new[] {"woah", "bleh"}
                });

            Assert.AreEqual(2, totalFriends);
            Assert.AreEqual(FriendshipAction.APPROVED, updatedFriends["woah"]);
            Assert.AreEqual(FriendshipAction.APPROVED, updatedFriends["bleh"]);
        }

        [Test]
        public void AddFriendRequests()
        {
            var totalReceivedCount = 0;
            var totalSentCount = 0;
            var friendsUpdated = new Dictionary<string, FriendshipAction>();
            controller.OnTotalFriendRequestUpdated += (received, sent) =>
            {
                totalReceivedCount = received;
                totalSentCount = sent;
            };
            controller.OnUpdateFriendship += (s, action) => friendsUpdated[s] = action;

            apiBridge.OnFriendRequestsAdded += Raise.Event<Action<AddFriendRequestsPayload>>(
                new AddFriendRequestsPayload
                {
                    totalReceivedFriendRequests = 3,
                    totalSentFriendRequests = 2,
                    requestedFrom = new[] {"rcv1", "rcv2", "rcv3"},
                    requestedTo = new[] {"snt1", "snt2"}
                });

            Assert.AreEqual(FriendshipAction.REQUESTED_TO, friendsUpdated["snt1"]);
            Assert.AreEqual(FriendshipAction.REQUESTED_TO, friendsUpdated["snt2"]);
            Assert.AreEqual(FriendshipAction.REQUESTED_FROM, friendsUpdated["rcv1"]);
            Assert.AreEqual(FriendshipAction.REQUESTED_FROM, friendsUpdated["rcv2"]);
            Assert.AreEqual(FriendshipAction.REQUESTED_FROM, friendsUpdated["rcv3"]);
        }

        [Test]
        public void AddFriendsWithDirectMessages()
        {
            var updatedFriends = new Dictionary<string, FriendshipAction>();
            var friendsWithDMs = new List<FriendWithDirectMessages>();
            controller.OnUpdateFriendship += (s, action) => updatedFriends[s] = action;
            controller.OnAddFriendsWithDirectMessages += list => friendsWithDMs = list;
            
            apiBridge.OnFriendWithDirectMessagesAdded += Raise.Event<Action<AddFriendsWithDirectMessagesPayload>>(
                new AddFriendsWithDirectMessagesPayload
                {
                    totalFriendsWithDirectMessages = 3,
                    currentFriendsWithDirectMessages = new[]
                    {
                        new FriendWithDirectMessages
                        {
                            userId = "usr1",
                            lastMessageBody = "hey",
                            lastMessageTimestamp = 100
                        },
                        new FriendWithDirectMessages
                        {
                            userId = "usr2",
                            lastMessageBody = "yey",
                            lastMessageTimestamp = 200
                        },
                        new FriendWithDirectMessages
                        {
                            userId = "usr3",
                            lastMessageBody = "woh",
                            lastMessageTimestamp = 300
                        }
                    }
                });
            
            Assert.AreEqual(FriendshipAction.APPROVED, updatedFriends["usr1"]);
            Assert.AreEqual(FriendshipAction.APPROVED, updatedFriends["usr2"]);
            Assert.AreEqual(FriendshipAction.APPROVED, updatedFriends["usr3"]);
            Assert.AreEqual("usr1", friendsWithDMs[0].userId);
            Assert.AreEqual("hey", friendsWithDMs[0].lastMessageBody);
            Assert.AreEqual(100, friendsWithDMs[0].lastMessageTimestamp);
            Assert.AreEqual("usr2", friendsWithDMs[1].userId);
            Assert.AreEqual("yey", friendsWithDMs[1].lastMessageBody);
            Assert.AreEqual(200, friendsWithDMs[1].lastMessageTimestamp);
            Assert.AreEqual("usr3", friendsWithDMs[2].userId);
            Assert.AreEqual("woh", friendsWithDMs[2].lastMessageBody);
            Assert.AreEqual(300, friendsWithDMs[2].lastMessageTimestamp);
        }

        [Test]
        public void UpdateUserPresence()
        {
            var updatedFriends = new Dictionary<string, UserStatus>();
            controller.OnUpdateUserStatus += (s, status) => updatedFriends[s] = status;
            apiBridge.OnFriendsAdded += Raise.Event<Action<AddFriendsPayload>>(
                new AddFriendsPayload
                {
                    totalFriends = 7,
                    friends = new[] {"usr1"}
                });

            apiBridge.OnUserPresenceUpdated += Raise.Event<Action<UserStatus>>(new UserStatus
            {
                position = new Vector2(32, -75),
                friendshipStatus = FriendshipStatus.FRIEND,
                presence = PresenceStatus.ONLINE,
                realm = new UserStatus.Realm
                {
                    layer = "idk",
                    serverName = "srv"
                },
                userId = "usr1"
            });
            
            Assert.AreEqual("usr1", updatedFriends["usr1"].userId);
            Assert.AreEqual(FriendshipStatus.FRIEND, updatedFriends["usr1"].friendshipStatus);
            Assert.AreEqual(PresenceStatus.ONLINE, updatedFriends["usr1"].presence);
            Assert.AreEqual(32, updatedFriends["usr1"].position.x);
            Assert.AreEqual(-75, updatedFriends["usr1"].position.y);
            Assert.AreEqual("idk", updatedFriends["usr1"].realm.layer);
            Assert.AreEqual("srv", updatedFriends["usr1"].realm.serverName);
        }

        [TestCase(FriendshipAction.NONE)]
        [TestCase(FriendshipAction.DELETED)]
        [TestCase(FriendshipAction.APPROVED)]
        [TestCase(FriendshipAction.REJECTED)]
        [TestCase(FriendshipAction.CANCELLED)]
        [TestCase(FriendshipAction.REQUESTED_TO)]
        [TestCase(FriendshipAction.REQUESTED_FROM)]
        public void UpdateUserStatus(FriendshipAction expectedAction)
        {
            var updatedUsers = new Dictionary<string, FriendshipAction>();
            controller.OnUpdateFriendship += (s, action) => updatedUsers[s] = action;
            
            apiBridge.OnFriendshipStatusUpdated += Raise.Event<Action<FriendshipUpdateStatusMessage>>(
                new FriendshipUpdateStatusMessage
                {
                    action = expectedAction,
                    userId = "usr1"
                });
            
            Assert.AreEqual(expectedAction, updatedUsers["usr1"]);
        }

        [Test]
        public void UpdateTotalFriendRequestCount()
        {
            var totalReceived = 0;
            var totalSent = 0;
            controller.OnTotalFriendRequestUpdated += (received, sent) =>
            {
                totalReceived = received;
                totalSent = sent;
            }; 
            
            apiBridge.OnTotalFriendRequestCountUpdated += Raise.Event<Action<UpdateTotalFriendRequestsPayload>>(
                new UpdateTotalFriendRequestsPayload
                {
                    totalReceivedRequests = 3,
                    totalSentRequests = 7
                });
            
            Assert.AreEqual(3, controller.TotalReceivedFriendRequestCount);
            Assert.AreEqual(7, controller.TotalSentFriendRequestCount);
            Assert.AreEqual(3, totalReceived);
            Assert.AreEqual(7, totalSent);
        }

        [Test]
        public void UpdateTotalFriendCount()
        {
            var totalFriendCount = 0;
            controller.OnTotalFriendsUpdated += i => totalFriendCount = i; 
            
            apiBridge.OnTotalFriendCountUpdated += Raise.Event<Action<UpdateTotalFriendsPayload>>(
                new UpdateTotalFriendsPayload
                {
                    totalFriends = 8
                });
            
            Assert.AreEqual(8, controller.TotalFriendCount);
            Assert.AreEqual(8, totalFriendCount);
        }
    }
}