using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private IUserProfileBridge userProfileBridge;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            apiBridge = Substitute.For<IFriendsApiBridge>();
            dataStore = new DataStore();
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = false } });
            dataStore.featureFlags.flags.Get().SetAsInitialized();

            rpcSocialApiBridge = Substitute.For<ISocialApiBridge>();

            rpcSocialApiBridge.GetInitializationInformationAsync(Arg.Any<CancellationToken>())
                              .Returns(UniTask.FromResult(new AllFriendsInitializationMessage(
                                   new List<string>(), new List<FriendRequest>(), new List<FriendRequest>())));
            userProfileBridge = Substitute.For<IUserProfileBridge>();

            controller = new FriendsController(apiBridge, rpcSocialApiBridge, dataStore, userProfileBridge);
        }

        [Test]
        public void Initialize()
        {
            controller.Initialize();

            var called = false;
            controller.OnInitialized += () => called = true;

            apiBridge.OnInitialized += Raise.Event<Action<FriendshipInitializationMessage>>(
                new FriendshipInitializationMessage
                {
                    totalReceivedRequests = 4,
                });

            Assert.AreEqual(4, controller.TotalReceivedFriendRequestCount);
            Assert.IsTrue(called);
        }

        [Test]
        public void AddFriends()
        {
            apiBridge.GetFriendsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(
                              new AddFriendsPayload
                              {
                                  totalFriends = 2,
                                  friends = new[] { "woah", "bleh" }
                              }));

            controller.Initialize();
            controller.GetFriendsAsync(0, 0).Forget();

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
                              requestedFrom = new[]
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
                              requestedTo = new[]
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

            controller.Initialize();
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

            controller.Initialize();

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

            apiBridge.GetFriendsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                         .Returns(UniTask.FromResult(
                              new AddFriendsPayload
                              {
                                  totalFriends = 7,
                                  friends = new[] { "usr1" }
                              }));

            controller.Initialize();
            controller.GetFriendsAsync(0, 0).Forget();

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

            controller.Initialize();

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

            controller.Initialize();

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
            controller.Initialize();

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
            controller.Initialize();

            apiBridge.OnFriendRequestReceived += Raise.Event<Action<FriendRequestPayload>>(new FriendRequestPayload
            {
                from = "senderId",
                timestamp = 100,
                to = "ownId",
                messageBody = "hey!",
                friendRequestId = "fr1",
            });

            bool wasFound = controller.TryGetAllocatedFriendRequest("fr1", out FriendRequest request);
            Assert.True(wasFound);
            Assert.AreEqual("fr1", request.FriendRequestId);
            Assert.AreEqual("senderId", request.From);
            Assert.AreEqual("ownId", request.To);
            Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
            Assert.AreEqual("hey!", request.MessageBody);
        }

        [Test]
        public void AllocateByUserIdWhenReceiveFriendRequest()
        {
            controller.Initialize();

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
            Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
            Assert.AreEqual("hey!", request.MessageBody);
        }

        [Test]
        public void AllocateByUserIdWhenReceiveFriendRequestWithSocialClient()
        {
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
            dataStore.featureFlags.flags.Get().SetAsInitialized();
            controller.Initialize();

            FriendRequest CreateFriendRequest(int number) =>
                new ($"id{number}", new DateTime(number), $"from{number}", $"to{number}", $"a message {number}");

            var lateUser1Request = new FriendRequest($"id1", new DateTime(10), "from1", "to1", "a message 2");

            var outgoingFriendRequests = new[]
            {
                CreateFriendRequest(1),
                CreateFriendRequest(2),
                CreateFriendRequest(3),
                CreateFriendRequest(4),
                lateUser1Request
            };

            var incomingFriendRequests = new[]
            {
                CreateFriendRequest(5),
                CreateFriendRequest(6),
                CreateFriendRequest(7),
                CreateFriendRequest(8),
            };

            foreach (var friendRequest in outgoingFriendRequests)
            {
                rpcSocialApiBridge.OnOutgoingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(
                    friendRequest
                );
            }

            foreach (var friendRequest in incomingFriendRequests)
            {
                rpcSocialApiBridge.OnIncomingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(
                    friendRequest
                );
            }

            FriendRequest request = controller.GetAllocatedFriendRequestByUser(lateUser1Request.From);

            Assert.AreEqual(lateUser1Request, request);
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
                    Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
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

                controller.Initialize();
                FriendRequest request = await controller.CancelRequestAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                bool wasFound = controller.TryGetAllocatedFriendRequest("fr", out request);
                Assert.IsNull(request);
                Assert.False(wasFound);

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
                    Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
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

                controller.Initialize();
                FriendRequest request = await controller.RequestFriendshipAsync("receiverId", "bleh",
                    default(CancellationToken));

                VerifyRequest(request);

                bool wasFound = controller.TryGetAllocatedFriendRequest("fr", out request);
                Assert.IsNotNull(request);
                Assert.True(wasFound);
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
                    Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
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

                controller.Initialize();
                FriendRequest request = await controller.AcceptFriendshipAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                bool wasFound = controller.TryGetAllocatedFriendRequest("fr", out request);
                Assert.IsNull(request);
                Assert.False(wasFound);

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
                    Assert.AreEqual(621355968001000000, request.Timestamp.Ticks);
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

                controller.Initialize();
                FriendRequest request = await controller.RejectFriendshipAsync("fr", default(CancellationToken));
                VerifyRequest(request);

                bool wasFound = controller.TryGetAllocatedFriendRequest("fr", out request);
                Assert.IsNull(request);
                Assert.False(wasFound);

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
                    Assert.AreEqual(DateTimeOffset.FromUnixTimeMilliseconds(payload.timestamp).Ticks, request.Timestamp.Ticks);
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

                controller.Initialize();
                List<FriendRequest> requests = (await controller.GetFriendRequestsAsync(5, 0, 5, 0,
                    default(CancellationToken))).ToList();

                Verify(requests[0], requestedFrom[0]);
                Verify(requests[1], requestedFrom[1]);
                Verify(requests[2], requestedTo[0]);
                Verify(requests[3], requestedTo[1]);

                bool wasFound = controller.TryGetAllocatedFriendRequest("fr1", out FriendRequest request);
                Assert.IsNotNull(request);
                Assert.True(wasFound);
                wasFound = controller.TryGetAllocatedFriendRequest("fr2", out request);
                Assert.IsNotNull(request);
                Assert.True(wasFound);
                wasFound = controller.TryGetAllocatedFriendRequest("fr3", out request);
                Assert.IsNotNull(request);
                Assert.True(wasFound);
                wasFound = controller.TryGetAllocatedFriendRequest("fr4", out request);
                Assert.IsNotNull(request);
                Assert.True(wasFound);
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
                const string FIRST_USER_ID = "userId";
                const string SECOND_USER_ID = "userId2";
                const string THIRD_USER_ID = "userId3";
                const string FOURTH_USER_ID = "userId4";

                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
                dataStore.featureFlags.flags.Get().SetAsInitialized();

                GivenUserProfile(FIRST_USER_ID, "aUserName1");
                GivenUserProfile(SECOND_USER_ID, "aUserName2");
                GivenUserProfile(THIRD_USER_ID, "searchText");
                GivenUserProfile(FOURTH_USER_ID, "searchText2");

                var initializationResponse = UniTask.FromResult(new AllFriendsInitializationMessage(
                    new List<string>
                    {
                        FOURTH_USER_ID,
                        THIRD_USER_ID,
                        SECOND_USER_ID,
                        FIRST_USER_ID,
                    }, new List<FriendRequest>(), new List<FriendRequest>()));

                rpcSocialApiBridge.GetInitializationInformationAsync(Arg.Any<CancellationToken>())
                                  .Returns(initializationResponse);

                controller.Initialize();
                await UniTask.NextFrame();

                IReadOnlyList<string> response = await controller.GetFriendsAsync(100, 0);
                string[] expected = { FIRST_USER_ID, SECOND_USER_ID, THIRD_USER_ID, FOURTH_USER_ID };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync(1, 0);
                expected = new[] { FIRST_USER_ID };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync(2, 1);
                expected = new[] { SECOND_USER_ID, THIRD_USER_ID };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync("search", 10);
                expected = new[] { THIRD_USER_ID, FOURTH_USER_ID };

                CollectionAssert.AreEqual(response, expected);

                response = await controller.GetFriendsAsync("search", 1);
                expected = new[] { THIRD_USER_ID };

                CollectionAssert.AreEqual(response, expected);
            });

        [UnityTest]
        public IEnumerator GetFriendRequestsAsyncWithSocialBridge() =>
            UniTask.ToCoroutine(async () =>
            {
                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
                dataStore.featureFlags.flags.Get().SetAsInitialized();
                controller.Initialize();

                FriendRequest CreateFriendRequest(int number) =>
                    new ($"id{number}", new DateTime(number), $"from{number}", $"to{number}", $"a message {number}");

                var outgoingFriendRequests = new[]
                {
                    CreateFriendRequest(1),
                    CreateFriendRequest(2),
                    CreateFriendRequest(3),
                    CreateFriendRequest(4),
                };

                var incomingFriendRequests = new[]
                {
                    CreateFriendRequest(5),
                    CreateFriendRequest(6),
                    CreateFriendRequest(7),
                    CreateFriendRequest(8),
                };

                foreach (var friendRequest in outgoingFriendRequests)
                {
                    rpcSocialApiBridge.OnOutgoingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(
                        friendRequest
                    );
                }

                foreach (var friendRequest in incomingFriendRequests)
                {
                    rpcSocialApiBridge.OnIncomingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(
                        friendRequest
                    );
                }

                var result = await controller.GetFriendRequestsAsync(100, 0, 100, 0, default(CancellationToken));

                // the result should be reversed since the result should be sorted by timestamp
                CollectionAssert.AreEqual(incomingFriendRequests.Reverse().Concat(outgoingFriendRequests.Reverse()), result);
            });

        [UnityTest]
        public IEnumerator AcceptedFriendRequestWithSocialBridge() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "FriendId";

                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
                dataStore.featureFlags.flags.Get().SetAsInitialized();
                controller.Initialize();

                GivenUserProfile(FRIEND_ID, "FriendName");
                var friendsUpdated = new Dictionary<string, FriendshipAction>();

                controller.OnUpdateFriendship += (s, action) => friendsUpdated[s] = action;

                rpcSocialApiBridge.OnFriendRequestAccepted += Raise.Event<Action<string>>(FRIEND_ID);

                string[] friends = await controller.GetFriendsAsync(100, 0);

                Assert.AreEqual(FriendshipAction.APPROVED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(friends, new List<string>
                    { FRIEND_ID });
            });

        [UnityTest]
        public IEnumerator DeletedByFriendWithSocialBridge() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "FriendId";

                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
                dataStore.featureFlags.flags.Get().SetAsInitialized();
                controller.Initialize();

                GivenUserProfile(FRIEND_ID, "FriendName");

                Dictionary<string, FriendshipAction> friendsUpdated = new ();
                controller.OnUpdateFriendship += (s, action) => friendsUpdated[s] = action;

                rpcSocialApiBridge.OnFriendRequestAccepted += Raise.Event<Action<string>>(FRIEND_ID);

                string[] friends = await controller.GetFriendsAsync(100, 0);

                Assert.AreEqual(FriendshipAction.APPROVED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(friends, new List<string>
                    { FRIEND_ID });

                rpcSocialApiBridge.OnDeletedByFriend += Raise.Event<Action<string>>(FRIEND_ID);

                friends = await controller.GetFriendsAsync(100, 0);

                Assert.AreEqual(FriendshipAction.DELETED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(friends, new List<string>());
            });

        [UnityTest]
        public IEnumerator FriendRequestFullFlowWithSocialService() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "FriendId";
                const string MESSAGE_BODY = "message";
                const string OWN_USER_ID = "ownUserId";

                dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["use-social-client"] = true } });
                dataStore.featureFlags.flags.Get().SetAsInitialized();
                controller.Initialize();

                Dictionary<string, FriendshipAction> friendsUpdated = new ();
                GivenUserProfile(FRIEND_ID, "FriendName");

                controller.OnUpdateFriendship += (s, action) => friendsUpdated[s] = action;

                var friendRequestId = $"{FRIEND_ID}-1";
                FriendRequest sentFriendRequest = new (friendRequestId, new DateTime(1), OWN_USER_ID, FRIEND_ID, MESSAGE_BODY);
                FriendRequest receivedFriendRequest = new (friendRequestId, new DateTime(1), FRIEND_ID, OWN_USER_ID, MESSAGE_BODY);

                rpcSocialApiBridge.RequestFriendshipAsync(FRIEND_ID, MESSAGE_BODY, Arg.Any<CancellationToken>())
                                  .Returns(UniTask.FromResult(sentFriendRequest));

                rpcSocialApiBridge.RejectFriendshipAsync(sentFriendRequest.From, Arg.Any<CancellationToken>())
                                  .Returns(UniTask.CompletedTask);

                rpcSocialApiBridge.CancelFriendshipAsync(sentFriendRequest.From, Arg.Any<CancellationToken>())
                                  .Returns(UniTask.CompletedTask);

                rpcSocialApiBridge.AcceptFriendshipAsync(sentFriendRequest.From, Arg.Any<CancellationToken>())
                                  .Returns(UniTask.CompletedTask);

                rpcSocialApiBridge.DeleteFriendshipAsync(sentFriendRequest.From, Arg.Any<CancellationToken>())
                                  .Returns(UniTask.CompletedTask);

                await controller.RequestFriendshipAsync(FRIEND_ID, MESSAGE_BODY, default);

                string[] friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.REQUESTED_TO, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string>(), friends);

                rpcSocialApiBridge.OnFriendRequestRejected += Raise.Event<Action<string>>(FRIEND_ID);

                friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.REJECTED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string>(), friends);

                rpcSocialApiBridge.OnIncomingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(receivedFriendRequest);

                friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.REQUESTED_FROM, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string>(), friends);

                await controller.RejectFriendshipAsync(friendRequestId, default);

                friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.REJECTED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string>(), friends);

                rpcSocialApiBridge.OnIncomingFriendRequestAdded += Raise.Event<Action<FriendRequest>>(receivedFriendRequest);

                friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.REQUESTED_FROM, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string>(), friends);

                await controller.AcceptFriendshipAsync(friendRequestId, default);

                friends = await controller.GetFriendsAsync(100, 0);
                Assert.AreEqual(FriendshipAction.APPROVED, friendsUpdated[FRIEND_ID]);
                CollectionAssert.AreEqual(new List<string> { FRIEND_ID }, friends);
            });

        private void GivenUserProfile(string userId, string userName)
        {
            UserProfile usr = ScriptableObject.CreateInstance<UserProfile>();
            usr.UpdateData(new UserProfileModel
            {
                userId = userId,
                name = userName,
            });

            userProfileBridge.Get(userId).Returns(usr);
        }
    }
}
