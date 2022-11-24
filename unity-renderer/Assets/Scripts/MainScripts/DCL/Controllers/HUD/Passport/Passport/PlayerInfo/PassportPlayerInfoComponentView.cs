using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SocialFeaturesAnalytics;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentView : BaseComponentView, IPassportPlayerInfoComponentView
    {
        [SerializeField] internal TextMeshProUGUI name;
        [SerializeField] internal TextMeshProUGUI nameInOptionsPanel;
        [SerializeField] internal Button walletCopyButton;
        [SerializeField] internal TextMeshProUGUI wallet;
        [SerializeField] internal ButtonComponentView optionsButton;
        [SerializeField] internal ButtonComponentView addFriendButton;
        [SerializeField] internal ButtonComponentView alreadyFriendsButton;
        [SerializeField] internal ButtonComponentView cancelFriendRequestButton;
        [SerializeField] internal ButtonComponentView acceptFriendButton;
        [SerializeField] internal ButtonComponentView blockedFriendButton;
        [SerializeField] internal Button blockButton;
        [SerializeField] internal Button reportButton;
        [SerializeField] internal Button unfriendButton;
        [SerializeField] internal Button whisperButton;
        [SerializeField] internal GameObject whisperNonFriendsPopup;
        [SerializeField] internal GameObject onlineStatus;
        [SerializeField] internal GameObject offlineStatus;
        [SerializeField] internal GameObject normalUserPanel;
        [SerializeField] internal GameObject optionsPanel;
        [SerializeField] internal GameObject friendsFlowContainer;
        
        [SerializeField] internal GameObject alreadyFriendsVariation;
        [SerializeField] internal GameObject unfriendVariation;

        [SerializeField] internal GameObject alreadyBlockedVariation;
        [SerializeField] internal GameObject unblockVariation;
        
        [SerializeField] internal JumpInButton jumpInButton;
        
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

        private void Start() 
        {
            walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
            addFriendButton.onClick.AddListener(()=>OnAddFriend?.Invoke());
            alreadyFriendsButton.onClick.AddListener(()=>OnRemoveFriend?.Invoke());
            cancelFriendRequestButton.onClick.AddListener(()=>OnCancelFriendRequest?.Invoke());
            acceptFriendButton.onClick.AddListener(()=>OnAcceptFriendRequest?.Invoke());
            blockButton.onClick.AddListener(()=>OnBlockUser?.Invoke());
            blockedFriendButton.onClick.AddListener(()=>OnUnblockUser?.Invoke());
            reportButton.onClick.AddListener(()=>OnReportUser?.Invoke());
            unfriendButton.onClick.AddListener(()=>OnRemoveFriend?.Invoke());
            whisperButton.onClick.AddListener(WhisperActionFlow);
            optionsButton.onClick.AddListener(OpenOptions);

            alreadyFriendsButton.onFocused += RemoveFriendsFocused;
            blockedFriendButton.onFocused += BlockFriendFocused;
            optionsButton.onFocused += OptionsPanelFocused;
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
        
        private void OptionsPanelFocused(bool isFocused)
        {
            if(!isFocused)
                optionsPanel.SetActive(false);
        }

        public void SetName(string name)
        {
            this.name.text = name;
            nameInOptionsPanel.text = name;
        }

        public void SetWallet(string wallet)
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

        public void SetPresence(PresenceStatus status)
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

        public void SetGuestUser(bool isGuest)
        {
            normalUserPanel.SetActive(!isGuest);
        }

        public void SetHasBlockedOwnUser(bool hasBlocked)
        {
            friendsFlowContainer.SetActive(!hasBlocked);
        }

        public void SetFriendStatus(FriendshipStatus friendStatus)
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
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }

        public override void RefreshControl() 
        {
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

        private IEnumerator WaitAndClosePopup()
        {
            yield return new WaitForSeconds(3);
            whisperNonFriendsPopup.SetActive(false);
        }
    }
}