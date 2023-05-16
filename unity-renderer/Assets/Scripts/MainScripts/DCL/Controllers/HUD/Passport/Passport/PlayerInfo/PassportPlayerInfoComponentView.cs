using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentView : BaseComponentView<PlayerPassportModel>, IPassportPlayerInfoComponentView
    {
        private const int COPY_TOAST_VISIBLE_TIME = 3000;

        [SerializeField] private TextMeshProUGUI name;
        [SerializeField] private TextMeshProUGUI address;
        [SerializeField] private TextMeshProUGUI nameInOptionsPanel;
        [SerializeField] private Button walletCopyButton;
        [SerializeField] private Button usernameCopyButton;
        [SerializeField] private RectTransform usernameRect;
        [SerializeField] private TextMeshProUGUI wallet;
        [SerializeField] private ButtonComponentView optionsButton;
        [SerializeField] private ButtonComponentView addFriendButton;
        [SerializeField] private ButtonComponentView alreadyFriendsButton;
        [SerializeField] private ButtonComponentView cancelFriendRequestButton;
        [SerializeField] private ButtonComponentView acceptFriendButton;
        [SerializeField] private ButtonComponentView blockedFriendButton;
        [SerializeField] private GameObject friendshipContainer;
        [SerializeField] private Button whisperButton;
        [SerializeField] private TooltipComponentView whisperNonFriendsPopup;
        [SerializeField] private GameObject onlineStatus;
        [SerializeField] private GameObject offlineStatus;
        [SerializeField] private GameObject normalUserPanel;
        [SerializeField] private GameObject friendsFlowContainer;
        [SerializeField] private GameObject blockedLabel;
        [SerializeField] private GameObject optionsContainer;
        [SerializeField] private UserContextMenu userContextMenu;

        [SerializeField] private GameObject alreadyFriendsVariation;
        [SerializeField] private GameObject unfriendVariation;

        [SerializeField] private JumpInButton jumpInButton;
        [SerializeField] private ShowHideAnimator copyAddressToast;
        [SerializeField] private ShowHideAnimator copyUsernameToast;
        [SerializeField] private GameObject actionsContainer;

        public event Action OnAddFriend;
        public event Action OnRemoveFriend;
        public event Action OnCancelFriendRequest;
        public event Action OnAcceptFriendRequest;
        public event Action OnBlockUser;
        public event Action OnUnblockUser;
        public event Action OnReportUser;
        public event Action<string> OnWhisperUser;
        public event Action OnJumpInUser;
        public event Action<string> OnWalletCopy;
        public event Action<string> OnUsernameCopy;

        private string fullWalletAddress;
        private bool areFriends;
        private bool isBlocked = false;
        private Dictionary<FriendshipStatus, GameObject> friendStatusButtonsMapping;
        private CancellationTokenSource cts;

        public void Start()
        {
            walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
            usernameCopyButton.onClick.AddListener(CopyUsernameToClipboard);
            addFriendButton.onClick.AddListener(() => OnAddFriend?.Invoke());
            alreadyFriendsButton.onClick.AddListener(() => OnRemoveFriend?.Invoke());
            cancelFriendRequestButton.onClick.AddListener(() => OnCancelFriendRequest?.Invoke());
            acceptFriendButton.onClick.AddListener(() => OnAcceptFriendRequest?.Invoke());
            userContextMenu.OnBlock += OnBlock;
            blockedFriendButton.onClick.AddListener(() => OnUnblockUser?.Invoke());
            userContextMenu.OnReport += OnReport;
            whisperButton.onClick.AddListener(WhisperActionFlow);
            optionsButton.onClick.AddListener(OpenOptions);
            jumpInButton.OnClick += () => OnJumpInUser?.Invoke();
            alreadyFriendsButton.onFocused += RemoveFriendsFocused;

            friendStatusButtonsMapping = new Dictionary<FriendshipStatus, GameObject>()
            {
                { FriendshipStatus.NOT_FRIEND, addFriendButton.gameObject },
                { FriendshipStatus.FRIEND, alreadyFriendsButton.gameObject },
                { FriendshipStatus.REQUESTED_FROM, acceptFriendButton.gameObject },
                { FriendshipStatus.REQUESTED_TO, cancelFriendRequestButton.gameObject }
            };
        }

        private void OnReport(string userId)
        {
            OnReportUser?.Invoke();
        }

        private void OnBlock(string userId, bool isBlocked)
        {
            model.isBlocked = isBlocked;
            RefreshControl();
            OnBlockUser?.Invoke();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetGuestUser(model.isGuest);
            SetName(model.name);
            userContextMenu.Hide();
            SetWallet(model.userId);
            SetPresence(model.presenceStatus);
            SetIsBlocked(model.isBlocked);
            SetHasBlockedOwnUser(model.hasBlocked);
            SetFriendStatus(model.friendshipStatus);
            SetFriendshipVisibility(model.isFriendshipVisible);
        }

        public override void Dispose()
        {
            base.Dispose();

            walletCopyButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.RemoveAllListeners();

            ResetCancellationToken();
        }

        public void ResetCopyToast()
        {
            copyAddressToast.Hide(true);
            copyUsernameToast.Hide(true);
            ResetCancellationToken();
        }

        public void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics)
        {
            if (friendsController.IsFriend(userId))
            {
                jumpInButton.gameObject.SetActive(true);
                jumpInButton.Initialize(friendsController, userId, socialAnalytics);
            }
            else { jumpInButton.gameObject.SetActive(false); }
        }

        public void SetFriendStatus(FriendshipStatus friendStatus)
        {
            areFriends = friendStatus == FriendshipStatus.FRIEND;

            if (isBlocked)
                return;

            DisableAllFriendFlowButtons();
            friendStatusButtonsMapping[friendStatus].SetActive(true);
            whisperNonFriendsPopup.Hide(true);
        }

        private void SetFriendshipVisibility(bool visible) =>
            friendshipContainer.SetActive(visible);

        private void RemoveFriendsFocused(bool isFocused)
        {
            alreadyFriendsVariation.SetActive(!isFocused);
            unfriendVariation.SetActive(isFocused);
        }

        private void SetName(string name)
        {
            string[] splitName = name.Split('#');
            this.name.SetText(splitName[0]);
            address.SetText($"{(splitName.Length == 2 ? "#" + splitName[1] : "")}");

            //We are forced to use this due to the UI not being correctly responsive with the placing of the copy icon
            //without the force rebuild it's not setting the elements as dirty and not replacing them correctly
            Utils.ForceRebuildLayoutImmediate(usernameRect);
            nameInOptionsPanel.text = name;
        }

        private void SetWallet(string wallet)
        {
            if (string.IsNullOrEmpty(wallet))
                return;

            fullWalletAddress = wallet;
            this.wallet.text = $"{wallet.Substring(0, 5)}...{wallet.Substring(wallet.Length - 5)}";
        }

        public void SetIsBlocked(bool isBlocked)
        {
            this.isBlocked = isBlocked;
            DisableAllFriendFlowButtons();

            blockedFriendButton.gameObject.SetActive(isBlocked);
            optionsContainer.SetActive(!isBlocked);
            blockedLabel.SetActive(isBlocked);

            if (!isBlocked) { SetFriendStatus(model.friendshipStatus); }
        }

        public void SetActionsActive(bool isActive) =>
            actionsContainer.SetActive(isActive);

        private void SetPresence(PresenceStatus status)
        {
            if (status == PresenceStatus.ONLINE)
            {
                onlineStatus.SetActive(true);
                offlineStatus.SetActive(false);
            }
            else
            {
                onlineStatus.SetActive(false);
                offlineStatus.SetActive(true);
            }
        }

        private void SetGuestUser(bool isGuest)
        {
            normalUserPanel.SetActive(!isGuest);
        }

        private void SetHasBlockedOwnUser(bool hasBlocked)
        {
            friendsFlowContainer.SetActive(!hasBlocked);
        }

        private void DisableAllFriendFlowButtons()
        {
            alreadyFriendsButton.gameObject.SetActive(false);
            addFriendButton.gameObject.SetActive(false);
            cancelFriendRequestButton.gameObject.SetActive(false);
            acceptFriendButton.gameObject.SetActive(false);
            blockedFriendButton.gameObject.SetActive(false);
        }

        private void CopyWalletToClipboard()
        {
            if (fullWalletAddress == null)
                return;

            OnWalletCopy?.Invoke(fullWalletAddress);
            ResetCopyToast();
            cts = new CancellationTokenSource();
            ShowCopyToast(copyAddressToast, cts.Token).Forget();
        }

        private void CopyUsernameToClipboard()
        {
            if (string.IsNullOrEmpty(model.name))
                return;

            OnUsernameCopy?.Invoke(model.name);
            ResetCopyToast();
            cts = new CancellationTokenSource();
            ShowCopyToast(copyUsernameToast, cts.Token).Forget();
        }

        private async UniTaskVoid ShowCopyToast(ShowHideAnimator toast, CancellationToken ct)
        {
            if (!toast.gameObject.activeSelf) { toast.gameObject.SetActive(true); }

            toast.Show();
            await Task.Delay(COPY_TOAST_VISIBLE_TIME, ct);
            toast.Hide();
        }

        private void ResetCancellationToken()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        private void WhisperActionFlow()
        {
            if (areFriends) { OnWhisperUser?.Invoke(model.userId); }
            else
            {
                if (areFriends) { whisperNonFriendsPopup.Hide(); }
                else { whisperNonFriendsPopup.Show(); }
            }
        }

        private void OpenOptions()
        {
            userContextMenu.Show(model.userId);
        }
    }
}
