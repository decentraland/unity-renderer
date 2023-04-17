using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private const int PAGE_SIZE = 15;

        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IUserProfileBridge userProfileBridge;

        private CancellationTokenSource requestWearablesCancellationToken = new ();
        private int currentWearablePage;

        public BackpackEditorHUDController(IBackpackEditorHUDView view, DataStore dataStore,
            IWearablesCatalogService wearablesCatalogService,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.wearablesCatalogService = wearablesCatalogService;
            this.userProfileBridge = userProfileBridge;

            dataStore.HUDs.avatarEditorVisible.OnChange += SetVisibility;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;

            view.OnWearablePageChanged += HandleNewPageRequested;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
        }

        public void Dispose()
        {
            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            view.OnWearablePageChanged -= HandleNewPageRequested;
            view.Dispose();
            requestWearablesCancellationToken.SafeCancelAndDispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                view.Show();
                currentWearablePage = 0;
                requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
                RequestWearablesAndShowThem(currentWearablePage, requestWearablesCancellationToken.Token).Forget();
            }
            else
            {
                view.Hide();
                requestWearablesCancellationToken.SafeCancelAndDispose();
            }
        }

        private void SetVisibility(bool current, bool _) =>
            SetVisibility(current);

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void HandleNewPageRequested(int page)
        {
            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            RequestWearablesAndShowThem(currentWearablePage, requestWearablesCancellationToken.Token).Forget();
        }

        private async UniTaskVoid RequestWearablesAndShowThem(int page, CancellationToken cancellationToken)
        {
            UserProfile ownUserProfile = userProfileBridge.GetOwn();
            string ownUserId = ownUserProfile.userId;

            try
            {
                // TODO: instead of requesting owned wearables, we should request all the wearables with the current filters & sorting
                (IReadOnlyList<WearableItem> wearables, int totalAmount) = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    ownUserId, page,
                    PAGE_SIZE, true, cancellationToken);

                var wearableModels = wearables.Select(wearable =>
                {
                    Enum.TryParse(wearable.rarity, true, out NftRarity rarity);

                    return new WearableGridItemModel
                    {
                        WearableId = wearable.id,
                        Rarity = rarity,
                        ImageUrl = wearable.ComposeThumbnailUrl(),
                        IsEquipped = ownUserProfile.HasEquipped(wearable.id),
                        // TODO: when we should display the loading status?
                        IsLoading = false,
                        // TODO: make the new state work
                        IsNew = false,
                        IsSelected = false,
                    };
                });

                view.SetWearablePages(currentWearablePage, totalAmount / PAGE_SIZE);
                // TODO: mark the wearables to be disposed if no references left
                view.ClearWearables();
                view.ShowWearables(wearableModels);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
