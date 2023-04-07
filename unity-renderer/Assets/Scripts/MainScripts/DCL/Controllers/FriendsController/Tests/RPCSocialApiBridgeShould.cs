using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCl.Social.Friends;
using Decentraland.Social.Friendships;
using Google.Protobuf.WellKnownTypes;
using MainScripts.DCL.Controllers.FriendsController;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        private IRPC rpc;
        private RPCSocialApiBridge rpcSocialApiBridge;

        private const string OWN_ID = "My custom id";

        [SetUp]
        public void SetUp()
        {
            var usersList = new List<Users>()
            {
                new Users() { Users_ = { new User() { Address = "addr1" }, new User() { Address = "addr2" } } },
                new Users() { Users_ = { new User() { Address = "addr3" }, new User() { Address = "addr4" } } }
            };

            rpc = Substitute.For<IRPC>();
            var clientSocial = Substitute.For<IClientFriendshipsService>();

            clientSocial.GetFriends(new Payload() { SynapseToken = "" })
                        .Returns(UniTaskAsyncEnumerable.Create<Users>(async (writer, token) =>
                         {
                             foreach (var users in usersList)
                             {
                                 if (token.IsCancellationRequested) break;
                                 await writer.YieldAsync(users); // instead of `yield return`
                             }
                         }));

            clientSocial.GetRequestEvents(new Payload() { SynapseToken = "" })
                        .Returns(new UniTask<RequestEvents>(new RequestEvents()
                         {
                             Incoming = new Requests()
                             {
                                 Items =
                                 {
                                     new List<RequestResponse>()
                                     {
                                         new RequestResponse()
                                         {
                                             User = new User() { Address = "addr1" },
                                             Message = "",
                                             CreatedAt = 1678912696639
                                         },
                                         new RequestResponse()
                                         {
                                             User = new User() { Address = "addr2" },
                                             Message = "",
                                             CreatedAt = 1678912696639
                                         }
                                     }
                                 }
                             },
                             Outgoing = new Requests()
                             {
                                 Items =
                                 {
                                     new List<RequestResponse>()
                                     {
                                         new RequestResponse()
                                         {
                                             User = new User() { Address = "addr3" },
                                             Message = "",
                                             CreatedAt = 1678912696639
                                         },
                                         new RequestResponse()
                                         {
                                             User = new User() { Address = "addr4" },
                                             Message = "",
                                             CreatedAt = 1678912696639
                                         }
                                     }
                                 }
                             }
                         }));

            UserProfile.GetOwnUserProfile().UpdateData(new UserProfileModel() { userId = OWN_ID });

            rpc.Social().Returns(clientSocial);
            GameObject go = new GameObject();
            var component = go.AddComponent<MatrixInitializationBridge>();
            rpcSocialApiBridge = new RPCSocialApiBridge(rpc, component);
        }

        [UnityTest]
        public IEnumerator Initialize()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var cancellationToken = new CancellationToken();
                var friends = new Dictionary<string, UserStatus>();
                var incomingFriendRequests = new Dictionary<string, FriendRequest>();
                var outgoingFriendRequests = new Dictionary<string, FriendRequest>();

                rpcSocialApiBridge.OnFriendAdded += friend => { friends.Add(friend.userId, friend); };
                rpcSocialApiBridge.OnIncomingFriendRequestAdded += request => { incomingFriendRequests.Add(request.FriendRequestId, request); };
                rpcSocialApiBridge.OnOutgoingFriendRequestAdded += request => { outgoingFriendRequests.Add(request.FriendRequestId, request); };

                await rpcSocialApiBridge.InitializeFriendshipsInformation(cancellationToken);

                Assert.AreEqual(4, friends.Count);
                Assert.AreEqual(2, incomingFriendRequests.Count);
                Assert.AreEqual(2, outgoingFriendRequests.Count);
            });
        }
    }
}
