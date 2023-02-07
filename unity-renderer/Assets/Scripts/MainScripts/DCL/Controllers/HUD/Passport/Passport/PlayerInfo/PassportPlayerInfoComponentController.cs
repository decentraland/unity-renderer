using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.ProfanityFiltering;
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
        private readonly IClipboard clipboard;
        private readonly IPassportApiBridge passportApiBridge;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private string name;
        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("new_friend_requests");
        private bool isFriendsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("friends_enabled");
        public event Action OnClosePassport;
        private CancellationTokenSource cancellationTokenSource;

        public PassportPlayerInfoComponentController(
            StringVariable currentPlayerId,
            IPassportPlayerInfoComponentView view,
            DataStore dataStore,
            IProfanityFilter profanityFilter,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics,
            IClipboard clipboard,
            IPassportApiBridge passportApiBridge)
        {
            this.currentPlayerId = currentPlayerId;
            this.view = view;
            this.dataStore = dataStore;
            this.profanityFilter = profanityFilter;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;
            this.clipboard = clipboard;
            this.passportApiBridge = passportApiBridge;

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
            view.OnUsernameCopy += UsernameCopy;

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

        private void WalletCopy(string address)
        {
            clipboard.WriteText(address);
            socialAnalytics.SendCopyWallet(PlayerActionSource.Passport);
        }

        private void UsernameCopy(string username)
        {
            clipboard.WriteText(username);
            socialAnalytics.SendCopyUsername(PlayerActionSource.Passport);
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

            UpdateWithUserProfileAsync(userProfile, cancellationTokenSource.Token).Forget();
        }

        public void ClosePassport() =>
            view.ResetCopyToast();

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken)
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
                    friendshipStatus = await friendsController.GetFriendshipStatus(userProfile.userId, cancellationToken),
                    isFriendshipVisible = isFriendsEnabled,
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
            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateUnFriendData(
                UserProfileController.userProfilesCatalog.Get(currentPlayerId)?.userName,
                () =>
                {
                    friendsController.RemoveFriend(currentPlayerId);
                    socialAnalytics.SendFriendDeleted(UserProfile.GetOwnUserProfile().userId, currentPlayerId, PlayerActionSource.Passport);
                }), true);
        }

        private void CancelFriendRequest()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            CancelFriendRequestAsync(cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid CancelFriendRequestAsync(CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    await friendsController.CancelRequestByUserIdAsync(currentPlayerId, cancellationToken);
                    dataStore.HUDs.openSentFriendRequestDetail.Set(null, true);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(currentPlayerId, "modal",
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
                friendsController.CancelRequestByUserId(currentPlayerId);

            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, currentPlayerId,
                PlayerActionSource.Passport.ToString());
        }

        private void AcceptFriendRequest()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            AcceptFriendRequestAsync(cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid AcceptFriendRequestAsync(CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(currentPlayerId);
                    await friendsController.AcceptFriendshipAsync(request.FriendRequestId, cancellationToken);
                    dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, true);

                    socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId, PlayerActionSource.Passport.ToString(),
                        request.HasBodyMessage);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(currentPlayerId, "modal",
                        friendsController, socialAnalytics);

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

            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateBlockUserData(
                userProfileBridge.Get(currentPlayerId)?.userName,
                () =>
                {
                    ownUserProfile.Block(currentPlayerId);
                    view.SetIsBlocked(true);
                    passportApiBridge.SendBlockPlayer(currentPlayerId);
                    socialAnalytics.SendPlayerBlocked(friendsController.IsFriend(currentPlayerId), PlayerActionSource.Passport);
                }), true);
        }

        private void UnblockUser()
        {
            if (!ownUserProfile.IsBlocked(currentPlayerId)) return;
            ownUserProfile.Unblock(currentPlayerId);
            view.SetIsBlocked(false);
            passportApiBridge.SendUnblockPlayer(currentPlayerId);
            socialAnalytics.SendPlayerUnblocked(friendsController.IsFriend(currentPlayerId), PlayerActionSource.Passport);
        }

        private void ReportUser()
        {
            passportApiBridge.SendReportPlayer(currentPlayerId, name);
            socialAnalytics.SendPlayerReport(PlayerReportIssueType.None, 0, PlayerActionSource.Passport);
        }

        private void WhisperUser(string userId)
        {
            dataStore.HUDs.openChat.Set(userId, true);
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
