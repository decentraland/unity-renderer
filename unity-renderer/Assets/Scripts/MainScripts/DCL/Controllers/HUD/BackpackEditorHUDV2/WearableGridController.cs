using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
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
        private const string URL_MARKET_PLACE = "https://market.decentraland.org/browse?section=wearables";
        private const string URL_GET_A_WALLET = "https://docs.decentraland.org/get-a-wallet";

        private readonly IWearableGridView view;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly DataStore_BackpackV2 dataStoreBackpackV2;
        private readonly IBrowserBridge browserBridge;
        private readonly BackpackFiltersController backpackFiltersController;
        private readonly AvatarSlotsHUDController avatarSlotsHUDController;
        private readonly IBackpackAnalyticsController backpackAnalyticsController;

        private Dictionary<string, WearableGridItemModel> currentWearables = new ();
        private CancellationTokenSource requestWearablesCancellationToken = new ();
        private CancellationTokenSource filtersCancellationToken = new ();
        private string categoryFilter;
        private ICollection<string> thirdPartyCollectionIdsFilter;
        private string nameFilter;
        private (NftOrderByOperation type, bool directionAscendent)? wearableSorting;
        private NftCollectionType collectionTypeMask = NftCollectionType.Base | NftCollectionType.OnChain;

        public event Action<string> OnWearableSelected;
        public event Action<string, EquipWearableSource> OnWearableEquipped;
        public event Action<string, UnequipWearableSource> OnWearableUnequipped;
        public event Action<string, bool> OnHideUnhidePressed;

        public WearableGridController(IWearableGridView view,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            DataStore_BackpackV2 dataStoreBackpackV2,
            IBrowserBridge browserBridge,
            BackpackFiltersController backpackFiltersController,
            AvatarSlotsHUDController avatarSlotsHUDController,
            IBackpackAnalyticsController backpackAnalyticsController)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.dataStoreBackpackV2 = dataStoreBackpackV2;
            this.browserBridge = browserBridge;
            this.backpackFiltersController = backpackFiltersController;
            this.avatarSlotsHUDController = avatarSlotsHUDController;
            this.backpackAnalyticsController = backpackAnalyticsController;

            view.OnWearablePageChanged += HandleNewPageRequested;
            view.OnWearableEquipped += HandleWearableEquipped;
            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableSelected += HandleWearableSelected;
            view.OnFilterSelected += FilterWearablesFromReferencePath;
            view.OnFilterRemoved += RemoveFiltersFromReferencePath;
            view.OnGoToMarketplace += GoToMarketplace;

            backpackFiltersController.OnThirdPartyCollectionChanged += SetThirdPartCollectionIds;
            backpackFiltersController.OnSortByChanged += SetSorting;
            backpackFiltersController.OnSearchTextChanged += SetTextFilter;
            backpackFiltersController.OnCollectionTypeChanged += SetCollectionTypeFromFilterSelection;

            avatarSlotsHUDController.OnToggleSlot += SetCategoryFromFilterSelection;
        }

        public void Dispose()
        {
            view.OnWearablePageChanged -= HandleNewPageRequested;
            view.OnWearableEquipped -= HandleWearableEquipped;
            view.OnWearableUnequipped -= HandleWearableUnequipped;
            view.OnWearableSelected -= HandleWearableSelected;
            view.OnFilterRemoved -= RemoveFiltersFromReferencePath;
            view.OnFilterSelected -= FilterWearablesFromReferencePath;
            view.OnGoToMarketplace -= GoToMarketplace;

            backpackFiltersController.OnThirdPartyCollectionChanged -= SetThirdPartCollectionIds;
            backpackFiltersController.OnSortByChanged -= SetSorting;
            backpackFiltersController.OnSearchTextChanged -= SetTextFilter;
            backpackFiltersController.OnCollectionTypeChanged -= SetCollectionTypeFromFilterSelection;
            backpackFiltersController.Dispose();

            avatarSlotsHUDController.OnToggleSlot -= SetCategoryFromFilterSelection;
            avatarSlotsHUDController.Dispose();

            view.Dispose();
            requestWearablesCancellationToken.SafeCancelAndDispose();
            filtersCancellationToken.SafeCancelAndDispose();
        }

        public void LoadWearables()
        {
            LoadWearablesWithFilters(categoryFilter, collectionTypeMask, thirdPartyCollectionIdsFilter,
                nameFilter, wearableSorting);
        }

        public void LoadWearablesWithFilters(string categoryFilter = null,
            NftCollectionType collectionTypeMask = NftCollectionType.All,
            ICollection<string> thirdPartyCollectionIdsFilter = null, string nameFilter = null,
            (NftOrderByOperation type, bool directionAscendent)? wearableSorting = null)
        {
            this.categoryFilter = categoryFilter;
            this.collectionTypeMask = collectionTypeMask;
            this.thirdPartyCollectionIdsFilter = thirdPartyCollectionIdsFilter;
            this.nameFilter = nameFilter;
            this.wearableSorting = wearableSorting;
            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            ShowWearablesAndUpdateFilters(1, requestWearablesCancellationToken.Token).Forget();
        }

        public void CancelWearableLoading() =>
            requestWearablesCancellationToken.SafeCancelAndDispose();

        public void Equip(string wearableId)
        {
            if (!currentWearables.TryGetValue(wearableId, out WearableGridItemModel wearableGridModel))
                return;

            wearableGridModel.IsEquipped = true;
            view.SetWearable(wearableGridModel);
        }

        public void UnEquip(string wearableId)
        {
            if (!currentWearables.TryGetValue(wearableId, out WearableGridItemModel wearableGridModel))
                return;

            wearableGridModel.IsEquipped = false;
            view.SetWearable(wearableGridModel);
        }

        public void UpdateBodyShapeCompatibility(string bodyShapeId)
        {
            foreach ((string wearableId, WearableGridItemModel model) in currentWearables)
            {
                if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable)) continue;
                bool isCompatibleWithBodyShape = IsCompatibleWithBodyShape(bodyShapeId, wearable);
                model.IsCompatibleWithBodyShape = isCompatibleWithBodyShape;
                view.SetWearable(model);
            }
        }

        public void LoadCollections() =>
            backpackFiltersController.LoadCollections();

        private async UniTaskVoid ShowWearablesAndUpdateFilters(int page, CancellationToken cancellationToken)
        {
            List<(string reference, string name, string type, bool removable)> path = new ();

            var additiveReferencePath = $"{ALL_FILTER_REF}";
            path.Add((reference: additiveReferencePath, name: "All", type: "all", removable: false));

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                additiveReferencePath += $"&{CATEGORY_FILTER_REF}{categoryFilter}";

                // TODO: translate category id into names (??)
                path.Add((reference: additiveReferencePath, name: categoryFilter, type: categoryFilter, removable: true));
            }

            if (!string.IsNullOrEmpty(nameFilter))
            {
                additiveReferencePath += $"&{NAME_FILTER_REF}{nameFilter}";
                path.Add((reference: additiveReferencePath, name: nameFilter, type: "nft-name", removable: true));
            }

            var wearableBreadcrumbModel = new NftBreadcrumbModel
            {
                Path = path.ToArray(),
                Current = path.Count - 1,
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
            AudioScriptableObjects.listItemAppear.ResetPitch();
            UserProfile ownUserProfile = userProfileBridge.GetOwn();
            string ownUserId = ownUserProfile.userId;

            try
            {
                currentWearables.Clear();

                (IReadOnlyList<WearableItem> wearables, int totalAmount) = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    ownUserId,
                    page,
                    PAGE_SIZE, cancellationToken,
                    categoryFilter, NftRarity.None, collectionTypeMask,
                    thirdPartyCollectionIdsFilter,
                    nameFilter, wearableSorting);

                currentWearables = wearables.Select(ToWearableGridModel)
                                            .ToDictionary(item => item.WearableId, model => model);

                view.SetWearablePages(page, (totalAmount + PAGE_SIZE - 1) / PAGE_SIZE);

                // TODO: mark the wearables to be disposed if no references left
                view.ClearWearables();
                view.ShowWearables(currentWearables.Values);

                return totalAmount;
            }
            catch (Exception e) { Debug.LogException(e); }

            return 0;
        }

        private WearableGridItemModel ToWearableGridModel(WearableItem wearable)
        {
            if (!Enum.TryParse(wearable.rarity, true, out NftRarity rarity))
            {
                rarity = NftRarity.None;
                Debug.LogWarning($"Could not parse the rarity of the wearable '{wearable.id}'. Fallback to common.");
            }

            string currentBodyShapeId = dataStoreBackpackV2.previewBodyShape.Get();

            return new WearableGridItemModel
            {
                WearableId = wearable.id,
                Rarity = rarity,
                Category = wearable.data.category,
                ImageUrl = wearable.ComposeThumbnailUrl(),
                IsEquipped = IsEquipped(wearable.id),
                IsNew = (DateTime.UtcNow - wearable.MostRecentTransferredDate).TotalHours < 24,
                IsSelected = false,
                UnEquipAllowed = CanWearableBeUnEquipped(wearable),
                IsCompatibleWithBodyShape = IsCompatibleWithBodyShape(currentBodyShapeId, wearable),
            };
        }

        private bool IsEquipped(string wearableId) =>
            dataStoreBackpackV2.previewEquippedWearables.Contains(wearableId)
            || wearableId == dataStoreBackpackV2.previewBodyShape.Get();

        private void HandleWearableSelected(WearableGridItemModel wearableGridItem)
        {
            string wearableId = wearableGridItem.WearableId;

            view.ClearWearableSelection();
            view.SelectWearable(wearableId);

            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot fill the wearable info card, the wearable id does not exist {wearableId}");
                return;
            }

            string[] hidesList = wearable.GetHidesList(userProfileBridge.GetOwn().avatar.bodyShape);

            view.FillInfoCard(new InfoCardComponentModel
            {
                rarity = wearable.rarity,
                category = wearable.data.category,
                description = wearable.description,
                imageUri = wearable.ComposeThumbnailUrl(),

                // TODO: solve hidden by field
                hiddenBy = null,
                name = wearable.GetName(),
                hideList = hidesList != null ? hidesList.ToList() : new List<string>(),
                isEquipped = IsEquipped(wearable.id),
                removeList = wearable.data.replaces != null ? wearable.data.replaces.ToList() : new List<string>(),
                wearableId = wearableId,
                unEquipAllowed = CanWearableBeUnEquipped(wearable),
            });

            OnWearableSelected?.Invoke(wearableId);
        }

        private void HandleWearableUnequipped(WearableGridItemModel wearableGridItem, UnequipWearableSource source) =>
            OnWearableUnequipped?.Invoke(wearableGridItem.WearableId, source);

        private void HandleWearableEquipped(WearableGridItemModel wearableGridItem, EquipWearableSource source) =>
            OnWearableEquipped?.Invoke(wearableGridItem.WearableId, source);

        private void FilterWearablesFromReferencePath(string referencePath)
        {
            string[] filters = referencePath.Split('&', StringSplitOptions.RemoveEmptyEntries);

            nameFilter = null;
            categoryFilter = null;

            foreach (string filter in filters)
            {
                if (filter.StartsWith(NAME_FILTER_REF))
                    nameFilter = filter[5..];
                else if (filter.StartsWith(CATEGORY_FILTER_REF))
                    categoryFilter = filter[9..];
            }

            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            ShowWearablesAndUpdateFilters(1, requestWearablesCancellationToken.Token).Forget();
        }

        private void RemoveFiltersFromReferencePath(string referencePath)
        {
            string[] filters = referencePath.Split('&', StringSplitOptions.RemoveEmptyEntries);
            string filter = filters[^1];

            if (filter.StartsWith(NAME_FILTER_REF))
                ClearTextFilter(false);
            else if (filter.StartsWith(CATEGORY_FILTER_REF))
            {
                avatarSlotsHUDController.ClearSlotSelection(filter[9..]);
                categoryFilter = null;
            }

            requestWearablesCancellationToken = requestWearablesCancellationToken.SafeRestart();
            ShowWearablesAndUpdateFilters(1, requestWearablesCancellationToken.Token).Forget();
        }

        private void GoToMarketplace()
        {
            browserBridge.OpenUrl(userProfileBridge.GetOwn().hasConnectedWeb3
                ? URL_MARKET_PLACE
                : URL_GET_A_WALLET);
        }

        private void SetThirdPartCollectionIds(HashSet<string> selectedCollections)
        {
            thirdPartyCollectionIdsFilter = selectedCollections;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
        }

        private void SetSorting((NftOrderByOperation type, bool directionAscendent) newSorting)
        {
            wearableSorting = newSorting;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
        }

        private void SetTextFilter(string newText)
        {
            nameFilter = newText;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
        }

        private void SetCollectionTypeFromFilterSelection(NftCollectionType collectionType)
        {
            collectionTypeMask = collectionType;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
        }

        private void SetCategoryFromFilterSelection(string category, bool supportColor, PreviewCameraFocus previewCameraFocus, bool isSelected)
        {
            ClearTextFilter(false);
            SetCategory(category, isSelected);
        }

        private void ClearTextFilter(bool fetchWearables)
        {
            nameFilter = null;
            backpackFiltersController.ClearTextSearch(false);

            if (fetchWearables)
            {
                filtersCancellationToken = filtersCancellationToken.SafeRestart();
                ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
                view.SetInfoCardVisible(false);
            }
        }

        private void SetCategory(string category, bool isSelected)
        {
            categoryFilter = isSelected ? category : null;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
        }

        private async UniTaskVoid ThrottleLoadWearablesWithCurrentFilters(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken);
            LoadWearables();
        }

        private bool CanWearableBeUnEquipped(WearableItem wearable) =>
            wearable.data.category != WearableLiterals.Categories.BODY_SHAPE &&
            wearable.data.category != WearableLiterals.Categories.EYES &&
            wearable.data.category != WearableLiterals.Categories.MOUTH;

        private bool IsCompatibleWithBodyShape(string bodyShapeId, WearableItem wearable)
        {
            bool isCompatibleWithBodyShape = wearable.data.category
                                                 is WearableLiterals.Categories.BODY_SHAPE
                                                 or WearableLiterals.Categories.SKIN
                                             || wearable.SupportsBodyShape(bodyShapeId);

            return isCompatibleWithBodyShape;
        }
    }
}
