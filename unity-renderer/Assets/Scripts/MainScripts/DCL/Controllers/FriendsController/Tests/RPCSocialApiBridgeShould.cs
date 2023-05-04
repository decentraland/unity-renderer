using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using NSubstitute;
using NSubstitute.Extensions;
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

            var friendshipsService = Substitute.For<IFriendshipsService<MockSocialServerContext>>();

            friendshipsService.GetFriends(Arg.Any<Payload>(), Arg.Any<MockSocialServerContext>())
                              .Returns(callInfo =>
                               {
                                   MockSocialServerContext context = (MockSocialServerContext)callInfo[1];

                                   return UniTaskAsyncEnumerable.Create<Users>(async (writer, token) =>
                                   {
                                       foreach (var users in context.userList)
                                       {
                                           if (token.IsCancellationRequested) break;
                                           await writer.YieldAsync(users);
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

                userProfile.UpdateData(new UserProfileModel()
                {
                    name = "Random name",
                });

                userProfileBridge.RequestFullUserProfileAsync(Arg.Any<string>())
                                 .Returns(new UniTask<UserProfile>(userProfile));

                rpcSocialApiBridge.OnFriendAdded += friend =>
                {
                    if (string.IsNullOrEmpty(friend.userName)) { throw new InvalidCastException("Friend doesn't have a name"); }

                    friends.Add(friend.userId, friend);
                };

                await rpcSocialApiBridge.GetInitializationInformationAsync(cancellationToken);

                Assert.AreEqual(4, friends.Count);
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

        [UnityTest]
        public IEnumerator RejectFriendshipAsyncKnownErrorResponse()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var cancellationToken = new CancellationToken();
                var friendId = "Friend ID";
                var createdAt = 123;
                var friendRequestId = $"{friendId}-{createdAt}";

                context.updateFriendshipResponse = new UpdateFriendshipResponse()
                {
                    Error = FriendshipErrorCode.BlockedUser
                };

                var resultUserId = "";

                rpcSocialApiBridge.OnFriendRequestRemoved += userId => { resultUserId = userId; };

                Assert.ThrowsAsync<FriendshipException>(
                    async () => await rpcSocialApiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken)
                );
            });
        }

        [UnityTest]
        public IEnumerator RejectFriendshipAsyncNoneResponse()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var cancellationToken = new CancellationToken();
                var friendId = "Friend ID2";
                var createdAt = 1234;
                var friendRequestId = $"{friendId}-{createdAt}";

                context.updateFriendshipResponse = new UpdateFriendshipResponse()
                    { };

                Assert.ThrowsAsync<NotSupportedException>(
                    async () => await rpcSocialApiBridge.RejectFriendshipAsync(friendRequestId, cancellationToken)
                );
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
    }
}
