using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using System.Threading;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentController : IDisposable
    {
        private readonly IPassportPlayerInfoComponentView view;
        private readonly DataStore dataStore;
        private readonly IProfanityFilter profanityFilter;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly StringVariable currentPlayerId;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private string name;
        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("new_friend_requests");
        public event Action OnClosePassport;
        private CancellationTokenSource cancellationTokenSource;

        public PassportPlayerInfoComponentController(
            StringVariable currentPlayerId,
            IPassportPlayerInfoComponentView view,
            DataStore dataStore,
            IProfanityFilter profanityFilter,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics)
        {
            this.currentPlayerId = currentPlayerId;
            this.view = view;
            this.dataStore = dataStore;
            this.profanityFilter = profanityFilter;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;

            view.OnAddFriend += AddPlayerAsFriend;
            view.OnRemoveFriend += RemoveFriend;
            view.OnCancelFriendRequest += CancelFriendRequest;
            view.OnAcceptFriendRequest += AcceptFriendRequest;
            view.OnBlockUser += BlockUser;
            view.OnUnblockUser += UnblockUser;
            view.OnReportUser += ReportUser;
            view.OnWhisperUser += WhisperUser;
            view.OnJumpInUser += JumpInUser;
            view.OnWalletCopy += WalletCopy;

            friendsController.OnUpdateFriendship += UpdateFriendshipStatus;
        }

        public void Dispose()
        {
            view.Dispose();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            friendsController.OnUpdateFriendship -= UpdateFriendshipStatus;
        }

        private void WalletCopy()
        {
            socialAnalytics.SendWalletCopy(PlayerActionSource.Passport);
        }

        private void JumpInUser()
        {
            socialAnalytics.SendJumpInToPlayer(PlayerActionSource.Passport);
            OnClosePassport?.Invoke();
        }

        public void UpdateWithUserProfile(UserProfile userProfile)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            // this is ugly, but nothing in the inner function works with cancellation token, we have to refactor FriendsController to be able to do it properly
            UpdateWithUserProfileAsync(userProfile).AttachExternalCancellation(cancellationTokenSource.Token).Forget();
        }

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            name = userProfile.name;
            string filteredName = await FilterName(userProfile);
            PlayerPassportModel playerPassportModel;

            if (userProfile.isGuest)
            {
                playerPassportModel = new PlayerPassportModel
                {
                    name = filteredName,
                    isGuest = userProfile.isGuest,
                };
            }
            else
            {
                playerPassportModel = new PlayerPassportModel
                {
                    name = filteredName,
                    userId = userProfile.userId,
                    presenceStatus = friendsController.GetUserStatus(userProfile.userId).presence,
                    isGuest = userProfile.isGuest,
                    isBlocked = ownUserProfile.IsBlocked(userProfile.userId),
                    hasBlocked = userProfile.IsBlocked(ownUserProfile.userId),
                    friendshipStatus = await friendsController.GetFriendshipStatus(userProfile.userId),
                };
            }

            view.SetModel(playerPassportModel);
            view.InitializeJumpInButton(friendsController, userProfile.userId, socialAnalytics);
            view.SetActionsActive(userProfile.userId != ownUserProfile.userId);
        }

        private async UniTask<string> FilterName(UserProfile userProfile)
        {
            return IsProfanityFilteringEnabled()
                ? await profanityFilter.Filter(userProfile.userName)
                : userProfile.userName;
        }

        private bool IsProfanityFilteringEnabled()
        {
            return dataStore.settings.profanityChatFilteringEnabled.Get();
        }

        private void AddPlayerAsFriend()
        {
            if (isNewFriendRequestsEnabled)
                dataStore.HUDs.sendFriendRequest.Set(currentPlayerId, true);
            else
            {
                friendsController.RequestFriendship(currentPlayerId);
                socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, currentPlayerId, 0, PlayerActionSource.Passport);
            }
        }

        private void RemoveFriend()
        {
            friendsController.RemoveFriend(currentPlayerId);
            socialAnalytics.SendFriendDeleted(UserProfile.GetOwnUserProfile().userId, currentPlayerId, PlayerActionSource.Passport);
        }

        private void CancelFriendRequest() =>
            CancelFriendRequestAsync().Forget();

        private async UniTaskVoid CancelFriendRequestAsync()
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    await friendsController.CancelRequestByUserIdAsync(currentPlayerId).Timeout(TimeSpan.FromSeconds(10));
                    dataStore.HUDs.openSentFriendRequestDetail.Set(null, true);
                }
                catch (Exception e)
                {
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(currentPlayerId);

                    socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                        "modal",
                        e is FriendshipException fe
                            ? fe.ErrorCode.ToString()
                            : FriendRequestErrorCodes.Unknown.ToString());

                    throw;
                }
            }
            else
                friendsController.CancelRequestByUserId(currentPlayerId);

            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, currentPlayerId,
                PlayerActionSource.Passport.ToString());
        }

        private void AcceptFriendRequest() =>
            AcceptFriendRequestAsync().Forget();

        private async UniTaskVoid AcceptFriendRequestAsync()
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(currentPlayerId);
                    await friendsController.AcceptFriendshipAsync(request.FriendRequestId).Timeout(TimeSpan.FromSeconds(10));
                    dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, true);

                    socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId, PlayerActionSource.Passport.ToString(),
                        request.HasBodyMessage);
                }
                catch (Exception e)
                {
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(currentPlayerId);

                    socialAnalytics.SendFriendRequestError(request?.From, request?.To,
                        "modal",
                        e is FriendshipException fe
                            ? fe.ErrorCode.ToString()
                            : FriendRequestErrorCodes.Unknown.ToString());

                    throw;
                }
            }
            else
            {
                friendsController.AcceptFriendship(currentPlayerId);

                socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId, PlayerActionSource.Passport.ToString(),
                    false);
            }
        }

        private void BlockUser()
        {
            if (ownUserProfile.IsBlocked(currentPlayerId)) return;
            ownUserProfile.Block(currentPlayerId);
            view.SetIsBlocked(true);
            WebInterface.SendBlockPlayer(currentPlayerId);
            socialAnalytics.SendPlayerBlocked(friendsController.IsFriend(currentPlayerId), PlayerActionSource.Passport);
        }

        private void UnblockUser()
        {
            if (!ownUserProfile.IsBlocked(currentPlayerId)) return;
            ownUserProfile.Unblock(currentPlayerId);
            view.SetIsBlocked(false);
            WebInterface.SendUnblockPlayer(currentPlayerId);
            socialAnalytics.SendPlayerUnblocked(friendsController.IsFriend(currentPlayerId), PlayerActionSource.Passport);
        }

        private void ReportUser()
        {
            WebInterface.SendReportPlayer(currentPlayerId, name);
            socialAnalytics.SendPlayerReport(PlayerReportIssueType.None, 0, PlayerActionSource.Passport);
        }

        private void WhisperUser(string userId)
        {
            dataStore.HUDs.openPrivateChat.Set(userId);
            socialAnalytics.SendStartedConversation(PlayerActionSource.Passport);
            OnClosePassport?.Invoke();
        }

        private void UpdateFriendshipStatus(string userId, FriendshipAction action)
        {
            if (userId != currentPlayerId) return;
            view.SetFriendStatus(ToFriendshipStatus(action));
        }

        private FriendshipStatus ToFriendshipStatus(FriendshipAction action)
        {
            switch (action)
            {
                case FriendshipAction.APPROVED:
                    return FriendshipStatus.FRIEND;
                case FriendshipAction.REQUESTED_TO:
                    return FriendshipStatus.REQUESTED_TO;
                case FriendshipAction.REQUESTED_FROM:
                    return FriendshipStatus.REQUESTED_FROM;
                case FriendshipAction.NONE:
                case FriendshipAction.DELETED:
                case FriendshipAction.REJECTED:
                case FriendshipAction.CANCELLED:
                default:
                    return FriendshipStatus.NOT_FRIEND;
            }
        }
    }
}
