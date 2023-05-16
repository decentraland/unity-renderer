using SocialFeaturesAnalytics;
using System;
using UnityEngine;

namespace DCL.Social.Friends
{
    public static class ReportFriendRequestToAnalyticsExtensions
    {
        public static void ReportFriendRequestErrorToAnalyticsByRequestId(this Exception e, string friendRequestId,
            string source,
            IFriendsController friendsController, ISocialAnalytics socialAnalytics)
        {


            var description = e is FriendshipException fe
                ? fe.ErrorCode.ToString()
                : FriendRequestErrorCodes.Unknown.ToString();

            if (!friendsController.TryGetAllocatedFriendRequest(friendRequestId, out FriendRequest request))
            {
                Debug.LogError($"Cannot display friend request {friendRequestId}, is not allocated");

                socialAnalytics.SendFriendRequestError(null, null,
                    source,
                    description);

                return;
            }

            socialAnalytics.SendFriendRequestError(request.From, request.To,
                source,
                description);
        }

        public static void ReportFriendRequestErrorToAnalyticsByUserId(this Exception e, string userId,
            string source,
            IFriendsController friendsController, ISocialAnalytics socialAnalytics)
        {
            FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(userId);

            socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                source,
                e is FriendshipException fe
                    ? fe.ErrorCode.ToString()
                    : FriendRequestErrorCodes.Unknown.ToString());
        }

        public static void ReportFriendRequestErrorToAnalyticsAsSender(this Exception e, string recipientId,
            string source,
            IUserProfileBridge userProfileBridge, ISocialAnalytics socialAnalytics)
        {
            socialAnalytics.SendFriendRequestError(userProfileBridge.GetOwn()?.userId, recipientId,
                source,
                e is FriendshipException fe
                    ? fe.ErrorCode.ToString()
                    : FriendRequestErrorCodes.Unknown.ToString());
        }
    }
}
