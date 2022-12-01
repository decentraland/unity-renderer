using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using Random = UnityEngine.Random;

namespace DCL.Social.Friends
{
    public class NewFriendRequestsApiBridgeMock : IFriendsApiBridge
    {
        private readonly WebInterfaceFriendsApiBridge apiBridge;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly Dictionary<string, FriendRequestPayload> friendRequests = new ();

        public event Action<FriendshipInitializationMessage> OnInitialized
        {
            add => apiBridge.OnInitialized += value;
            remove => apiBridge.OnInitialized -= value;
        }

        public event Action<string> OnFriendNotFound
        {
            add => apiBridge.OnFriendNotFound += value;
            remove => apiBridge.OnFriendNotFound -= value;
        }

        public event Action<AddFriendsPayload> OnFriendsAdded
        {
            add => apiBridge.OnFriendsAdded += value;
            remove => apiBridge.OnFriendsAdded -= value;
        }

        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded
        {
            add => apiBridge.OnFriendWithDirectMessagesAdded += value;
            remove => apiBridge.OnFriendWithDirectMessagesAdded -= value;
        }

        public event Action<UserStatus> OnUserPresenceUpdated
        {
            add => apiBridge.OnUserPresenceUpdated += value;
            remove => apiBridge.OnUserPresenceUpdated -= value;
        }

        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;

        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated
        {
            add => apiBridge.OnTotalFriendRequestCountUpdated += value;
            remove => apiBridge.OnTotalFriendRequestCountUpdated -= value;
        }

        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated
        {
            add => apiBridge.OnTotalFriendCountUpdated += value;
            remove => apiBridge.OnTotalFriendCountUpdated -= value;
        }

        public NewFriendRequestsApiBridgeMock(WebInterfaceFriendsApiBridge apiBridge,
            IUserProfileBridge userProfileBridge)
        {
            this.apiBridge = apiBridge;
            this.userProfileBridge = userProfileBridge;

            apiBridge.OnFriendshipStatusUpdated += message => OnFriendshipStatusUpdated?.Invoke(message);
        }

        public void RejectFriendship(string userId)
        {
            apiBridge.RejectFriendship(userId);
        }

        public void RemoveFriend(string userId)
        {
            apiBridge.RemoveFriend(userId);
        }

        public void GetFriends(int limit, int skip)
        {
            apiBridge.GetFriends(limit, skip);
        }

        public void GetFriends(string usernameOrId, int limit)
        {
            apiBridge.GetFriends(usernameOrId, limit);
        }

        public async UniTask<AddFriendRequestsPayload> GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip)
        {
            await UniTask.Delay(Random.Range(100, 1000));

            // FAKE RECEIVED REQUESTS
            int amountOfReceivedRequests = Random.Range(1, 11);
            List<FriendRequestPayload> requestedFromList = new List<FriendRequestPayload>();

            for (var i = 0; i < amountOfReceivedRequests; i++)
            {
                var fakeUserId = $"fake_from_user_{i + 1}";

                userProfileBridge.AddUserProfileToCatalog(new UserProfileModel
                {
                    userId = fakeUserId,
                    name = $"fake from user {i + 1}",
                    snapshots = new UserProfileModel.Snapshots
                    {
                        face256 = $"https://picsum.photos/50?{i}"
                    }
                });

                var friendRequestId = Guid.NewGuid().ToString("N");

                var friendRequest = new FriendRequestPayload
                {
                    from = fakeUserId,
                    to = userProfileBridge.GetOwn().userId,
                    friendRequestId = friendRequestId,
                    messageBody = Random.Range(0, 2) == 0 ? $"Test message from {fakeUserId}..." : string.Empty,
                    timestamp = DateTimeOffset.UtcNow.AddDays(-i).ToUnixTimeMilliseconds()
                };

                friendRequests[friendRequestId] = friendRequest;

                requestedFromList.Add(friendRequest);
            }

            // FAKE SENT REQUESTS
            int amountOfSentRequests = Random.Range(1, 11);
            List<FriendRequestPayload> requestedToList = new List<FriendRequestPayload>();

            for (var i = 0; i < amountOfSentRequests; i++)
            {
                var fakeUserId = $"fake_to_user_{i + 1}";

                userProfileBridge.AddUserProfileToCatalog(new UserProfileModel
                {
                    userId = fakeUserId,
                    name = $"fake to user {i + 1}",
                    snapshots = new UserProfileModel.Snapshots
                    {
                        face256 = $"https://picsum.photos/50?{i}"
                    }
                });

                var friendRequestId = Guid.NewGuid().ToString("N");

                var friendRequest = new FriendRequestPayload
                {
                    from = userProfileBridge.GetOwn().userId,
                    to = fakeUserId,
                    friendRequestId = friendRequestId,
                    messageBody = Random.Range(0, 2) == 0 ? $"Test message to {fakeUserId}..." : string.Empty,
                    timestamp = DateTimeOffset.UtcNow.AddDays(-i).ToUnixTimeMilliseconds()
                };

                friendRequests[friendRequestId] = friendRequest;

                requestedToList.Add(friendRequest);
            }

            var response = new AddFriendRequestsPayload
            {
                messageId = Guid.NewGuid().ToString("N"),
                requestedFrom = requestedFromList.ToArray(),
                requestedTo = requestedToList.ToArray(),
                totalReceivedFriendRequests = amountOfReceivedRequests,
                totalSentFriendRequests = amountOfSentRequests
            };

            return response;
        }

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip)
        {
            apiBridge.GetFriendsWithDirectMessages(usernameOrId, limit, skip);
        }

        public async UniTask<RequestFriendshipConfirmationPayload> RequestFriendship(string userId, string messageBody)
        {
            await UniTask.Delay(Random.Range(100, 16000));

            // TODO: add user profile to catalog if necessary

            // if (Random.Range(0, 2) == 0)
            //     throw new FriendshipException(FriendRequestErrorCodes.Unknown);

            var friendRequestId = Guid.NewGuid().ToString("N");

            var friendRequest = new FriendRequestPayload
            {
                from = userProfileBridge.GetOwn().userId,
                friendRequestId = friendRequestId,
                messageBody = messageBody,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                to = userId
            };

            friendRequests[friendRequestId] = friendRequest;

            var response = new RequestFriendshipConfirmationPayload
            {
                friendRequest = friendRequest,
                messageId = Guid.NewGuid().ToString("N")
            };

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.REQUESTED_TO,
                userId = userId
            });

            return response;
        }

        public async UniTask<CancelFriendshipConfirmationPayload> CancelRequest(string friendRequestId)
        {
            await UniTask.Delay(Random.Range(100, 16000));

            // if (Random.Range(0, 2) == 0)
            //     throw new FriendshipException(FriendRequestErrorCodes.Unknown);

            var response = new CancelFriendshipConfirmationPayload
            {
                messageId = Guid.NewGuid().ToString("N"),
                friendRequest = friendRequests[friendRequestId],
            };

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.CANCELLED,
                userId = friendRequests[friendRequestId].to
            });

            return response;
        }

        public UniTask CancelRequestByUserId(string userId) =>
            apiBridge.CancelRequestByUserId(userId);

        public void AcceptFriendship(string userId)
        {
            apiBridge.AcceptFriendship(userId);
        }
    }
}
