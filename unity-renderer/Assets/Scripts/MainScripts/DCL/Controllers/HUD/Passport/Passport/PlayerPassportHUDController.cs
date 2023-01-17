using Cysharp.Threading.Tasks;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDController : IHUD
    {
        private const string URL_COLLECTIBLE_NAME = "https://market.decentraland.org/accounts/{userId}?section=ens";
        private const string URL_COLLECTIBLE_LAND = "https://market.decentraland.org/accounts/{userId}?section=land";
        private const string URL_BUY_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}?utm_source=dcl_explorer";
        private const string URL_COLLECTIBLE_GENERIC = "https://market.decentraland.org?utm_source=dcl_explorer";
        private static readonly string[] ALLOWED_TYPES = { "name", "parcel", "estate" };

        private readonly IPlayerPassportHUDView view;
        private readonly StringVariable currentPlayerId;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IPassportApiBridge passportApiBridge;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly DataStore dataStore;
        private readonly InputAction_Trigger closeWindowTrigger;
        private readonly PassportPlayerInfoComponentController playerInfoController;
        private readonly PassportPlayerPreviewComponentController playerPreviewController;
        private readonly PassportNavigationComponentController passportNavigationController;
        private readonly BooleanVariable playerInfoCardVisibleState;

        private UserProfile currentUserProfile;
        private List<Nft> ownedNftCollectionsL1 = new ();
        private List<Nft> ownedNftCollectionsL2 = new ();
        private double passportOpenStartTime;

        public PlayerPassportHUDController(
            IPlayerPassportHUDView view,
            PassportPlayerInfoComponentController playerInfoController,
            PassportPlayerPreviewComponentController playerPreviewController,
            PassportNavigationComponentController passportNavigationController,
            StringVariable currentPlayerId,
            IUserProfileBridge userProfileBridge,
            IPassportApiBridge passportApiBridge,
            ISocialAnalytics socialAnalytics,
            DataStore dataStore,
            MouseCatcher mouseCatcher,
            BooleanVariable playerInfoCardVisibleState)
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
            this.playerInfoCardVisibleState = playerInfoCardVisibleState;

            view.Initialize(mouseCatcher);
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
            dataStore.HUDs.currentPassportSortingOrder.Set(view.PassportCurrentSortingOrder);
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

            playerInfoController.Dispose();
            playerPreviewController.Dispose();
            passportNavigationController.Dispose();

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
                QueryNftCollectionsAsync(currentUserProfile.userId).Forget();
                userProfileBridge.RequestFullUserProfile(currentUserProfile.userId);
                currentUserProfile.OnUpdate += UpdateUserProfile;
                UpdateUserProfile(currentUserProfile, true);
            }
        }

        private void SetPassportPanelVisibility(bool visible)
        {
            if (visible && userProfileBridge.GetOwn().isGuest)
            {
                dataStore.HUDs.connectWalletModalVisible.Set(true);
            }
            playerInfoCardVisibleState.Set(visible);
            view.SetPassportPanelVisibility(visible);
            playerPreviewController.SetPassportPanelVisibility(visible);
        }

        private async UniTask QueryNftCollectionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            //TODO: Not integrating CT yet due to changes incoming from issue #4040
            (ownedNftCollectionsL1, ownedNftCollectionsL2) = await UniTask.WhenAll(passportApiBridge.QueryNftCollectionsEthereum(userId), passportApiBridge.QueryNftCollectionsMatic(userId));
        }

        private void ClickedBuyNft(string id, string wearableType)
        {
            if (ALLOWED_TYPES.Contains(wearableType))
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

        private void UpdateUserProfile(UserProfile userProfile)
        {
            UpdateUserProfile(userProfile, false);
        }

        private void UpdateUserProfile(UserProfile userProfile, bool activateLoading)
        {
            playerPreviewController.UpdateWithUserProfile(userProfile, activateLoading);
            playerInfoController.UpdateWithUserProfile(userProfile);
            passportNavigationController.UpdateWithUserProfile(userProfile);
        }

        private void RemoveCurrentPlayer()
        {
            passportNavigationController.CloseAllNFTItemInfos();
            currentPlayerId.Set(null);
        }

    }
}
