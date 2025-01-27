using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.CustomNftCollection;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

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
        private const string EMPTY_WEARABLE_DESCRIPTION = "This item doesn’t have a description.";

        private readonly IWearableGridView view;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly DataStore_BackpackV2 dataStoreBackpackV2;
        private readonly IBrowserBridge browserBridge;
        private readonly BackpackFiltersController backpackFiltersController;
        private readonly AvatarSlotsHUDController avatarSlotsHUDController;
        private readonly IBackpackAnalyticsService backpackAnalyticsService;
        private readonly ICustomNftCollectionService customNftCollectionService;
        private readonly List<WearableItem> customWearablesBuffer = new ();

        private Dictionary<string, WearableGridItemModel> currentWearables = new ();
        private CancellationTokenSource requestWearablesCancellationToken = new ();
        private CancellationTokenSource filtersCancellationToken = new ();
        private string categoryFilter;
        private ICollection<string> thirdPartyCollectionIdsFilter;
        private string nameFilter;

        // initialize as "newest"
        private (NftOrderByOperation type, bool directionAscendent)? wearableSorting = new (NftOrderByOperation.Date, false);
        private NftCollectionType collectionTypeMask = NftCollectionType.Base | NftCollectionType.OnChain;

        public event Action<string> OnWearableSelected;
        public event Action<string, EquipWearableSource> OnWearableEquipped;
        public event Action<string, UnequipWearableSource> OnWearableUnequipped;
        public event Action OnCategoryFilterRemoved;

        public WearableGridController(IWearableGridView view,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            DataStore_BackpackV2 dataStoreBackpackV2,
            IBrowserBridge browserBridge,
            BackpackFiltersController backpackFiltersController,
            AvatarSlotsHUDController avatarSlotsHUDController,
            IBackpackAnalyticsService backpackAnalyticsService,
            ICustomNftCollectionService customNftCollectionService)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.dataStoreBackpackV2 = dataStoreBackpackV2;
            this.browserBridge = browserBridge;
            this.backpackFiltersController = backpackFiltersController;
            this.avatarSlotsHUDController = avatarSlotsHUDController;
            this.backpackAnalyticsService = backpackAnalyticsService;
            this.customNftCollectionService = customNftCollectionService;

            view.OnWearablePageChanged += HandleNewPageRequested;
            view.OnWearableEquipped += HandleWearableEquipped;
            view.OnWearableUnequipped += HandleWearableUnequipped;
            view.OnWearableSelected += HandleWearableSelected;
            view.OnFilterSelected += FilterWearablesFromReferencePath;
            view.OnFilterRemoved += RemoveFiltersFromReferencePath;
            view.OnGoToMarketplace += GoToMarketplace;

            backpackFiltersController.OnThirdPartyCollectionChanged += SetThirdPartCollectionIds;
            backpackFiltersController.OnSortByChanged += SetSorting;
            backpackFiltersController.OnSearchTextChanged += SetNameFilterFromSearchText;
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
            backpackFiltersController.OnSearchTextChanged -= SetNameFilterFromSearchText;
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
            view.RefreshAllWearables();
        }

        public void UnEquip(string wearableId)
        {
            if (!currentWearables.TryGetValue(wearableId, out WearableGridItemModel wearableGridModel))
                return;

            wearableGridModel.IsEquipped = false;
            view.SetWearable(wearableGridModel);
            view.RefreshWearable(wearableId);
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

        public void ResetFilters()
        {
            categoryFilter = null;
            collectionTypeMask = NftCollectionType.Base | NftCollectionType.OnChain;
            thirdPartyCollectionIdsFilter = null;
            nameFilter = null;
            wearableSorting = new (NftOrderByOperation.Date, false);
        }

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

            if (string.IsNullOrEmpty(categoryFilter))
            {
                avatarSlotsHUDController.ClearSlotSelection();
                OnCategoryFilterRemoved?.Invoke();
            }
            else
                avatarSlotsHUDController.SelectSlot(categoryFilter, false);

            if (string.IsNullOrEmpty(nameFilter))
                backpackFiltersController.ClearTextSearch(false);
            else
                backpackFiltersController.SetTextSearch(nameFilter, false);

            if (wearableSorting != null)
            {
                (NftOrderByOperation type, bool directionAscending) = wearableSorting.Value;
                backpackFiltersController.SetSorting(type, directionAscending, false);
            }

            backpackFiltersController.SelectCollections(collectionTypeMask, thirdPartyCollectionIdsFilter, false);

            int resultCount = await RequestWearablesAndShowThem(page, cancellationToken);

            // This call has been removed to avoid flickering. The result count is hidden in the view
            // view.SetWearableBreadcrumb(wearableBreadcrumbModel with { ResultCount = resultCount });
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

                view.SetLoadingActive(true);

                List<WearableItem> wearables = new ();

                (IReadOnlyList<WearableItem> ownedWearables, int totalAmount) = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    "0x3a55404f3b6B40876512fE612711e76d3714F49B".ToLower(),
                    page,
                    PAGE_SIZE, cancellationToken,
                    categoryFilter, NftRarity.None, collectionTypeMask,
                    thirdPartyCollectionIdsFilter,
                    nameFilter, wearableSorting);

                wearables.AddRange(ownedWearables);

                try
                {
                    totalAmount += await MergePublishedWearableCollections(page, wearables, totalAmount, cancellationToken);
                    totalAmount += await MergeBuilderWearableCollections(page, wearables, totalAmount, cancellationToken);
                    totalAmount += await MergeCustomWearableItems(page, wearables, totalAmount, cancellationToken);

                    customWearablesBuffer.Clear();
                }
                catch (Exception e) when (e is not OperationCanceledException) { Debug.LogError(e); }

                view.SetLoadingActive(false);

                currentWearables = wearables.Select(ToWearableGridModel)
                                            .ToDictionary(item => ExtendedUrnParser.GetShortenedUrn(item.WearableId), model => model);

                view.SetWearablePages(page, Mathf.CeilToInt((float)totalAmount / PAGE_SIZE));

                // TODO: mark the wearables to be disposed if no references left
                view.ClearWearables();
                view.ShowWearables(currentWearables.Values);

                return totalAmount;
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }

            return 0;
        }

        private async UniTask<int> MergeCustomWearableItems(int page, List<WearableItem> wearables,
            int totalAmount, CancellationToken cancellationToken) =>
            await MergeToWearableResults(page, wearables, totalAmount, FetchCustomWearableItems, cancellationToken);

        private async UniTask<int> MergePublishedWearableCollections(int page, List<WearableItem> wearables, int totalAmount,
            CancellationToken cancellationToken) =>
             await MergeToWearableResults(page, wearables, totalAmount, FetchPublishedWearableCollections, cancellationToken);

        private async UniTask<int> MergeToWearableResults(int page, List<WearableItem> wearables, int totalAmount,
            Func<List<WearableItem>, CancellationToken, UniTask> fetchOperation,
            CancellationToken cancellationToken)
        {
            int startingPage = (totalAmount / PAGE_SIZE) + 1;
            int pageOffset = page - startingPage;
            int pageSize = PAGE_SIZE - wearables.Count;
            int skip = pageOffset * PAGE_SIZE;
            int until = skip + pageSize;

            customWearablesBuffer.Clear();

            await fetchOperation.Invoke(customWearablesBuffer, cancellationToken);

            if (skip < 0) return customWearablesBuffer.Count;

            for (int i = skip; i < customWearablesBuffer.Count && i < until; i++)
                wearables.Add(customWearablesBuffer[i]);

            return customWearablesBuffer.Count;
        }

        private async UniTask<int> MergeBuilderWearableCollections(int page, List<WearableItem> wearables, int totalAmount, CancellationToken cancellationToken)
        {
            int startingPage = (totalAmount / PAGE_SIZE) + 1;
            int pageOffset = Mathf.Max(1, page - startingPage);
            int pageSize = PAGE_SIZE - wearables.Count;

            customWearablesBuffer.Clear();

            int collectionsWearableCount = await FetchBuilderWearableCollections(pageOffset, PAGE_SIZE,
                customWearablesBuffer, cancellationToken);

            for (var i = 0; i < pageSize && i < customWearablesBuffer.Count; i++)
                wearables.Add(customWearablesBuffer[i]);

            return collectionsWearableCount;
        }

        private async UniTask FetchCustomWearableItems(ICollection<WearableItem> wearables, CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customItems = await customNftCollectionService.GetConfiguredCustomNftItemsAsync(cancellationToken);

            WearableItem[] retrievedWearables = await UniTask.WhenAll(customItems.Select(nftId =>
            {
                if (nftId.StartsWith("urn", StringComparison.OrdinalIgnoreCase))
                    return wearablesCatalogService.RequestWearableAsync(nftId, cancellationToken);

                return wearablesCatalogService.RequestWearableFromBuilderAsync(nftId, cancellationToken);
            }));

            foreach (WearableItem wearable in retrievedWearables)
            {
                if (wearable == null)
                {
                    Debug.LogWarning("Custom wearable item skipped is null");
                    continue;
                }

                wearables.Add(wearable);
            }
        }

        private async UniTask FetchPublishedWearableCollections(
            List<WearableItem> wearableBuffer, CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customCollections =
                await customNftCollectionService.GetConfiguredCustomNftCollectionAsync(cancellationToken);

            HashSet<string> collectionsToRequest = HashSetPool<string>.Get();

            foreach (string collectionId in customCollections)
                if (collectionId.StartsWith("urn", StringComparison.OrdinalIgnoreCase))
                    collectionsToRequest.Add(collectionId);

            await wearablesCatalogService.RequestWearableCollection(collectionsToRequest, cancellationToken, wearableBuffer);

            HashSetPool<string>.Release(collectionsToRequest);
        }

        private async UniTask<int> FetchBuilderWearableCollections(
            int pageNumber, int pageSize,
            List<WearableItem> wearableBuffer,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<string> customCollections =
                await customNftCollectionService.GetConfiguredCustomNftCollectionAsync(cancellationToken);

            HashSet<string> collectionsToRequest = HashSetPool<string>.Get();

            foreach (string collectionId in customCollections)
                if (!collectionId.StartsWith("urn", StringComparison.OrdinalIgnoreCase))
                    collectionsToRequest.Add(collectionId);

            (IReadOnlyList<WearableItem> _, int totalAmount) = await wearablesCatalogService.RequestWearableCollectionInBuilder(
                collectionsToRequest, cancellationToken,
                collectionBuffer: wearableBuffer,
                nameFilter: nameFilter,
                pageNumber: pageNumber, pageSize: pageSize);

            HashSetPool<string>.Release(collectionsToRequest);

            return totalAmount;
        }

        private WearableGridItemModel ToWearableGridModel(WearableItem wearable)
        {
            NftRarity rarity;

            if (string.IsNullOrEmpty(wearable.rarity))
                rarity = NftRarity.None;
            else if (!Enum.TryParse(wearable.rarity, true, out NftRarity result))
            {
                rarity = NftRarity.None;
                Debug.LogWarning($"Could not parse the rarity \"{wearable.rarity}\" of the wearable '{wearable.id}'. Fallback to common.");
            }
            else
                rarity = result;

            string currentBodyShapeId = dataStoreBackpackV2.previewBodyShape.Get();

            return new WearableGridItemModel
            {
                WearableId = wearable.id,
                Rarity = rarity,
                Category = wearable.data.category,
                ImageUrl = wearable.ComposeThumbnailUrl(),
                IsEquipped = IsEquipped(ExtendedUrnParser.GetShortenedUrn(wearable.id)),
                IsNew = (DateTime.UtcNow - wearable.MostRecentTransferredDate).TotalHours < 24,
                IsSelected = false,
                UnEquipAllowed = wearable.CanBeUnEquipped(),
                IsCompatibleWithBodyShape = IsCompatibleWithBodyShape(currentBodyShapeId, wearable),
                IsSmartWearable = wearable.IsSmart(),
                Amount = wearable.amount > 1 ? $"x{wearable.amount.ToString()}" : "",
            };
        }

        private bool IsEquipped(string wearableId) =>
            dataStoreBackpackV2.previewEquippedWearables.Contains(wearableId)
            || wearableId == dataStoreBackpackV2.previewBodyShape.Get();

        private void HandleWearableSelected(WearableGridItemModel wearableGridItem)
        {
            string wearableId = wearableGridItem.WearableId;
            string shortenedWearableId = ExtendedUrnParser.GetShortenedUrn(wearableId);

            view.ClearWearableSelection();
            view.SelectWearable(shortenedWearableId);

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
                description = string.IsNullOrEmpty(wearable.description) ? EMPTY_WEARABLE_DESCRIPTION : wearable.description,
                imageUri = wearable.ComposeThumbnailUrl(),

                // TODO: solve hidden by field
                hiddenBy = null,
                name = wearable.GetName(),
                hideList = hidesList != null ? hidesList.ToList() : new List<string>(),
                isEquipped = IsEquipped(shortenedWearableId),
                removeList = wearable.data.replaces != null ? wearable.data.replaces.ToList() : new List<string>(),
                wearableId = wearableId,
                unEquipAllowed = wearable.CanBeUnEquipped(),
                blockVrmExport = wearable.data.blockVrmExport,
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
                nameFilter = null;
            else if (filter.StartsWith(CATEGORY_FILTER_REF))
                categoryFilter = null;

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
            backpackAnalyticsService.SendWearableSortedBy(newSorting.type, newSorting.directionAscendent);
        }

        private void SetNameFilterFromSearchText(string newText)
        {
            categoryFilter = null;
            collectionTypeMask = NftCollectionType.All;
            thirdPartyCollectionIdsFilter?.Clear();
            nameFilter = newText;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
            backpackAnalyticsService.SendWearableSearch(newText);
        }

        private void SetCollectionTypeFromFilterSelection(NftCollectionType collectionType)
        {
            nameFilter = null;
            collectionTypeMask = collectionType;
            filtersCancellationToken = filtersCancellationToken.SafeRestart();
            ThrottleLoadWearablesWithCurrentFilters(filtersCancellationToken.Token).Forget();
            view.SetInfoCardVisible(false);
            backpackAnalyticsService.SendWearableFilter(!collectionType.HasFlag(NftCollectionType.Base));
        }

        private void SetCategoryFromFilterSelection(string category, bool supportColor, PreviewCameraFocus previewCameraFocus, bool isSelected)
        {
            nameFilter = null;
            SetCategory(category, isSelected);
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
