using SocialFeaturesAnalytics;
using System;

namespace DCL.Social.Friends
{
    public static class ReportFriendRequestToAnalyticsExtensions
    {
        public static void ReportFriendRequestErrorToAnalyticsByRequestId(this Exception e, string friendRequestId,
            string source,
            IFriendsController friendsController, ISocialAnalytics socialAnalytics)
        {
            FriendRequest request = friendsController.GetAllocatedFriendRequest(friendRequestId);
            socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                source,
                e is FriendshipException fe
                    ? fe.ErrorCode.ToString()
                    : FriendRequestErrorCodes.Unknown.ToString());
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
