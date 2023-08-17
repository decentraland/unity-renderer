using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Tasks;
using DCLServices.CameraReelService;
using DCLServices.WearablesCatalogService;
using System;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotVisiblePersonController
    {
        private const string SCREEN_SOURCE = "ReelPictureDetail";

        private readonly ScreenshotVisiblePersonView view;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IBrowserBridge browserBridge;
        private readonly DataStore dataStore;
        private readonly ICameraReelAnalyticsService analytics;

        private CancellationTokenSource updateProfileIconCancellationToken;
        private CancellationTokenSource fetchWearablesCancellationToken;
        private VisiblePerson person;

        public ScreenshotVisiblePersonController(ScreenshotVisiblePersonView view,
            IWearablesCatalogService wearablesCatalogService,
            IUserProfileBridge userProfileBridge,
            IBrowserBridge browserBridge,
            DataStore dataStore,
            ICameraReelAnalyticsService analytics)
        {
            this.view = view;
            this.wearablesCatalogService = wearablesCatalogService;
            this.userProfileBridge = userProfileBridge;
            this.browserBridge = browserBridge;
            this.dataStore = dataStore;
            this.analytics = analytics;

            view.OnConfigureRequested += OnConfigureRequested;
            view.OnOpenWearableMarketplaceRequested += OnOpenWearableMarketplaceRequested;
            view.OnOpenProfileRequested += OnOpenProfileRequested;
        }

        public void Dispose()
        {
            view.OnConfigureRequested -= OnConfigureRequested;
            view.OnOpenWearableMarketplaceRequested -= OnOpenWearableMarketplaceRequested;
            view.OnOpenProfileRequested -= OnOpenProfileRequested;
        }

        private void OnOpenWearableMarketplaceRequested(NFTIconComponentModel nftModel)
        {
            analytics.OpenWearableInMarketplace("Explorer");
            browserBridge.OpenUrl(nftModel.marketplaceURI);
        }

        private void OnConfigureRequested(VisiblePerson person)
        {
            this.person = person;
            view.SetProfileName(person.userName);
            view.SetProfileAddress(person.userAddress);
            updateProfileIconCancellationToken = updateProfileIconCancellationToken.SafeRestart();
            UpdateProfileIcon(person.userAddress, updateProfileIconCancellationToken.Token).Forget();
            view.SetGuestMode(person.isGuest);
            view.ClearWearables();

            if (!person.isGuest && person.wearables.Length > 0)
            {
                fetchWearablesCancellationToken = fetchWearablesCancellationToken.SafeRestart();
                FetchWearables(person, fetchWearablesCancellationToken.Token).Forget();
            }
        }

        private async UniTaskVoid FetchWearables(VisiblePerson person, CancellationToken cancellationToken)
        {
            foreach (string wearable in person.wearables)
            {
                try
                {
                    WearableItem wearableItem = await wearablesCatalogService.RequestWearableAsync(wearable, cancellationToken);

                    if (wearableItem == null) continue;

                    var nftModel = new NFTIconComponentModel
                    {
                        name = wearableItem.GetName(),
                        imageURI = wearableItem.ComposeThumbnailUrl(),
                        rarity = wearableItem.rarity,
                        nftInfo = wearableItem.GetNftInfo(),
                        marketplaceURI = wearableItem.GetMarketplaceLink(),
                        showMarketplaceButton = true,
                        showType = false,
                        type = wearableItem.data.category,
                    };

                    view.AddWearable(nftModel);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        private async UniTaskVoid UpdateProfileIcon(string userId, CancellationToken cancellationToken)
        {
            try
            {
                UserProfile profile = userProfileBridge.Get(userId)
                                      ?? await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken);

                view.SetProfilePicture(profile.face256SnapshotURL);
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }

        private void OnOpenProfileRequested()
        {
            dataStore.HUDs.currentPlayerId.Set((person.userAddress, SCREEN_SOURCE));
        }
    }
}
