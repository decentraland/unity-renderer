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
        public List<Users> userList = new List<Users>();

        public UpdateFriendshipResponse updateFriendshipResponse;
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
                                       foreach (var users in context.userList)
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
                                   return UniTask.FromResult(context.updateFriendshipResponse);
                               });

            var rpcServer = new RpcServer<MockSocialServerContext>();
            rpcServer.SetHandler((port, _, _) => { FriendshipsServiceCodeGen.RegisterService(port, friendshipsService); });

            context = new MockSocialServerContext();
            rpcServer.AttachTransport(server, context);

            var matrixInitializationBridge = Substitute.For<IMatrixInitializationBridge>();

            rpcSocialApiBridge = new RPCSocialApiBridge(matrixInitializationBridge, userProfileBridge, () => client);
            rpcSocialApiBridge.Initialize();
            yield return rpcSocialApiBridge.InitializeAsync(default(CancellationToken)).ToCoroutine();

            matrixInitializationBridge.OnReceiveMatrixAccessToken += Raise.Event<Action<string>>(ACCESS_TOKEN);
        }

        [UnityTest]
        public IEnumerator Initialize()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var cancellationToken = new CancellationToken();
                var friends = new Dictionary<string, UserStatus>();

                context.userList = new List<Users>()
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

                rpcSocialApiBridge.OnFriendAdded += friend =>
                {
                    if (string.IsNullOrEmpty(friend.userName)) { throw new Exception("User should have a userName"); }

                    friends.Add(friend.userId, friend);
                };

                userProfileBridge.RequestFullUserProfileAsync(Arg.Any<string>())
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

                var response = await rpcSocialApiBridge.GetInitializationInformationAsync(cancellationToken);

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

                context.updateFriendshipResponse = new UpdateFriendshipResponse()
                {
                    Event = CreateRequestFriendshipResponse(message, createdAt),
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
                var friends = new Dictionary<string, UserStatus>();
                var friendId = "Friend ID";
                var createdAt = 123;
                var friendRequestId = $"{friendId}-{createdAt}";

                context.updateFriendshipResponse = new UpdateFriendshipResponse()
                {
                    Event = CreateRejectFriendshipResponse(),
                };

                var resultUserId = "";

                rpcSocialApiBridge.OnFriendRequestRemoved += userId => { resultUserId = userId; };

                await rpcSocialApiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken);

                Assert.AreEqual(friendId, resultUserId);
            });
        }

        private static FriendshipEventResponse CreateRejectFriendshipResponse()
        {
            return new FriendshipEventResponse()
            {
                Reject = new RejectResponse()
                {
                    User = new User()
                    {
                        Address = OWN_ID
                    }
                }
            };
        }

        private static FriendshipEventResponse CreateRequestFriendshipResponse(String message, long createdAt)
        {
            return new FriendshipEventResponse()
            {
                Request = new RequestResponse()
                {
                    User = new User()
                    {
                        Address = OWN_ID
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
