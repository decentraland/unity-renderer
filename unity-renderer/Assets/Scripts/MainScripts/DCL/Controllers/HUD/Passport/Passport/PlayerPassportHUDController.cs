using Cysharp.Threading.Tasks;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDController : IHUD
    {
        private const string URL_COLLECTIBLE_NAME = "https://market.decentraland.org/accounts/{userId}?section=ens";
        private const string URL_COLLECTIBLE_LAND = "https://market.decentraland.org/accounts/{userId}?section=land";
        private const string URL_BUY_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}?utm_source=dcl_explorer";
        private const string URL_COLLECTIBLE_GENERIC = "https://market.decentraland.org?utm_source=dcl_explorer";
        private const string NAME_TYPE = "name";
        private static readonly string[] ALLOWED_TYPES = { NAME_TYPE, "parcel", "estate" };

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
        private CancellationTokenSource cts = new CancellationTokenSource();

        private bool isOpen;

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
            view.OnClose += ClosePassport;

            closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
            closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
            closeWindowTrigger.OnTriggered += OnCloseButtonPressed;

            passportNavigationController.OnClickBuyNft += ClickedBuyNft;
            passportNavigationController.OnClickedLink += ClickedLink;
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
            if(!isOpen)
                return;

            isOpen = false;

            if (userProfileBridge.GetOwn().isGuest)
                dataStore.HUDs.connectWalletModalVisible.Set(false);

            passportNavigationController.CloseAllNFTItemInfos();
            passportNavigationController.SetViewInitialPage();
            playerInfoController.ClosePassport();
            currentPlayerId.Set(null);
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
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            closeWindowTrigger.OnTriggered -= OnCloseButtonPressed;
            currentPlayerId.OnChange -= OnCurrentPlayerIdChanged;
            playerInfoController.OnClosePassport -= ClosePassport;
            dataStore.HUDs.closedWalletModal.OnChange -= ClosedGuestWalletPanel;

            playerInfoController.Dispose();
            playerPreviewController.Dispose();
            passportNavigationController.Dispose();

            view?.Dispose();
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
            isOpen = visible;

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

            (ownedNftCollectionsL1, ownedNftCollectionsL2) = await UniTask.WhenAll(passportApiBridge.QueryNftCollectionsAsync(userId, NftCollectionsLayer.ETHEREUM, cts.Token), passportApiBridge.QueryNftCollectionsAsync(userId, NftCollectionsLayer.MATIC, cts.Token));
        }

        private void ClickedLink()
        {
            socialAnalytics.SendLinkClick(PlayerActionSource.Passport);
        }

        private void ClickedBuyNft(string id, string wearableType)
        {
            async UniTaskVoid QueryNftCollectionByUrnAsync(string urn)
            {
                var nft = await passportApiBridge.QueryNftCollectionAsync(currentUserProfile.userId, urn, NftCollectionsLayer.MATIC, cts.Token);

                if (nft == null)
                    nft = await passportApiBridge.QueryNftCollectionAsync(currentUserProfile.userId, urn, NftCollectionsLayer.ETHEREUM, cts.Token);

                if (nft != null)
                    OpenNftMarketUrl(nft);
                else
                    passportApiBridge.OpenURL(URL_COLLECTIBLE_GENERIC);
            }

            if (ALLOWED_TYPES.Contains(wearableType))
            {
                passportApiBridge.OpenURL((wearableType is NAME_TYPE ? URL_COLLECTIBLE_NAME : URL_COLLECTIBLE_LAND).Replace("{userId}", id));
                socialAnalytics.SendNftBuy(PlayerActionSource.Passport);
                return;
            }

            var ownedCollectible = ownedNftCollectionsL1.FirstOrDefault(nft => nft.urn == id);
            if (ownedCollectible == null)
                ownedCollectible = ownedNftCollectionsL2.FirstOrDefault(nft => nft.urn == id);

            if (ownedCollectible != null)
                OpenNftMarketUrl(ownedCollectible);
            else
            {
                cts?.Cancel();
                cts?.Dispose();
                cts = new CancellationTokenSource();

                // In case the NFT's information is not found neither on ownedNftCollectionsL1 nor or ownedNftCollectionsL2 (it could happen due
                // to the TheGraph queries only return a maximum of 100 entries by default), we request the information of this specific NFT.
                QueryNftCollectionByUrnAsync(id).Forget();
            }
        }

        private void OpenNftMarketUrl(Nft nft)
        {
            passportApiBridge.OpenURL(URL_BUY_SPECIFIC_COLLECTIBLE.Replace("{collectionId}", nft.collectionId).Replace("{tokenId}", nft.tokenId));
            //TODO: integrate ItemType itemType once new lambdas are active
            socialAnalytics.SendNftBuy(PlayerActionSource.Passport);
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

    }
}
