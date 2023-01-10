using DCL.Helpers;
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using SocialFeaturesAnalytics;
using DCL.Social.Friends;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentView : BaseComponentView<PlayerPassportModel>, IPassportPlayerInfoComponentView
    {
        private const float COPY_TOAST_VISIBLE_TIME = 3;

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
        [SerializeField] private Button whisperButton;
        [SerializeField] private TooltipComponentView whisperNonFriendsPopup;
        [SerializeField] private GameObject onlineStatus;
        [SerializeField] private GameObject offlineStatus;
        [SerializeField] private GameObject normalUserPanel;
        [SerializeField] private GameObject friendsFlowContainer;
        [SerializeField] private UserContextMenu userContextMenu;

        [SerializeField] private GameObject alreadyFriendsVariation;
        [SerializeField] private GameObject unfriendVariation;

        [SerializeField] private JumpInButton jumpInButton;
        [SerializeField] private ShowHideAnimator copyAddressToast;
        [SerializeField] private ShowHideAnimator copyUsernameToast;

        public event Action OnAddFriend;
        public event Action OnRemoveFriend;
        public event Action OnCancelFriendRequest;
        public event Action OnAcceptFriendRequest;
        public event Action OnBlockUser;
        public event Action OnUnblockUser;
        public event Action OnReportUser;
        public event Action<string> OnWhisperUser;
        public event Action OnJumpInUser;
        public event Action OnWalletCopy;

        private string fullWalletAddress;
        private bool areFriends;
        private bool isBlocked = false;
        private Coroutine copyAddressRoutine = null;
        private Coroutine copyNameRoutine = null;
        private Dictionary<FriendshipStatus, GameObject> friendStatusButtonsMapping;
        
        public override void Start()
        {
            walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
            usernameCopyButton.onClick.AddListener(CopyUsernameToClipboard);
            addFriendButton.onClick.AddListener(()=>OnAddFriend?.Invoke());
            alreadyFriendsButton.onClick.AddListener(()=>OnRemoveFriend?.Invoke());
            cancelFriendRequestButton.onClick.AddListener(()=>OnCancelFriendRequest?.Invoke());
            acceptFriendButton.onClick.AddListener(()=>OnAcceptFriendRequest?.Invoke());
            userContextMenu.OnBlock += OnBlock;
            blockedFriendButton.onClick.AddListener(()=>OnUnblockUser?.Invoke());
            userContextMenu.OnReport += OnReport;
            whisperButton.onClick.AddListener(WhisperActionFlow);
            optionsButton.onClick.AddListener(OpenOptions);
            jumpInButton.OnClick += () => OnJumpInUser?.Invoke();
            alreadyFriendsButton.onFocused += RemoveFriendsFocused;

            friendStatusButtonsMapping = new Dictionary<FriendshipStatus, GameObject>()
            {
                {FriendshipStatus.NOT_FRIEND, addFriendButton.gameObject},
                {FriendshipStatus.FRIEND, alreadyFriendsButton.gameObject},
                {FriendshipStatus.REQUESTED_FROM, acceptFriendButton.gameObject},
                {FriendshipStatus.REQUESTED_TO, cancelFriendRequestButton.gameObject}
            };
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

            SetGuestUser(model.isGuest);
            SetName(model.name);
            userContextMenu.Hide();
            SetWallet(model.userId);
            SetPresence(model.presenceStatus);
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

        public void ResetPanelOnClose()
        {
            copyAddressToast.Hide(true);
            copyUsernameToast.Hide(true);
        }

        public void InitializeJumpInButton(IFriendsController friendsController, string userId, ISocialAnalytics socialAnalytics)
        {
            if (friendsController.IsFriend(userId))
            {
                jumpInButton.gameObject.SetActive(true);
                jumpInButton.Initialize(friendsController, userId, socialAnalytics);
            }
            else
            {
                jumpInButton.gameObject.SetActive(false);
            }
        }

        public void SetFriendStatus(FriendshipStatus friendStatus)
        {
            areFriends = friendStatus == FriendshipStatus.FRIEND;

            if(isBlocked)
                return;

            DisableAllFriendFlowButtons();
            friendStatusButtonsMapping[friendStatus].SetActive(true);
            whisperNonFriendsPopup.Hide(true);
        }

        private void RemoveFriendsFocused(bool isFocused)
        {
            alreadyFriendsVariation.SetActive(!isFocused);
            unfriendVariation.SetActive(isFocused);
        }

        private void SetName(string name)
        {
            if (name.Contains('#'))
            {
                this.name.SetText(name.Split('#')[0]);
                address.SetText($"#{name.Split('#')[1]}");
            }
            else
            {
                this.name.SetText(name);
                address.SetText("");
            }

            Utils.ForceRebuildLayoutImmediate(usernameRect);

            nameInOptionsPanel.text = name;
        }

        private void SetWallet(string wallet)
        {
            if (string.IsNullOrEmpty(wallet))
                return;

            fullWalletAddress = wallet;
            this.wallet.text = $"{wallet.Substring(0,5)}...{wallet.Substring(wallet.Length - 5)}";
        }

        public void SetIsBlocked(bool isBlocked)
        {
            this.isBlocked = isBlocked;
            DisableAllFriendFlowButtons();
            blockedFriendButton.gameObject.SetActive(isBlocked);

            if (!isBlocked)
            {
                SetFriendStatus(model.friendshipStatus);
            }
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

            OnWalletCopy?.Invoke();
            Environment.i.platform.clipboard.WriteText(fullWalletAddress);
            if (copyAddressRoutine != null)
            {
                StopCoroutine(copyAddressRoutine);
            }

            copyAddressRoutine = StartCoroutine(ShowCopyToast(copyAddressToast));
        }

        private void CopyUsernameToClipboard()
        {
            if(string.IsNullOrEmpty(model.name))
                return;

            Environment.i.platform.clipboard.WriteText(model.name);
            if (copyNameRoutine != null)
            {
                StopCoroutine(copyNameRoutine);
            }

            copyNameRoutine = StartCoroutine(ShowCopyToast(copyUsernameToast));
        }

        private IEnumerator ShowCopyToast(ShowHideAnimator toast)
        {
            if (!toast.gameObject.activeSelf)
            {
                toast.gameObject.SetActive(true);
            }

            toast.Show();
            yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
            toast.Hide();
        }

        private void WhisperActionFlow()
        {
            if (areFriends)
            {
                OnWhisperUser?.Invoke(model.userId);
            }
            else
            {
                if (areFriends)
                {
                    whisperNonFriendsPopup.Hide();
                }
                else
                {
                    whisperNonFriendsPopup.Show();
                }
            }
        }

        private void OpenOptions()
        {
            userContextMenu.Show(model.userId);
        }
    }
}
