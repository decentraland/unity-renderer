using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using NSubstitute;
using NUnit.Framework;
using rpc_csharp;
using rpc_csharp.transport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Friends
{
    public class MockSocialServerContext
    {
        public List<Users> UserList = new List<Users>();

        public UpdateFriendshipResponse UpdateFriendshipResponse;

        public List<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdatesResponses;
    }

    public class RPCSocialApiBridgeShould
    {
        private RPCSocialApiBridge rpcSocialApiBridge;
        private MockSocialServerContext context;
        private IUserProfileBridge userProfileBridge;
        private IFriendshipsService<MockSocialServerContext> friendshipsService;

        private const string OWN_ID = "My custom id";
        private const string ACCESS_TOKEN = "Token";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            userProfileBridge = Substitute.For<IUserProfileBridge>();

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownProfile.UpdateData(new UserProfileModel { userId = OWN_ID });
            userProfileBridge.GetOwn().Returns(ownProfile);

            (var client, var server) = MemoryTransport.Create();

            friendshipsService = Substitute.For<IFriendshipsService<MockSocialServerContext>>();

            friendshipsService.GetFriends(Arg.Any<Payload>(), Arg.Any<MockSocialServerContext>())
                              .Returns(callInfo =>
                               {
                                   MockSocialServerContext context = (MockSocialServerContext)callInfo[1];

                                   return UniTaskAsyncEnumerable.Create<UsersResponse>(async (writer, token) =>
                                   {
                                       foreach (var users in context.UserList)
                                       {
                                           if (token.IsCancellationRequested) break;

                                           await writer.YieldAsync(new UsersResponse()
                                           {
                                               Users = users
                                           });
                                       }
                                   });
                               });

            friendshipsService.UpdateFriendshipEvent(Arg.Any<UpdateFriendshipPayload>(), Arg.Any<MockSocialServerContext>(),
                                   Arg.Any<CancellationToken>())
                              .Returns(callInfo =>
                               {
                                   MockSocialServerContext context = (MockSocialServerContext)callInfo[1];
                                   return UniTask.FromResult(context.UpdateFriendshipResponse);
                               });

            friendshipsService.SubscribeFriendshipEventsUpdates(Arg.Any<Payload>(), Arg.Any<MockSocialServerContext>())
                              .Returns(
                                   callInfo =>
                                   {
                                       MockSocialServerContext context = (MockSocialServerContext)callInfo[1];

                                       return UniTaskAsyncEnumerable.Create<SubscribeFriendshipEventsUpdatesResponse>(async (writer, token) =>
                                       {
                                           var responses = context.SubscribeFriendshipEventsUpdatesResponses;

                                           foreach (var response in responses) { await writer.YieldAsync(response); }
                                       });
                                   });

            var rpcServer = new RpcServer<MockSocialServerContext>();
            rpcServer.SetHandler((port, _, _) => { FriendshipsServiceCodeGen.RegisterService(port, friendshipsService); });

            context = new MockSocialServerContext();
            rpcServer.AttachTransport(server, context);

            var matrixInitializationBridge = Substitute.For<IMatrixInitializationBridge>();

            rpcSocialApiBridge = new RPCSocialApiBridge(matrixInitializationBridge, userProfileBridge, () => UniTask.FromResult(client));
            rpcSocialApiBridge.Initialize();
            yield return rpcSocialApiBridge.InitializeAsync(default(CancellationToken)).ToCoroutine();

            matrixInitializationBridge.OnReceiveMatrixAccessToken += Raise.Event<Action<string>>(ACCESS_TOKEN);
        }

        [UnityTest]
        public IEnumerator Initialize()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var friends = new HashSet<string>();

                context.UserList = new List<Users>()
                {
                    new ()
                    {
                        Users_ =
                        {
                            new User() { Address = "addr1" },
                            new User() { Address = "addr2" }
                        }
                    },
                    new ()
                    {
                        Users_ =
                        {
                            new User() { Address = "addr3" },
                            new User() { Address = "addr4" }
                        }
                    }
                };

                var userProfile = ScriptableObject.CreateInstance<UserProfile>();
                var userName = "A custom name";

                userProfile.UpdateData(new UserProfileModel()
                {
                    name = userName,
                });

                rpcSocialApiBridge.OnFriendAdded += friendId =>
                {
                    if (string.IsNullOrEmpty(friendId))
                        throw new Exception("User should have a userId");

                    friends.Add(friendId);
                };

                userProfileBridge.RequestFullUserProfileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                 .Returns(new UniTask<UserProfile>(userProfile));

                var incomingResult = new List<FriendRequest>();
                var outgoingResult = new List<FriendRequest>();

                rpcSocialApiBridge.OnIncomingFriendRequestAdded += friendRequest => { incomingResult.Add(friendRequest); };
                rpcSocialApiBridge.OnOutgoingFriendRequestAdded += friendRequest => { outgoingResult.Add(friendRequest); };

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

                friendshipsService.GetRequestEvents(Arg.Any<Payload>(), Arg.Any<MockSocialServerContext>(), Arg.Any<CancellationToken>())
                                  .Returns(UniTask.FromResult(
                                       new RequestEventsResponse()
                                       {
                                           Events = new RequestEvents()
                                           {
                                               Incoming = new Requests()
                                               {
                                                   Items = { incomingRequests },
                                                   Total = incomingRequests.Length,
                                               },
                                               Outgoing = new Requests()
                                               {
                                                   Items = { outgoingRequests },
                                                   Total = outgoingRequests.Length,
                                               }
                                           }
                                       }
                                   ));

                var response = await rpcSocialApiBridge.GetInitializationInformationAsync();

                Assert.AreEqual(4, friends.Count);
                Assert.AreEqual(incomingRequests.Length, response.totalReceivedRequests);

                var expectedIncoming = incomingRequests.Select(IncomingFriendRequestFromResponse).ToList();
                var expectedOutgoing = outgoingRequests.Select(OutgoingFriendRequestFromResponse).ToList();

                CollectionAssert.AreEqual(expectedIncoming.ToList(), incomingResult);
                CollectionAssert.AreEqual(expectedOutgoing.ToList(), outgoingResult);
            });
        }

        [UnityTest]
        public IEnumerator RequestFriendshipAsync()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var cancellationToken = new CancellationToken();
                var friendId = "Friend ID";
                var message = "Hello friend";
                var createdAt = 1234;

                context.UpdateFriendshipResponse = new UpdateFriendshipResponse()
                {
                    Event = CreateRequestFriendshipResponse(friendId, message, createdAt),
                };

                var result = await rpcSocialApiBridge.RequestFriendshipAsync(friendId, message, cancellationToken);

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
                var cancellationToken = new CancellationToken();
                var friendId = "Friend ID";
                var createdAt = 123;
                var friendRequestId = $"{friendId}-{createdAt}";

                context.UpdateFriendshipResponse = new UpdateFriendshipResponse()
                {
                    Event = CreateRejectFriendshipResponse(friendId),
                };

                var resultUserId = "";

                rpcSocialApiBridge.OnFriendRequestRejected += userId => { resultUserId = userId; };

                await rpcSocialApiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken);

                Assert.AreEqual(friendId, resultUserId);
            });
        }

        [UnityTest]
        public IEnumerator SubscribeToFriendshipEventsEverySuccessfulAnswer()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var friendId = "Friend ID";
                var acceptFriendId = "Accept";
                var rejectFriendId = "Reject";
                var cancelFriendId = "Cancel";
                var deleteFriendId = "Delete";
                var createdAt = 123;
                var userProfile = ScriptableObject.CreateInstance<UserProfile>();
                userProfile.UpdateData(new UserProfileModel() { userId = "a user id" });

                var friendRequest = new RequestResponse()
                {
                    User = new User()
                    {
                        Address = friendId,
                    },
                    Message = "a message",
                    CreatedAt = createdAt
                };

                context.SubscribeFriendshipEventsUpdatesResponses = new List<SubscribeFriendshipEventsUpdatesResponse>()
                {
                    new ()
                    {
                        Events = new FriendshipEventResponses()
                        {
                            Responses =
                            {
                                new List<FriendshipEventResponse>()
                                {
                                    new ()
                                    {
                                        Accept = new AcceptResponse() { User = new User() { Address = acceptFriendId } },
                                    },
                                    new ()
                                    {
                                        Cancel = new CancelResponse() { User = new User() { Address = cancelFriendId } },
                                    },
                                    new ()
                                    {
                                        Reject = new RejectResponse() { User = new User() { Address = rejectFriendId } },
                                    },
                                    new ()
                                    {
                                        Delete = new DeleteResponse() { User = new User() { Address = deleteFriendId } },
                                    },
                                    new ()
                                    {
                                        Request = friendRequest,
                                    },
                                },
                            },
                        },
                    }
                };

                userProfileBridge.RequestFullUserProfileAsync(acceptFriendId, Arg.Any<CancellationToken>())
                                 .Returns(UniTask.FromResult(userProfile));

                var expectedFriendRequest = new FriendRequest(
                    $"{friendRequest.User.Address}-{friendRequest.CreatedAt}",
                    friendRequest.CreatedAt,
                    friendRequest.User.Address,
                    OWN_ID,
                    friendRequest.Message);

                var acceptedResult = "";
                var cancelResult = "";
                var deletedResult = "";
                var rejectResult = "";
                FriendRequest requestResult = null;

                rpcSocialApiBridge.OnFriendRequestAccepted += userId =>
                {
                    acceptedResult = userId;
                };

                rpcSocialApiBridge.OnFriendRequestCanceled += (userId) => { cancelResult = userId; };

                rpcSocialApiBridge.OnFriendRequestRejected += (userId) => { rejectResult = userId; };
                rpcSocialApiBridge.OnDeletedByFriend += (userId) => { deletedResult = userId; };
                rpcSocialApiBridge.OnIncomingFriendRequestAdded += (request) => { requestResult = request; };

                await rpcSocialApiBridge.InitializeAsync(default(CancellationToken));

                await UniTask.WaitUntil(() => !string.IsNullOrEmpty(acceptedResult));

                Assert.AreEqual(acceptFriendId, acceptedResult);

                Assert.AreEqual(cancelFriendId, cancelResult);
                Assert.AreEqual(rejectFriendId, rejectResult);
                Assert.AreEqual(deleteFriendId, deletedResult);
                Assert.AreEqual(expectedFriendRequest, requestResult);
            });
        }

        [UnityTest]
        public IEnumerator SubscribeToFriendshipEventsEveryFailedAnswer()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var rejectFriendId = "Reject";

                context.SubscribeFriendshipEventsUpdatesResponses = new List<SubscribeFriendshipEventsUpdatesResponse>()
                {
                    new ()
                    {
                        ForbiddenError = new ForbiddenError() { Message = "Forbidden" },
                    },
                    new ()
                    {
                        UnauthorizedError = new UnauthorizedError() { Message = "Unauthorized" },
                    },
                    new ()
                    {
                        InternalServerError = new InternalServerError() { Message = "Internal server" },
                    },
                    new ()
                    {
                        TooManyRequestsError = new TooManyRequestsError() { Message = "Too many requests" },
                    },
                    new ()
                    {
                        Events = new FriendshipEventResponses()
                        {
                            Responses =
                            {
                                new FriendshipEventResponse()
                                {
                                    Reject = new RejectResponse() { User = new User() { Address = rejectFriendId } }
                                }
                            }
                        }
                    }
                };

                var rejectResult = "";

                rpcSocialApiBridge.OnFriendRequestRejected += (userId) => { rejectResult = userId; };

                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Forbidden error Forbidden");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Unauthorized error Unauthorized");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got internal server error Internal server");
                LogAssert.Expect(LogType.Error, "Subscription to friendship events got Too many requests error Too many requests");

                await rpcSocialApiBridge.InitializeAsync(default(CancellationToken));

                await UniTask.WaitUntil(() => !string.IsNullOrEmpty(rejectResult));

                // Asserting that reject was correctly received and all errors were parsed correctly
                Assert.AreEqual(rejectFriendId, rejectResult);
            });
        }

        private static FriendshipEventResponse CreateRejectFriendshipResponse(string address)
        {
            return new FriendshipEventResponse()
            {
                Reject = new RejectResponse()
                {
                    User = new User()
                    {
                        Address = address
                    }
                }
            };
        }

        private static FriendshipEventResponse CreateRequestFriendshipResponse(string address, string message, long createdAt)
        {
            return new FriendshipEventResponse()
            {
                Request = new RequestResponse()
                {
                    User = new User()
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
                User = new User()
                {
                    Address = address,
                },
                CreatedAt = createdAt,
            };

        private static FriendRequest IncomingFriendRequestFromResponse(RequestResponse response) =>
            new FriendRequest(
                $"{response.User.Address}-{response.CreatedAt}",
                response.CreatedAt,
                response.User.Address,
                OWN_ID,
                response.Message);

        private static FriendRequest OutgoingFriendRequestFromResponse(RequestResponse response) =>
            new FriendRequest(
                $"{response.User.Address}-{response.CreatedAt}",
                response.CreatedAt,
                OWN_ID,
                response.User.Address,
                response.Message);
    }
}
