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
    public class WearableGridController
    {
        private const int PAGE_SIZE = 15;
        private const string ALL_FILTER_REF = "all";
        private const string NAME_FILTER_REF = "name=";
        private const string CATEGORY_FILTER_REF = "category=";

        private readonly IWearableGridView view;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IWearablesCatalogService wearablesCatalogService;

        private CancellationTokenSource requestWearablesCancellationToken = new ();

        public WearableGridController(IWearableGridView view,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;

            view.OnWearablePageChanged += HandleNewPageRequested;
            view.OnWearableEquipped += HandleWearableEquipped;
            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableSelected += HandleWearableSelected;
            view.OnFilterWearables += FilterWearablesFromBreadcrumb;
        }

        public void Dispose()
        {
            view.OnWearablePageChanged -= HandleNewPageRequested;
            view.OnWearableEquipped -= HandleWearableEquipped;
            view.OnWearableUnequipped -= HandleWearableUnequipped;
            view.OnWearableSelected -= HandleWearableSelected;
            view.OnFilterWearables -= FilterWearablesFromBreadcrumb;

            view.Dispose();
            requestWearablesCancellationToken.SafeCancelAndDispose();
        }

        public void LoadWearables()
        {
            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            ShowWearablesAndItsFilteringPath(1, requestWearablesCancellationToken.Token).Forget();
        }

        public void CancelWearableLoading() =>
            requestWearablesCancellationToken.SafeCancelAndDispose();

        private async UniTaskVoid ShowWearablesAndItsFilteringPath(int page, CancellationToken cancellationToken)
        {
            var wearableBreadcrumbModel = new NftBreadcrumbModel
            {
                Path = new[]
                {
                    (Reference: ALL_FILTER_REF, Name: "All"),
                    // (Reference: $"{CATEGORY_FILTER_REF}shoes", Name: "Shoes"),
                    // (Reference: $"{NAME_FILTER_REF}my wearable", Name: "my wearable"),
                },
                Current = 0,
                ResultCount = 0,
            };

            view.SetWearableBreadcrumb(wearableBreadcrumbModel);

            int resultCount = await RequestWearablesAndShowThem(page, cancellationToken);

            view.SetWearableBreadcrumb(wearableBreadcrumbModel with { ResultCount = resultCount });
        }

        private void HandleNewPageRequested(int page)
        {
            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            RequestWearablesAndShowThem(page, requestWearablesCancellationToken.Token).Forget();
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
                    page,
                    PAGE_SIZE, true, cancellationToken);

                var wearableModels = wearables.Select(ToWearableGridModel);

                view.SetWearablePages(page, (totalAmount / PAGE_SIZE) + 1);

                // TODO: mark the wearables to be disposed if no references left
                view.ClearWearables();
                view.ShowWearables(wearableModels);

                return totalAmount;
            }
            catch (Exception e) { Debug.LogException(e); }

            return 0;
        }

        private WearableGridItemModel ToWearableGridModel(WearableItem wearable)
        {
            if (!Enum.TryParse(wearable.rarity, true, out NftRarity rarity))
            {
                rarity = NftRarity.Common;
                Debug.LogError($"Could not parse the rarity of the wearable: {wearable.rarity}. Fallback to common..");
            }

            return new WearableGridItemModel
            {
                WearableId = wearable.id,
                Rarity = rarity,
                ImageUrl = wearable.ComposeThumbnailUrl(),
                IsEquipped = userProfileBridge.GetOwn().HasEquipped(wearable.id),
                // TODO: make the new state work
                IsNew = false,
                IsSelected = false,
            };
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
            if (referencePath.StartsWith(ALL_FILTER_REF))
            {
                requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
                ShowWearablesAndItsFilteringPath(1, requestWearablesCancellationToken.Token).Forget();
            }
            else if (referencePath.StartsWith(NAME_FILTER_REF)) { throw new NotImplementedException(); }
            else if (referencePath.StartsWith(CATEGORY_FILTER_REF)) { throw new NotImplementedException(); }
        }
    }
}
