using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
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
    public class RPCSocialApiBridgeShould
    {
        private const string OWN_ID = "My custom id";
        private const string ACCESS_TOKEN = "Token";

        private RPCSocialApiBridge rpcSocialApiBridge;
        private IUserProfileBridge userProfileBridge;
        private IClientFriendshipsService friendshipsService;
        private CancellationTokenSource testsCancellationToken;

        [SetUp]
        public void SetUp()
        {
            testsCancellationToken = new CancellationTokenSource();
            userProfileBridge = Substitute.For<IUserProfileBridge>();

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownProfile.UpdateData(new UserProfileModel { userId = OWN_ID });
            userProfileBridge.GetOwn().Returns(ownProfile);

            var matrixInitializationBridge = Substitute.For<IMatrixInitializationBridge>();

            ISocialClientProvider socialClientProvider = Substitute.For<ISocialClientProvider>();
            friendshipsService = Substitute.For<IClientFriendshipsService>();

            friendshipsService.GetFriends(Arg.Any<Payload>())
                              .Returns(UniTaskAsyncEnumerable.Create<UsersResponse>(async (writer, token) =>
                               {
                                   await writer.YieldAsync(new UsersResponse
                                   {
                                       Users = new Users()
                                   });
                               }));

            friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                              .Returns(UniTask.Never<UpdateFriendshipResponse>(testsCancellationToken.Token));

            friendshipsService.SubscribeFriendshipEventsUpdates(Arg.Any<Payload>())
                              .Returns(UniTaskAsyncEnumerable.Never<SubscribeFriendshipEventsUpdatesResponse>());

            friendshipsService.GetRequestEvents(Arg.Any<Payload>())
                              .Returns(UniTask.FromResult(new RequestEventsResponse
                               {
                                   Events = new RequestEvents
                                   {
                                       Incoming = new Requests
                                       {
                                           Total = 0,
                                       },
                                       Outgoing = new Requests
                                       {
                                           Total = 0,
                                       }
                                   }
                               }));

            socialClientProvider.Provide(Arg.Any<CancellationToken>())
                                .Returns(UniTask.FromResult(friendshipsService));

            rpcSocialApiBridge = new RPCSocialApiBridge(matrixInitializationBridge, userProfileBridge, socialClientProvider);

            matrixInitializationBridge.AccessToken.Returns(ACCESS_TOKEN);
        }

        [TearDown]
        public void TearDown()
        {
            testsCancellationToken.Cancel();
            testsCancellationToken.Dispose();
        }

        [UnityTest]
        public IEnumerator Initialize() =>
            UniTask.ToCoroutine(async () =>
            {
                friendshipsService.GetFriends(Arg.Any<Payload>())
                                  .Returns(UniTaskAsyncEnumerable.Create<UsersResponse>(async (writer, token) =>
                                   {
                                       await writer.YieldAsync(new UsersResponse
                                       {
                                           Users = new Users
                                           {
                                               Users_ =
                                               {
                                                   new User { Address = "addr1" },
                                                   new User { Address = "addr2" },
                                               },
                                           },
                                       });

                                       await writer.YieldAsync(new UsersResponse
                                       {
                                           Users = new Users
                                           {
                                               Users_ =
                                               {
                                                   new User { Address = "addr3" },
                                                   new User { Address = "addr4" },
                                               },
                                           },
                                       });
                                   }));

                var incomingRequests = new[]
                {
                    NewRequest("a Message 1", "firstUser", 1),
                    NewRequest("a Message 2", "secondUser", 2),
                    NewRequest("a Message 3", "thirdUser", 3),
                };

                var outgoingRequests = new[]
                {
                    NewRequest("a Message 4", "fourthUser", 4),
                    NewRequest("a Message 5", "fifthUser", 5),
                    NewRequest("a Message 6", "sixthUser", 6),
                    NewRequest("a Message 7", "seventhUser", 7),
                };

                friendshipsService.GetRequestEvents(Arg.Any<Payload>())
                                  .Returns(UniTask.FromResult(
                                       new RequestEventsResponse
                                       {
                                           Events = new RequestEvents
                                           {
                                               Incoming = new Requests
                                               {
                                                   Items = { incomingRequests },
                                                   Total = incomingRequests.Length,
                                               },
                                               Outgoing = new Requests
                                               {
                                                   Items = { outgoingRequests },
                                                   Total = outgoingRequests.Length,
                                               }
                                           }
                                       }
                                   ));

                rpcSocialApiBridge.Initialize();

                var response = await rpcSocialApiBridge.GetInitializationInformationAsync();

                Assert.AreEqual(4, response.Friends.Count);
                Assert.AreEqual(incomingRequests.Length, response.totalReceivedRequests);

                var expectedIncoming = incomingRequests.Select(IncomingFriendRequestFromResponse).ToList();
                var expectedOutgoing = outgoingRequests.Select(OutgoingFriendRequestFromResponse).ToList();

                CollectionAssert.AreEqual(expectedIncoming.ToList(), response.IncomingFriendRequests);
                CollectionAssert.AreEqual(expectedOutgoing.ToList(), response.OutgoingFriendRequests);
            });

        [UnityTest]
        public IEnumerator RequestFriendshipAsync()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var friendId = "Friend ID";
                var message = "Hello friend";
                var createdAt = 1234;

                friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                                  .Returns(UniTask.FromResult(new UpdateFriendshipResponse
                                   {
                                       Event = CreateRequestFriendshipResponse(friendId, message, createdAt),
                                   }));

                rpcSocialApiBridge.Initialize();

                // wait for async initialization
                await UniTask.NextFrame();

                var result = await rpcSocialApiBridge.RequestFriendshipAsync(friendId, message);

                Assert.AreEqual(result.MessageBody, message);
                Assert.AreEqual(result.From, OWN_ID);
                Assert.AreEqual(result.To, friendId);
            });
        }

        [UnityTest]
        public IEnumerator RejectFriendshipAsync()
        {
            return UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "Friend ID";

                friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                                  .Returns(UniTask.FromResult(new UpdateFriendshipResponse
                                   {
                                       Event = CreateRejectFriendshipResponse(FRIEND_ID),
                                   }));

                rpcSocialApiBridge.Initialize();

                // wait for async initialization
                await UniTask.NextFrame();

                await rpcSocialApiBridge.RejectFriendshipAsync(FRIEND_ID);

                friendshipsService.Received(1)
                                  .UpdateFriendshipEvent(
                                       Arg.Is<UpdateFriendshipPayload>(u => u.Event.Reject.User.Address == FRIEND_ID));
            });
        }

        [UnityTest]
        public IEnumerator CancelFriendRequest() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "Friend ID";

                friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                                  .Returns(UniTask.FromResult(new UpdateFriendshipResponse
                                   {
                                       Event = new FriendshipEventResponse
                                       {
                                           Cancel = new CancelResponse
                                           {
                                               User = new User
                                               {
                                                   Address = FRIEND_ID
                                               }
                                           }
                                       }
                                   }));

                rpcSocialApiBridge.Initialize();

                // wait for async initialization
                await UniTask.NextFrame();

                await rpcSocialApiBridge.CancelFriendshipAsync(FRIEND_ID);

                friendshipsService.Received(1)
                                  .UpdateFriendshipEvent(
                                       Arg.Is<UpdateFriendshipPayload>(u => u.Event.Cancel.User.Address == FRIEND_ID));
            });

        [UnityTest]
        public IEnumerator AcceptFriendRequest() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "Friend ID";

                friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                                  .Returns(UniTask.FromResult(new UpdateFriendshipResponse
                                   {
                                       Event = new FriendshipEventResponse
                                       {
                                           Accept = new AcceptResponse
                                           {
                                               User = new User
                                               {
                                                   Address = FRIEND_ID
                                               }
                                           }
                                       }
                                   }));

                rpcSocialApiBridge.Initialize();

                // wait for async initialization
                await UniTask.NextFrame();

                await rpcSocialApiBridge.AcceptFriendshipAsync(FRIEND_ID);

                friendshipsService.Received(1)
                                  .UpdateFriendshipEvent(
                                       Arg.Is<UpdateFriendshipPayload>(u => u.Event.Accept.User.Address == FRIEND_ID));
            });

        [UnityTest]
        public IEnumerator DeleteFriendRequest() =>
            UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "Friend ID";

                friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>())
                                  .Returns(UniTask.FromResult(new UpdateFriendshipResponse
                                   {
                                       Event = new FriendshipEventResponse
                                       {
                                           Delete = new DeleteResponse
                                           {
                                               User = new User
                                               {
                                                   Address = FRIEND_ID
                                               }
                                           }
                                       }
                                   }));

                rpcSocialApiBridge.Initialize();

                // wait for async initialization
                await UniTask.NextFrame();

                await rpcSocialApiBridge.DeleteFriendshipAsync(FRIEND_ID);

                friendshipsService.Received(1)
                                  .UpdateFriendshipEvent(
                                       Arg.Is<UpdateFriendshipPayload>(u => u.Event.Delete.User.Address == FRIEND_ID));
            });

        [UnityTest]
        public IEnumerator SubscribeToFriendshipEventsEverySuccessfulAnswer()
        {
            return UniTask.ToCoroutine(async () =>
            {
                const string FRIEND_ID = "Friend ID";
                const string ACCEPT_FRIEND_ID = "Accept";
                const string REJECT_FRIEND_ID = "Reject";
                const string CANCEL_FRIEND_ID = "Cancel";
                const string DELETE_FRIEND_ID = "Delete";
                const int CREATED_AT = 123;

                var friendRequest = new RequestResponse
                {
                    User = new User
                    {
                        Address = FRIEND_ID,
                    },
                    Message = "a message",
                    CreatedAt = CREATED_AT
                };

                friendshipsService.SubscribeFriendshipEventsUpdates(Arg.Any<Payload>())
                                  .Returns(UniTaskAsyncEnumerable.Return(new SubscribeFriendshipEventsUpdatesResponse
                                   {
                                       Events = new FriendshipEventResponses
                                       {
                                           Responses =
                                           {
                                               new List<FriendshipEventResponse>
                                               {
                                                   new ()
                                                   {
                                                       Accept = new AcceptResponse { User = new User { Address = ACCEPT_FRIEND_ID } },
                                                   },
                                                   new ()
                                                   {
                                                       Cancel = new CancelResponse { User = new User { Address = CANCEL_FRIEND_ID } },
                                                   },
                                                   new ()
                                                   {
                                                       Reject = new RejectResponse { User = new User { Address = REJECT_FRIEND_ID } },
                                                   },
                                                   new ()
                                                   {
                                                       Delete = new DeleteResponse { User = new User { Address = DELETE_FRIEND_ID } },
                                                   },
                                                   new ()
                                                   {
                                                       Request = friendRequest,
                                                   },
                                               },
                                           },
                                       },
                                   }));

                var expectedFriendRequest = new FriendRequest(
                    $"{friendRequest.User.Address}-{friendRequest.CreatedAt}",
                    DateTimeOffset.FromUnixTimeMilliseconds(friendRequest.CreatedAt * 1000L).DateTime,
                    friendRequest.User.Address,
                    OWN_ID,
                    friendRequest.Message);

                var acceptedResult = "";
                var cancelResult = "";
                var deletedResult = "";
                var rejectResult = "";
                FriendRequest requestResult = null;

                rpcSocialApiBridge.OnFriendRequestAccepted += userId => { acceptedResult = userId; };

                rpcSocialApiBridge.OnFriendRequestCanceled += (userId) => { cancelResult = userId; };

                rpcSocialApiBridge.OnFriendRequestRejected += (userId) => { rejectResult = userId; };
                rpcSocialApiBridge.OnDeletedByFriend += (userId) => { deletedResult = userId; };
                rpcSocialApiBridge.OnIncomingFriendRequestAdded += (request) => { requestResult = request; };

                rpcSocialApiBridge.Initialize();
                // wait for async initialization
                await UniTask.NextFrame();

                Assert.AreEqual(ACCEPT_FRIEND_ID, acceptedResult);

                Assert.AreEqual(CANCEL_FRIEND_ID, cancelResult);
                Assert.AreEqual(REJECT_FRIEND_ID, rejectResult);
                Assert.AreEqual(DELETE_FRIEND_ID, deletedResult);
                Assert.AreEqual(expectedFriendRequest, requestResult);
            });
        }

        [UnityTest]
        public IEnumerator SubscribeToFriendshipEventsEveryFailedAnswer()
        {
            return UniTask.ToCoroutine(async () =>
            {
                const string REJECT_FRIEND_ID = "Reject";

                friendshipsService.SubscribeFriendshipEventsUpdates(Arg.Any<Payload>())
                                  .Returns(UniTaskAsyncEnumerable.Create<SubscribeFriendshipEventsUpdatesResponse>(async (writer, token) =>
                                   {
                                       await writer.YieldAsync(new SubscribeFriendshipEventsUpdatesResponse
                                       {
                                           ForbiddenError = new ForbiddenError() { Message = "Forbidden" },
                                       });

                                       await writer.YieldAsync(new SubscribeFriendshipEventsUpdatesResponse
                                       {
                                           UnauthorizedError = new UnauthorizedError() { Message = "Unauthorized" },
                                       });

                                       await writer.YieldAsync(new SubscribeFriendshipEventsUpdatesResponse
                                       {
                                           InternalServerError = new InternalServerError() { Message = "Internal server" },
                                       });

                                       await writer.YieldAsync(new SubscribeFriendshipEventsUpdatesResponse
                                       {
                                           TooManyRequestsError = new TooManyRequestsError() { Message = "Too many requests" },
                                       });

                                       await writer.YieldAsync(new SubscribeFriendshipEventsUpdatesResponse
                                       {
                                           Events = new FriendshipEventResponses()
                                           {
                                               Responses =
                                               {
                                                   new FriendshipEventResponse()
                                                   {
                                                       Reject = new RejectResponse() { User = new User() { Address = REJECT_FRIEND_ID } }
                                                   }
                                               }
                                           }
                                       });
                                   }));

                var rejectResult = "";

                rpcSocialApiBridge.OnFriendRequestRejected += (userId) => { rejectResult = userId; };

                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Forbidden error Forbidden");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Unauthorized error Unauthorized");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got internal server error Internal server");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Too many requests error Too many requests");

                rpcSocialApiBridge.Initialize();
                await UniTask.WaitUntil(() => !string.IsNullOrEmpty(rejectResult));

                // Asserting that reject was correctly received and all errors were parsed correctly
                Assert.AreEqual(REJECT_FRIEND_ID, rejectResult);
            });
        }

        private static FriendshipEventResponse CreateRejectFriendshipResponse(string address)
        {
            return new FriendshipEventResponse
            {
                Reject = new RejectResponse
                {
                    User = new User
                    {
                        Address = address
                    }
                }
            };
        }

        private static FriendshipEventResponse CreateRequestFriendshipResponse(string address, string message, long createdAt)
        {
            return new FriendshipEventResponse
            {
                Request = new RequestResponse
                {
                    User = new User
                    {
                        Address = address
                    },
                    Message = message,
                    CreatedAt = createdAt
                }
            };
        }

        private static RequestResponse NewRequest(string message, string address, long createdAt) =>
            new ()
            {
                Message = message,
                User = new User
                {
                    Address = address,
                },
                CreatedAt = createdAt,
            };

        private static FriendRequest IncomingFriendRequestFromResponse(RequestResponse response) =>
            new FriendRequest(
                $"{response.User.Address}-{response.CreatedAt}",
                DateTimeOffset.FromUnixTimeMilliseconds(response.CreatedAt * 1000L).DateTime,
                response.User.Address,
                OWN_ID,
                response.Message);

        private static FriendRequest OutgoingFriendRequestFromResponse(RequestResponse response) =>
            new FriendRequest(
                $"{response.User.Address}-{response.CreatedAt}",
                DateTimeOffset.FromUnixTimeMilliseconds(response.CreatedAt * 1000L).DateTime,
                OWN_ID,
                response.User.Address,
                response.Message);
    }
}
