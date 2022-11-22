using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using SocialFeaturesAnalytics;

namespace DCL.Social.Passports
{
    public class PassportPlayerInfoComponentView : BaseComponentView, IPassportPlayerInfoComponentView
    {
        [SerializeField] internal TextMeshProUGUI name;
        [SerializeField] internal Button walletCopyButton;
        [SerializeField] internal TextMeshProUGUI wallet;
        [SerializeField] internal Button addFriendButton;
        [SerializeField] internal GameObject onlineStatus;
        [SerializeField] internal GameObject offlineStatus;
        [SerializeField] internal GameObject normalUserPanel;
        [SerializeField] internal JumpInButton jumpInButton;

        public event Action OnAddFriend;
        public event Action OnJumpIn;

        private string fullWalletAddress;

        private void Start() 
        {
            walletCopyButton.onClick.AddListener(CopyWalletToClipboard);
            addFriendButton.onClick.AddListener(()=>OnAddFriend?.Invoke());
        }

        public void SetName(string name)
        {
            this.name.text = name;
        }

        public void SetWallet(string wallet)
        {
            fullWalletAddress = wallet;
            this.wallet.text = $"{wallet.Substring(0,5)}...{wallet.Substring(wallet.Length - 5)}";
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

        private void CopyWalletToClipboard()
        {
            if(fullWalletAddress == null)
                return;
            
            GUIUtility.systemCopyBuffer = fullWalletAddress;
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
    }
}