using DCl.Social.Friends;
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using SocialFeaturesAnalytics;
using DCL.Social.Friends;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentView : BaseComponentView<PlayerPassportModel>, IPassportPlayerInfoComponentView
    {
        [SerializeField] private TextMeshProUGUI name;
        [SerializeField] private TextMeshProUGUI nameInOptionsPanel;
        [SerializeField] private Button walletCopyButton;
        [SerializeField] private TextMeshProUGUI wallet;
        [SerializeField] private ButtonComponentView optionsButton;
        [SerializeField] private ButtonComponentView addFriendButton;
        [SerializeField] private ButtonComponentView alreadyFriendsButton;
        [SerializeField] private ButtonComponentView cancelFriendRequestButton;
        [SerializeField] private ButtonComponentView acceptFriendButton;
        [SerializeField] private ButtonComponentView blockedFriendButton;
        [SerializeField] private Button whisperButton;
        [SerializeField] private GameObject whisperNonFriendsPopup;
        [SerializeField] private GameObject onlineStatus;
        [SerializeField] private GameObject offlineStatus;
        [SerializeField] private GameObject normalUserPanel;
        [SerializeField] private GameObject friendsFlowContainer;
        [SerializeField] private UserContextMenu userContextMenu;

        [SerializeField] private GameObject alreadyFriendsVariation;
        [SerializeField] private GameObject unfriendVariation;

        [SerializeField] private GameObject alreadyBlockedVariation;
        [SerializeField] private GameObject unblockVariation;

        [SerializeField] private JumpInButton jumpInButton;

        public event Action OnAddFriend;
        public event Action OnRemoveFriend;
        public event Action OnCancelFriendRequest;
        public event Action OnAcceptFriendRequest;
        public event Action OnBlockUser;
        public event Action OnUnblockUser;
        public event Action OnReportUser;

        private string fullWalletAddress;
        private bool areFriends;
        private bool isBlocked = false;

        public override void Start()
        {
            walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
            addFriendButton.onClick.AddListener(()=>OnAddFriend?.Invoke());
            alreadyFriendsButton.onClick.AddListener(()=>OnRemoveFriend?.Invoke());
            cancelFriendRequestButton.onClick.AddListener(()=>OnCancelFriendRequest?.Invoke());
            acceptFriendButton.onClick.AddListener(()=>OnAcceptFriendRequest?.Invoke());
            userContextMenu.OnBlock += OnBlock;
            blockedFriendButton.onClick.AddListener(()=>OnUnblockUser?.Invoke());
            userContextMenu.OnReport += OnReport;
            whisperButton.onClick.AddListener(WhisperActionFlow);
            optionsButton.onClick.AddListener(OpenOptions);

            alreadyFriendsButton.onFocused += RemoveFriendsFocused;
            blockedFriendButton.onFocused += BlockFriendFocused;
        }

        private void OnReport(string Obj)
        {
            OnReportUser?.Invoke();
        }

        private void OnBlock(string Arg1, bool Arg2)
        {
            OnBlockUser?.Invoke();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            userContextMenu.Hide();
            SetName(model.name);
            SetWallet(model.userId);
            SetPresence(model.presenceStatus);
            SetGuestUser(model.isGuest);
            SetIsBlocked(model.isBlocked);
            SetHasBlockedOwnUser(model.hasBlocked);
            SetFriendStatus(model.friendshipStatus);
        }

        public override void Dispose()
        {
            base.Dispose();

            walletCopyButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.RemoveAllListeners();
        }

        public void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics)
        {
            jumpInButton.gameObject.SetActive(true);
            jumpInButton.Initialize(friendsController, userId, socialAnalytics);
        }

        private void RemoveFriendsFocused(bool isFocused)
        {
            alreadyFriendsVariation.SetActive(!isFocused);
            unfriendVariation.SetActive(isFocused);
        }

        private void BlockFriendFocused(bool isFocused)
        {
            alreadyBlockedVariation.SetActive(!isFocused);
            unblockVariation.SetActive(isFocused);
        }

        private void SetName(string name)
        {
            this.name.text = name;
            nameInOptionsPanel.text = name;
        }

        private void SetWallet(string wallet)
        {
            fullWalletAddress = wallet;
            this.wallet.text = $"{wallet.Substring(0,5)}...{wallet.Substring(wallet.Length - 5)}";
        }

        public void SetIsBlocked(bool isBlocked)
        {
            this.isBlocked = isBlocked;
            DisableAllFriendFlowButtons();
            blockedFriendButton.gameObject.SetActive(true);
        }

        private void SetPresence(PresenceStatus status)
        {
            if(status == PresenceStatus.ONLINE)
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

        private void SetFriendStatus(FriendshipStatus friendStatus)
        {
            areFriends = friendStatus == FriendshipStatus.FRIEND;

            if(isBlocked) return;

            switch (friendStatus)
            {
                case FriendshipStatus.NOT_FRIEND:
                    DisableAllFriendFlowButtons();
                    addFriendButton.gameObject.SetActive(true);
                break;
                case FriendshipStatus.FRIEND:
                    DisableAllFriendFlowButtons();
                    alreadyFriendsButton.gameObject.SetActive(true);
                break;
                case FriendshipStatus.REQUESTED_FROM:
                    DisableAllFriendFlowButtons();
                    acceptFriendButton.gameObject.SetActive(true);
                break;
                case FriendshipStatus.REQUESTED_TO:
                    DisableAllFriendFlowButtons();
                    cancelFriendRequestButton.gameObject.SetActive(true);
                break;
                default:
                break;
            }
            whisperNonFriendsPopup.SetActive(false);
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
            if(fullWalletAddress == null)
                return;

            GUIUtility.systemCopyBuffer = fullWalletAddress;
        }

        private void WhisperActionFlow()
        {
            whisperNonFriendsPopup.SetActive(!areFriends);
            StartCoroutine(WaitAndClosePopup());
        }

        private void OpenOptions()
        {
            userContextMenu.Show(model.userId);
        }

        private IEnumerator WaitAndClosePopup()
        {
            yield return new WaitForSeconds(3);
            whisperNonFriendsPopup.SetActive(false);
        }
    }
}
