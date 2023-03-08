using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
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
        private readonly BaseVariable<KeyValuePair<string, string>> currentPlayerId;
        private readonly IClipboard clipboard;
        private readonly IPassportApiBridge passportApiBridge;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();
        private string name;
        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("new_friend_requests");
        private bool isFriendsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled("friends_enabled");
        public event Action OnClosePassport;
        private CancellationTokenSource cancellationTokenSource;

        public PassportPlayerInfoComponentController(
            IPassportPlayerInfoComponentView view,
            DataStore dataStore,
            IProfanityFilter profanityFilter,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics,
            IClipboard clipboard,
            IPassportApiBridge passportApiBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.profanityFilter = profanityFilter;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;
            this.clipboard = clipboard;
            this.passportApiBridge = passportApiBridge;
            this.currentPlayerId = dataStore.HUDs.currentPlayerId;

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
                dataStore.HUDs.sendFriendRequest.Set(currentPlayerId.Get().Key, true);
            else
            {
                friendsController.RequestFriendship(currentPlayerId.Get().Key);
                socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, currentPlayerId.Get().Key, 0, PlayerActionSource.Passport);
            }
        }

        private void RemoveFriend()
        {
            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateUnFriendData(
                UserProfileController.userProfilesCatalog.Get(currentPlayerId.Get().Key)?.userName,
                () =>
                {
                    friendsController.RemoveFriend(currentPlayerId.Get().Key);
                    socialAnalytics.SendFriendDeleted(UserProfile.GetOwnUserProfile().userId, currentPlayerId.Get().Key, PlayerActionSource.Passport);
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
                    await friendsController.CancelRequestByUserIdAsync(currentPlayerId.Get().Key, cancellationToken);
                    dataStore.HUDs.openSentFriendRequestDetail.Set(null, true);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(currentPlayerId.Get().Key, "modal",
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
                friendsController.CancelRequestByUserId(currentPlayerId.Get().Key);

            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, currentPlayerId.Get().Key,
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
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(currentPlayerId.Get().Key);
                    await friendsController.AcceptFriendshipAsync(request.FriendRequestId, cancellationToken);
                    dataStore.HUDs.openReceivedFriendRequestDetail.Set(null, true);

                    socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId.Get().Key, PlayerActionSource.Passport.ToString(),
                        request.HasBodyMessage);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(currentPlayerId.Get().Key, "modal",
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
            {
                friendsController.AcceptFriendship(currentPlayerId.Get().Key);

                socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, currentPlayerId.Get().Key, PlayerActionSource.Passport.ToString(),
                    false);
            }
        }

        private void BlockUser()
        {
            if (ownUserProfile.IsBlocked(currentPlayerId.Get().Key)) return;

            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateBlockUserData(
                userProfileBridge.Get(currentPlayerId.Get().Key)?.userName,
                () =>
                {
                    ownUserProfile.Block(currentPlayerId.Get().Key);
                    view.SetIsBlocked(true);
                    passportApiBridge.SendBlockPlayer(currentPlayerId.Get().Key);
                    socialAnalytics.SendPlayerBlocked(friendsController.IsFriend(currentPlayerId.Get().Key), PlayerActionSource.Passport);
                }), true);
        }

        private void UnblockUser()
        {
            if (!ownUserProfile.IsBlocked(currentPlayerId.Get().Key)) return;
            ownUserProfile.Unblock(currentPlayerId.Get().Key);
            view.SetIsBlocked(false);
            passportApiBridge.SendUnblockPlayer(currentPlayerId.Get().Key);
            socialAnalytics.SendPlayerUnblocked(friendsController.IsFriend(currentPlayerId.Get().Key), PlayerActionSource.Passport);
        }

        private void ReportUser()
        {
            passportApiBridge.SendReportPlayer(currentPlayerId.Get().Key, name);
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
            if (userId != currentPlayerId.Get().Key) return;
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
