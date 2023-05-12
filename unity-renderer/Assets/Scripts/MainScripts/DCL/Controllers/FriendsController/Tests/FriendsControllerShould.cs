using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using MainScripts.DCL.Controllers.FriendsController;
using NSubstitute;
using NUnit.Framework;
using rpc_csharp.transport;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class FriendsControllerShould
    {
        private IFriendsApiBridge apiBridge;
        private ISocialApiBridge rpcSocialApiBridge;
        private FriendsController controller;

        [SetUp]
        public void SetUp()
        {
            apiBridge = Substitute.For<IFriendsApiBridge>();
            GameObject go = new GameObject();
            var component = go.AddComponent<MatrixInitializationBridge>();
            var dataStore = new DataStore();
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = false } });

            rpcSocialApiBridge = Substitute.For<ISocialApiBridge>();

            controller = new FriendsController(apiBridge, rpcSocialApiBridge, dataStore);

            dataStore.featureFlags.flags.Get().SetAsInitialized();
            controller.Initialize();
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
            _ = apiBridge.GetFriendsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(
                              new AddFriendsPayload
                              {
                                  totalFriends = 2,
                                  friends = new[] { "woah", "bleh" }
                              }));

            controller.GetFriendsAsync(0, 0, new CancellationToken()).Forget();

            Assert.AreEqual(2, controller.TotalFriendCount);
            var updatedFriends = controller.GetAllocatedFriends();
            Assert.AreEqual(FriendshipStatus.FRIEND, updatedFriends["woah"].friendshipStatus);
            Assert.AreEqual(FriendshipStatus.FRIEND, updatedFriends["bleh"].friendshipStatus);
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

            apiBridge.GetFriendRequestsAsync(0, 0, 0, 0, Arg.Any<CancellationToken>())
                     .Returns(UniTask.FromResult(
                          new AddFriendRequestsV2Payload
                          {
                              totalReceivedFriendRequests = 3,
                              totalSentFriendRequests = 2,
                              requestedFrom = new FriendRequestPayload[]
                              {
                                  new FriendRequestPayload
                                  {
                                      friendRequestId = "test1",
                                      from = "rcv1",
                                      to = "me",
                                      messageBody = "",
                                      timestamp = 0
                                  },
                                  new FriendRequestPayload
                                  {
                                      friendRequestId = "test2",
                                      from = "rcv2",
                                      to = "me",
                                      messageBody = "",
                                      timestamp = 0
                                  },
                                  new FriendRequestPayload
                                  {
                                      friendRequestId = "test3",
                                      from = "rcv3",
                                      to = "me",
                                      messageBody = "",
                                      timestamp = 0
                                  }
                              },
                              requestedTo = new FriendRequestPayload[]
                              {
                                  new FriendRequestPayload
                                  {
                                      friendRequestId = "test1",
                                      from = "me",
                                      to = "snt1",
                                      messageBody = "",
                                      timestamp = 0
                                  },
                                  new FriendRequestPayload
                                  {
                                      friendRequestId = "test2",
                                      from = "me",
                                      to = "snt2",
                                      messageBody = "",
                                      timestamp = 0
                                  }
                              }
                          }));

            controller.GetFriendRequestsAsync(0, 0, 0, 0, default(CancellationToken)).Forget();

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

            _ = apiBridge.GetFriendsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(
                              new AddFriendsPayload
                              {
                                  totalFriends = 7,
                                  friends = new[] { "usr1" }
                              }));

            controller.GetFriendsAsync(0, 0, new CancellationToken()).Forget();

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
            apiBridge.OnTotalFriendCountUpdated += Raise.Event<Action<UpdateTotalFriendsPayload>>(
                new UpdateTotalFriendsPayload
                {
                    totalFriends = 8
                });

            Assert.AreEqual(8, controller.TotalFriendCount);
        }

        [Test]
        public void AllocateWhenReceiveFriendRequest()
        {
            apiBridge.OnFriendRequestReceived += Raise.Event<Action<FriendRequestPayload>>(new FriendRequestPayload
            {
                from = "senderId",
                timestamp = 100,
                to = "ownId",
                messageBody = "hey!",
                friendRequestId = "fr1",
            });

            FriendRequest request = controller.GetAllocatedFriendRequest("fr1");

            Assert.AreEqual("fr1", request.FriendRequestId);
            Assert.AreEqual("senderId", request.From);
            Assert.AreEqual("ownId", request.To);
            Assert.AreEqual(100, request.Timestamp);
            Assert.AreEqual("hey!", request.MessageBody);
        }

        [Test]
        public void AllocateByUserIdWhenReceiveFriendRequest()
        {
            apiBridge.OnFriendRequestReceived += Raise.Event<Action<FriendRequestPayload>>(new FriendRequestPayload
            {
                from = "senderId",
                timestamp = 100,
                to = "ownId",
                messageBody = "hey!",
                friendRequestId = "fr1",
            });

            FriendRequest request = controller.GetAllocatedFriendRequestByUser("senderId");

            Assert.AreEqual("fr1", request.FriendRequestId);
            Assert.AreEqual("senderId", request.From);
            Assert.AreEqual("ownId", request.To);
            Assert.AreEqual(100, request.Timestamp);
            Assert.AreEqual("hey!", request.MessageBody);
        }

        [UnityTest]
        public IEnumerator CancelFriendRequest() =>
            UniTask.ToCoroutine(async () =>
            {
                void VerifyRequest(FriendRequest request)
                {
                    Assert.AreEqual("fr", request.FriendRequestId);
                    Assert.AreEqual("ownId", request.From);
                    Assert.AreEqual("receiverId", request.To);
                    Assert.AreEqual(100, request.Timestamp);
                    Assert.AreEqual("bleh", request.MessageBody);
                }

                apiBridge.CancelRequestAsync("fr", Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(new CancelFriendshipConfirmationPayload
                          {
                              friendRequest = new FriendRequestPayload
                              {
                                  from = "ownId",
                                  friendRequestId = "fr",
                                  timestamp = 100,
                                  to = "receiverId",
                                  messageBody = "bleh",
                              }
                          }));

                FriendRequest request = await controller.CancelRequestAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                request = controller.GetAllocatedFriendRequest("fr");
                Assert.IsNull(request);

                request = controller.GetAllocatedFriendRequestByUser("receiverId");
                Assert.IsNull(request);
            });

        [UnityTest]
        public IEnumerator RequestFriendship() =>
            UniTask.ToCoroutine(async () =>
            {
                void VerifyRequest(FriendRequest request)
                {
                    Assert.AreEqual("fr", request.FriendRequestId);
                    Assert.AreEqual("ownId", request.From);
                    Assert.AreEqual("receiverId", request.To);
                    Assert.AreEqual(100, request.Timestamp);
                    Assert.AreEqual("bleh", request.MessageBody);
                }

                apiBridge.RequestFriendshipAsync("receiverId", "bleh", Arg.Any<CancellationToken>())
                         .Returns(
                              UniTask.FromResult(new RequestFriendshipConfirmationPayload
                              {
                                  friendRequest = new FriendRequestPayload
                                  {
                                      from = "ownId",
                                      friendRequestId = "fr",
                                      timestamp = 100,
                                      to = "receiverId",
                                      messageBody = "bleh",
                                  },
                              }));

                FriendRequest request = await controller.RequestFriendshipAsync("receiverId", "bleh",
                    default(CancellationToken));

                VerifyRequest(request);

                request = controller.GetAllocatedFriendRequest("fr");
                VerifyRequest(request);

                request = controller.GetAllocatedFriendRequestByUser("receiverId");
                VerifyRequest(request);
            });

        [UnityTest]
        public IEnumerator AcceptFriendship() =>
            UniTask.ToCoroutine(async () =>
            {
                void VerifyRequest(FriendRequest request)
                {
                    Assert.AreEqual("fr", request.FriendRequestId);
                    Assert.AreEqual("senderId", request.From);
                    Assert.AreEqual("receiverId", request.To);
                    Assert.AreEqual(100, request.Timestamp);
                    Assert.AreEqual("bleh", request.MessageBody);
                }

                apiBridge.AcceptFriendshipAsync("fr", Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(new AcceptFriendshipPayload
                          {
                              FriendRequest = new FriendRequestPayload
                              {
                                  from = "senderId",
                                  friendRequestId = "fr",
                                  timestamp = 100,
                                  to = "receiverId",
                                  messageBody = "bleh",
                              },
                          }));

                FriendRequest request = await controller.AcceptFriendshipAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                request = controller.GetAllocatedFriendRequest("fr");
                Assert.IsNull(request);

                request = controller.GetAllocatedFriendRequestByUser("receiverId");
                Assert.IsNull(request);
            });

        [UnityTest]
        public IEnumerator RejectFriendship() =>
            UniTask.ToCoroutine(async () =>
            {
                void VerifyRequest(FriendRequest request)
                {
                    Assert.AreEqual("fr", request.FriendRequestId);
                    Assert.AreEqual("senderId", request.From);
                    Assert.AreEqual("receiverId", request.To);
                    Assert.AreEqual(100, request.Timestamp);
                    Assert.AreEqual("bleh", request.MessageBody);
                }

                apiBridge.RejectFriendshipAsync("fr", Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(new RejectFriendshipPayload
                          {
                              FriendRequestPayload = new FriendRequestPayload
                              {
                                  from = "senderId",
                                  friendRequestId = "fr",
                                  timestamp = 100,
                                  to = "receiverId",
                                  messageBody = "bleh",
                              },
                          }));

                FriendRequest request = await controller.RejectFriendshipAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                request = controller.GetAllocatedFriendRequest("fr");
                Assert.IsNull(request);

                request = controller.GetAllocatedFriendRequestByUser("senderId");
                Assert.IsNull(request);
            });

        [UnityTest]
        public IEnumerator GetFriendRequests() =>
            UniTask.ToCoroutine(async () =>
            {
                void Verify(FriendRequest request, FriendRequestPayload payload)
                {
                    Assert.AreEqual(payload.friendRequestId, request.FriendRequestId);
                    Assert.AreEqual(payload.from, request.From);
                    Assert.AreEqual(payload.to, request.To);
                    Assert.AreEqual(payload.timestamp, request.Timestamp);
                    Assert.AreEqual(payload.messageBody, request.MessageBody);
                }

                FriendRequestPayload[] requestedTo =
                {
                    new ()
                    {
                        from = "ownId",
                        friendRequestId = "fr3",
                        timestamp = 300,
                        to = "usr3",
                        messageBody = "bleh",
                    },
                    new ()
                    {
                        from = "ownId",
                        friendRequestId = "fr4",
                        timestamp = 400,
                        to = "usr4",
                        messageBody = "bleh",
                    },
                };

                FriendRequestPayload[] requestedFrom =
                {
                    new ()
                    {
                        from = "usr1",
                        friendRequestId = "fr1",
                        timestamp = 100,
                        to = "ownId",
                        messageBody = "bleh",
                    },
                    new ()
                    {
                        from = "usr2",
                        friendRequestId = "fr2",
                        timestamp = 200,
                        to = "ownId",
                        messageBody = "bleh",
                    },
                };

                apiBridge.GetFriendRequestsAsync(5, 0, 5, 0, Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(new AddFriendRequestsV2Payload
                          {
                              totalReceivedFriendRequests = 2,
                              totalSentFriendRequests = 2,
                              requestedFrom = requestedFrom,
                              requestedTo = requestedTo,
                          }));

                List<FriendRequest> requests = await controller.GetFriendRequestsAsync(5, 0, 5, 0,
                    default(CancellationToken));

                Verify(requests[0], requestedFrom[0]);
                Verify(requests[1], requestedFrom[1]);
                Verify(requests[2], requestedTo[0]);
                Verify(requests[3], requestedTo[1]);

                FriendRequest request = controller.GetAllocatedFriendRequest("fr1");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequest("fr2");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequest("fr3");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequest("fr4");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequestByUser("usr1");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequestByUser("usr2");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequestByUser("usr3");
                Assert.IsNotNull(request);
                request = controller.GetAllocatedFriendRequestByUser("usr4");
                Assert.IsNotNull(request);
            });

        [UnityTest]
        public IEnumerator GetFriendsAsyncWithSocialService() =>
            UniTask.ToCoroutine(async () =>
            {
                GameObject go = new GameObject();
                var component = go.AddComponent<MatrixInitializationBridge>();
                var dataStore = new DataStore();

                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });

                var cancellationToken = default(CancellationToken);

                var initializationResponse = UniTask.FromResult(new FriendshipInitializationMessage()
                {
                    totalReceivedRequests = 0,
                });

                rpcSocialApiBridge.GetInitializationInformationAsync(cancellationToken)
                                  .Returns(initializationResponse);

                controller = new FriendsController(apiBridge, rpcSocialApiBridge, dataStore);

                dataStore.featureFlags.flags.Get().SetAsInitialized();
                controller.Initialize();

                await controller.InitializeAsync(cancellationToken);

                var firstUserId = "userId";
                var secondUserId = "userId2";
                var thirdUserId = "userId3";
                var fourthdUserId = "userId4";

                var firstUser = new UserStatus()
                {
                    userId = firstUserId,
                    userName = "aUserName1",
                };

                var secondUser = new UserStatus()
                {
                    userId = secondUserId,
                    userName = "aUserName2",
                };

                var thirdUser = new UserStatus()
                {
                    userId = thirdUserId,
                    userName = "searchText",
                };

                var fourthUser = new UserStatus()
                {
                    userId = fourthdUserId,
                    userName = "searchText2",
                };

                // insert unsorted
                rpcSocialApiBridge.OnFriendAdded += Raise.Event<Action<UserStatus>>(fourthUser);
                rpcSocialApiBridge.OnFriendAdded += Raise.Event<Action<UserStatus>>(thirdUser);
                rpcSocialApiBridge.OnFriendAdded += Raise.Event<Action<UserStatus>>(secondUser);
                rpcSocialApiBridge.OnFriendAdded += Raise.Event<Action<UserStatus>>(firstUser);

                IReadOnlyList<string> response = await controller.GetFriendsAsync(100, 0, cancellationToken);
                string[] expected = { firstUserId, secondUserId, thirdUserId, fourthdUserId };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync(1, 0, cancellationToken);
                expected = new[] { firstUserId };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync(2, 1, cancellationToken);
                expected = new[] { secondUserId, thirdUserId };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync("search", 10, cancellationToken);
                expected = new[] { thirdUserId, fourthdUserId };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync("search", 1, cancellationToken);
                expected = new[] { thirdUserId };

                CollectionAssert.AreEqual(response, expected);
            });
    }
}
