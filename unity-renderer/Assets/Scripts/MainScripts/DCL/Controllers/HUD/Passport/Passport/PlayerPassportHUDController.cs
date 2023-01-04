using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Threading.Tasks;
using System;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDController : IHUD
    {
        internal readonly IPlayerPassportHUDView view;
        internal readonly StringVariable currentPlayerId;
        internal readonly IUserProfileBridge userProfileBridge;
        private readonly IPassportApiBridge passportApiBridge;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly DataStore dataStore;

        internal UserProfile currentUserProfile;

        private const string URL_COLLECTIBLE_NAME = "https://market.decentraland.org/accounts/{userId}?section=ens";
        private const string URL_COLLECTIBLE_LAND = "https://market.decentraland.org/accounts/{userId}?section=land";
        private const string URL_BUY_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}?utm_source=dcl_explorer";
        private const string URL_COLLECTIBLE_GENERIC = "https://market.decentraland.org?utm_source=dcl_explorer";
        private readonly InputAction_Trigger closeWindowTrigger;

        private PassportPlayerInfoComponentController playerInfoController;
        private PassportPlayerPreviewComponentController playerPreviewController;
        private PassportNavigationComponentController passportNavigationController;

        private List<Nft> ownedNftCollectionsL1 = new List<Nft>();
        private List<Nft> ownedNftCollectionsL2 = new List<Nft>();
        private double passportOpenStartTime = 0;

        public PlayerPassportHUDController(
            IPlayerPassportHUDView view,
            PassportPlayerInfoComponentController playerInfoController,
            PassportPlayerPreviewComponentController playerPreviewController,
            PassportNavigationComponentController passportNavigationController,
            StringVariable currentPlayerId,
            IUserProfileBridge userProfileBridge,
            IPassportApiBridge passportApiBridge,
            ISocialAnalytics socialAnalytics,
            DataStore dataStore)
        {
            this.view = view;
            this.playerInfoController = playerInfoController;
            this.playerPreviewController = playerPreviewController;
            this.passportNavigationController = passportNavigationController;
            this.currentPlayerId = currentPlayerId;
            this.userProfileBridge = userProfileBridge;
            this.passportApiBridge = passportApiBridge;
            this.socialAnalytics = socialAnalytics;
            this.dataStore = dataStore;

            view.Initialize();
            view.OnClose += RemoveCurrentPlayer;

            closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
            closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
            closeWindowTrigger.OnTriggered += OnCloseButtonPressed;

            passportNavigationController.OnClickBuyNft += ClickedBuyNft;
            passportNavigationController.OnClickCollectibles += ClickedCollectibles;

            currentPlayerId.OnChange += OnCurrentPlayerIdChanged;
            OnCurrentPlayerIdChanged(currentPlayerId, null);

            playerInfoController.OnClosePassport += ClosePassport;
            dataStore.HUDs.closedWalletModal.OnChange += ClosedGuestWalletPanel;
        }

        private void ClosedGuestWalletPanel(bool current, bool previous)
        {
            if (current)
            {
                ClosePassport();
                dataStore.HUDs.closedWalletModal.Set(false, false);
            }
        }

        private void ClosePassport()
        {
            RemoveCurrentPlayer();
        }

        /// <summary>
        /// Called from <see cref="HUDBridge"/>
        /// so it just should control the root object visibility
        /// </summary>
        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        private void OnCloseButtonPressed(DCLAction_Trigger action = DCLAction_Trigger.CloseWindow)
        {
            ClosePassport();
        }

        public void Dispose()
        {
            closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
            currentPlayerId.OnChange -= OnCurrentPlayerIdChanged;
            playerInfoController.OnClosePassport -= ClosePassport;
            dataStore.HUDs.closedWalletModal.OnChange -= ClosedGuestWalletPanel;

            playerPreviewController.Dispose();

            if (view != null)
                view.Dispose();
        }

        private void OnCurrentPlayerIdChanged(string current, string previous)
        {
            if (currentUserProfile != null)
                currentUserProfile.OnUpdate -= UpdateUserProfile;

            ownedNftCollectionsL1 = new List<Nft>();
            ownedNftCollectionsL2 = new List<Nft>();
            currentUserProfile = string.IsNullOrEmpty(current)
                ? null
                : userProfileBridge.Get(current);

            if (currentUserProfile == null)
            {
                socialAnalytics.SendPassportClose(Time.realtimeSinceStartup - passportOpenStartTime);
                SetPassportPanelVisibility(false);
            }
            else
            {
                SetPassportPanelVisibility(true);
                passportOpenStartTime = Time.realtimeSinceStartup;
                socialAnalytics.SendPassportOpen();
                QueryNftCollectionsAsync(currentUserProfile.userId);
                userProfileBridge.RequestFullUserProfile(currentUserProfile.userId);
                currentUserProfile.OnUpdate += UpdateUserProfile;
                UpdateUserProfileInSubpanels(currentUserProfile);
            }
        }

        private void SetPassportPanelVisibility(bool visible)
        {
            if (visible && userProfileBridge.GetOwn().isGuest)
            {
                dataStore.HUDs.connectWalletModalVisible.Set(true);
            }
            view.SetPassportPanelVisibility(visible);
            playerPreviewController.SetPassportPanelVisibility(visible);
        }

        private async Task QueryNftCollectionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            ownedNftCollectionsL1 = await passportApiBridge.QueryNftCollectionsEthereum(userId);
            ownedNftCollectionsL2 = await passportApiBridge.QueryNftCollectionsMatic(userId);
        }

        private void ClickedBuyNft(string id, string wearableType)
        {
            if (wearableType is "name" or "parcel" or "estate")
            {
                WebInterface.OpenURL((wearableType is "name" ? URL_COLLECTIBLE_NAME : URL_COLLECTIBLE_LAND).Replace("{userId}", id));
                socialAnalytics.SendNftBuy(PlayerActionSource.Passport);
                return;
            }

            var ownedCollectible = ownedNftCollectionsL1.FirstOrDefault(nft => nft.urn == id);
            if (ownedCollectible == null)
                ownedCollectible = ownedNftCollectionsL2.FirstOrDefault(nft => nft.urn == id);

            if (ownedCollectible != null)
            {
                WebInterface.OpenURL(URL_BUY_SPECIFIC_COLLECTIBLE.Replace("{collectionId}", ownedCollectible.collectionId).Replace("{tokenId}", ownedCollectible.tokenId));
                //TODO: integrate ItemType itemType once new lambdas are active
                socialAnalytics.SendNftBuy(PlayerActionSource.Passport);
            }
            else
            {
                WebInterface.OpenURL(URL_COLLECTIBLE_GENERIC);
            }
        }

        private void ClickedCollectibles()
        {
            socialAnalytics.SendClickedOnCollectibles();
        }

        private void UpdateUserProfile(UserProfile userProfile) => UpdateUserProfileInSubpanels(userProfile);

        private void UpdateUserProfileInSubpanels(UserProfile userProfile)
        {
            playerInfoController.UpdateWithUserProfile(userProfile);
            passportNavigationController.UpdateWithUserProfile(userProfile);
            playerPreviewController.UpdateWithUserProfile(userProfile);
        }

        private void RemoveCurrentPlayer()
        {
            currentPlayerId.Set(null);
        }

    }
}
