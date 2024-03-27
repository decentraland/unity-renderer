using Cysharp.Threading.Tasks;
using DCL.Social.Friends;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.MyAccount
{
    public class BlockedListController
    {
        private readonly IBlockedListComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        private readonly IBlockedListApiBridge blockedListApiBridge;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly IFriendsController friendsController;

        private CancellationTokenSource lifeTimeCancellationToken;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public BlockedListController(
            IBlockedListComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IBlockedListApiBridge blockedListApiBridge,
            ISocialAnalytics socialAnalytics,
            IFriendsController friendsController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.blockedListApiBridge = blockedListApiBridge;
            this.socialAnalytics = socialAnalytics;
            this.friendsController = friendsController;

            dataStore.myAccount.isMyAccountSectionVisible.OnChange += OnMyAccountSectionVisibleChanged;
            dataStore.myAccount.openSection.OnChange += OnMyAccountSectionTabChanged;

            ownUserProfile.OnUpdate += OnOwnUserProfileUpdated;
            view.OnUnblockUser += UnblockUser;
        }

        private void OnMyAccountSectionVisibleChanged(bool isVisible, bool _)
        {
            if (isVisible)
                OpenSection();
            else
                CloseSection();
        }

        private void OpenSection()
        {
            lifeTimeCancellationToken = lifeTimeCancellationToken.SafeRestart();
            UpdateBlockedUserList(ownUserProfile.blocked, lifeTimeCancellationToken.Token);
        }

        private void OnMyAccountSectionTabChanged(string currentOpenSection, string _)
        {
            if (currentOpenSection != MyAccountSection.BlockedList.ToString())
                return;
        }

        private void CloseSection()
        {
            lifeTimeCancellationToken.SafeCancelAndDispose();

            view.ClearAllEntries();
        }


        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null)
                return;
        }

        private void UnblockUser(string userId)
        {
            if (!ownUserProfile.IsBlocked(userId)) return;

            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateUnBlockUserData(
                userProfileBridge.Get(userId)?.userName,
                () =>
                {
                    ownUserProfile.Unblock(userId);
                    view.Remove(userId);
                    blockedListApiBridge.SendUnblockPlayer(userId);
                    socialAnalytics.SendPlayerUnblocked(friendsController.IsFriend(userId), PlayerActionSource.MyProfile, userId);

                }), true);
        }

        public void Dispose()
        {
            dataStore.myAccount.isMyAccountSectionVisible.OnChange -= OnMyAccountSectionVisibleChanged;
            dataStore.myAccount.openSection.OnChange -= OnMyAccountSectionTabChanged;
            ownUserProfile.OnUpdate -= OnOwnUserProfileUpdated;
        }

        private async void UpdateBlockedUserList(List<string> blockedUsersList, CancellationToken token)
        {
            view.SetupBlockedList();

            async UniTaskVoid UpdateChannelMembersAsync(IEnumerable<string> blockedUsers,
                CancellationToken cancellationToken)
            {
                view.SetLoadingActive(false);

                foreach (string member in blockedUsers)
                {
                    UserProfile memberProfile = userProfileBridge.Get(member);

                    try { memberProfile ??= await userProfileBridge.RequestFullUserProfileAsync(member, cancellationToken); }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        var fallbackBlockedUserEntry = new BlockedUserEntryModel
                        {
                            thumbnailUrl = "",
                            userId = member,
                            userName = member,
                        };

                        view.Set(fallbackBlockedUserEntry);
                    }

                    if (memberProfile != null)
                    {
                        var userToAdd = new BlockedUserEntryModel
                        {
                            thumbnailUrl = memberProfile.face256SnapshotURL,
                            userId = memberProfile.userId,
                            userName = memberProfile.userName,
                        };

                        view.Set(userToAdd);
                    }
                }

                view.SetLoadingActive(false);
            }

            UpdateChannelMembersAsync(blockedUsersList, token).Forget();
        }
    }
}
