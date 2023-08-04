using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotVisiblePersonController
    {
        private readonly ScreenshotVisiblePersonView view;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IBrowserBridge browserBridge;

        private CancellationTokenSource updateProfileIconCancellationToken;
        private CancellationTokenSource fetchWearablesCancellationToken;

        public ScreenshotVisiblePersonController(ScreenshotVisiblePersonView view,
            IWearablesCatalogService wearablesCatalogService,
            IUserProfileBridge userProfileBridge,
            IBrowserBridge browserBridge)
        {
            this.view = view;
            this.wearablesCatalogService = wearablesCatalogService;
            this.userProfileBridge = userProfileBridge;
            this.browserBridge = browserBridge;

            view.OnConfigureRequested += OnConfigureRequested;
            view.OnOpenWearableMarketplaceRequested += OnOpenWearableMarketplaceRequested;
        }

        public void Dispose()
        {
            view.OnConfigureRequested -= OnConfigureRequested;
            view.OnOpenWearableMarketplaceRequested -= OnOpenWearableMarketplaceRequested;
        }

        private void OnOpenWearableMarketplaceRequested(NFTIconComponentModel nftModel) =>
            browserBridge.OpenUrl(nftModel.marketplaceURI);

        private void OnConfigureRequested(VisiblePerson person)
        {
            view.SetProfileName(person.userName);
            view.SetProfileAddress(person.userAddress);
            updateProfileIconCancellationToken = updateProfileIconCancellationToken.SafeRestart();
            UpdateProfileIcon(person.userAddress, updateProfileIconCancellationToken.Token).Forget();
            view.SetGuestMode(person.isGuest);

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
                WearableItem wearableItem = await wearablesCatalogService.RequestWearableAsync(wearable, cancellationToken);

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
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
