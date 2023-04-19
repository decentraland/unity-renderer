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
            view.OnWearableEquipped += HandleWearableEquipped;
            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableSelected += HandleWearableSelected;
            view.OnFilterWearables += FilterWearablesFromBreadcrumb;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
        }

        public void Dispose()
        {
            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            view.OnWearablePageChanged -= HandleNewPageRequested;
            view.OnWearableEquipped -= HandleWearableEquipped;
            view.OnWearableUnequipped -= HandleWearableUnequipped;
            view.OnWearableSelected -= HandleWearableSelected;
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
                ShowWearablesAndItsFilteringPath(currentWearablePage, requestWearablesCancellationToken.Token).Forget();
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
            currentWearablePage = page;
            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            RequestWearablesAndShowThem(currentWearablePage, requestWearablesCancellationToken.Token).Forget();
        }

        private async UniTaskVoid ShowWearablesAndItsFilteringPath(int page, CancellationToken cancellationToken)
        {
            var wearableBreadcrumbModel = new NftBreadcrumbModel
            {
                Path = new[]
                {
                    (Type: "all://", Reference: "all://", Name: "All"),

                    // (Type: "category://shoes", Reference: "category://shoes", Name: "Shoes"),
                    // (Type: "name://", Reference: "name://my wearable", Name: "my wearable"),
                },
                Current = 0,
                ResultCount = 0,
            };

            view.SetWearableBreadcrumb(wearableBreadcrumbModel);

            int resultCount = await RequestWearablesAndShowThem(page, cancellationToken);

            view.SetWearableBreadcrumb(wearableBreadcrumbModel with { ResultCount = resultCount });
        }

        private async UniTask<int> RequestWearablesAndShowThem(int page, CancellationToken cancellationToken)
        {
            UserProfile ownUserProfile = userProfileBridge.GetOwn();
            string ownUserId = ownUserProfile.userId;

            try
            {
                // TODO: instead of requesting owned wearables, we should request all the wearables with the current filters & sorting
                (IReadOnlyList<WearableItem> wearables, int totalAmount) = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    ownUserId,

                    // page is not zero based
                    page + 1,
                    PAGE_SIZE, true, cancellationToken);

                var wearableModels = wearables.Select(wearable =>
                {
                    if (!Enum.TryParse(wearable.rarity, true, out NftRarity rarity))
                    {
                        rarity = NftRarity.Uncommon;
                        Debug.LogError($"Could not parse the rarity of the wearable: {wearable.rarity}. Fallback to uncommon..");
                    }

                    return new WearableGridItemModel
                    {
                        WearableId = wearable.id,
                        Rarity = rarity,
                        ImageUrl = wearable.ComposeThumbnailUrl(),
                        IsEquipped = ownUserProfile.HasEquipped(wearable.id),

                        // TODO: make the new state work
                        IsNew = false,
                        IsSelected = false,
                    };
                });

                view.SetWearablePages(page, (totalAmount / PAGE_SIZE) + 1);

                // TODO: mark the wearables to be disposed if no references left
                view.ClearWearables();
                view.ShowWearables(wearableModels);

                return totalAmount;
            }
            catch (Exception e) { Debug.LogException(e); }

            return 0;
        }

        private void HandleWearableSelected(WearableGridItemModel wearableGridItem)
        {
            view.ClearWearableSelection();
            view.SelectWearable(wearableGridItem.WearableId);
        }

        private void HandleWearableUnequipped(WearableGridItemModel wearableGridItem)
        {
            throw new NotImplementedException();
        }

        private void HandleWearableEquipped(WearableGridItemModel wearableGridItem)
        {
            throw new NotImplementedException();
        }

        private void FilterWearablesFromBreadcrumb(string referencePath)
        {
            if (referencePath.StartsWith("all://"))
            {
                currentWearablePage = 0;
                requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
                ShowWearablesAndItsFilteringPath(currentWearablePage, requestWearablesCancellationToken.Token).Forget();
            }
            else if (referencePath.StartsWith("name://")) { throw new NotImplementedException(); }
            else if (referencePath.StartsWith("category://")) { throw new NotImplementedException(); }
        }
    }
}
