using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Social.Friendships;
using MainScripts.DCL.Controllers.FriendsController;
using NSubstitute;
using rpc_csharp.transport;
using System.Collections;
using System.Collections.Generic;
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

        [UnitySetUp]
        public IEnumerator SetUp()
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

            GameObject go = new GameObject();
            var component = go.AddComponent<MatrixInitializationBridge>();
            var userProfileBridge = Substitute.For<IUserProfileBridge>();

            var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownProfile.UpdateData(new UserProfileModel { userId = OWN_ID });
            userProfileBridge.GetOwn().Returns(ownProfile);

            ITransport TransportProvider()
            {
                (var client, var server) = MemoryTransport.Create();
                return client;
            }

            rpcSocialApiBridge = new RPCSocialApiBridge(component, userProfileBridge, TransportProvider);
            rpcSocialApiBridge.Initialize();
            yield return rpcSocialApiBridge.InitializeAsync(default(CancellationToken)).ToCoroutine();
        }

        // [UnityTest]
        // public IEnumerator Initialize()
        // {
        //     return UniTask.ToCoroutine(async () =>
        //     {
        //         var cancellationToken = new CancellationToken();
        //         var friends = new Dictionary<string, UserStatus>();
        //         var friendRequests = new Dictionary<string, FriendRequest>();
        //
        //         rpcSocialApiBridge.OnFriendAdded += friend => { friends.Add(friend.userId, friend); };
        //         rpcSocialApiBridge.OnFriendRequestAdded += request => { friendRequests.Add(request.FriendRequestId, request); };
        //
        //         await rpcSocialApiBridge.InitializeFriendshipsInformation(cancellationToken);
        //
        //         Assert.AreEqual(4, friends.Count);
        //         Assert.AreEqual(2, friendRequests.Count(friendRequest => friendRequest.Value.From == OWN_ID));
        //         Assert.AreEqual(2, friendRequests.Count(friendRequest => friendRequest.Value.To == OWN_ID));
        //     });
        // }
    }
}
