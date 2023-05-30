using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class WearableGridComponentView : MonoBehaviour, IWearableGridView
    {
        [SerializeField] internal NftBreadcrumbComponentView wearablesBreadcrumbComponentView;
        [SerializeField] internal GridLayoutGroup wearablesGridContainer;
        [SerializeField] internal WearableGridItemComponentView wearableGridItemPrefab;
        [SerializeField] internal PageSelectorComponentView wearablePageSelector;
        [SerializeField] internal InfoCardComponentView infoCardComponentView;
        [SerializeField] internal GameObject emptyStateContainer;
        [SerializeField] internal Button goToMarketplaceButton;
        [SerializeField] internal GameObject loadingSpinner;

        private readonly Dictionary<WearableGridItemComponentView, PoolableObject> wearablePooledObjects = new ();
        private readonly Dictionary<string, WearableGridItemComponentView> wearablesById = new ();

        private Pool wearableGridItemsPool;
        private WearableGridItemComponentView selectedWearableItem;

        public event Action<int> OnWearablePageChanged;
        public event Action<string> OnFilterSelected;
        public event Action<string> OnFilterRemoved;
        public event Action OnGoToMarketplace;
        public event Action<WearableGridItemModel> OnWearableSelected;
        public event Action<WearableGridItemModel, EquipWearableSource> OnWearableEquipped;
        public event Action<WearableGridItemModel, UnequipWearableSource> OnWearableUnequipped;

        private void Awake()
        {
            wearablePageSelector.OnValueChanged += i => OnWearablePageChanged?.Invoke(i + 1);

            wearableGridItemsPool = PoolManager.i.AddPool(
                $"GridWearableItems_{GetInstanceID()}",
                Instantiate(wearableGridItemPrefab).gameObject,
                maxPrewarmCount: 15,
                isPersistent: true);

            wearableGridItemsPool.ForcePrewarm();

            wearablesBreadcrumbComponentView.OnFilterSelected += reference => OnFilterSelected?.Invoke(reference);
            wearablesBreadcrumbComponentView.OnFilterRemoved += reference => OnFilterRemoved?.Invoke(reference);

            infoCardComponentView.OnEquipWearable += () => OnWearableEquipped?.Invoke(selectedWearableItem.Model, EquipWearableSource.InfoCard);
            infoCardComponentView.OnUnEquipWearable += () => OnWearableUnequipped?.Invoke(selectedWearableItem.Model, UnequipWearableSource.InfoCard);

            goToMarketplaceButton.onClick.AddListener(() => OnGoToMarketplace?.Invoke());
        }

        private void OnEnable()
        {
            ClearWearableSelection();
        }

        public void Dispose()
        {
            if (this && gameObject)
                Destroy(gameObject);
        }

        public void SetWearablePages(int pageNumber, int totalPages)
        {
            if (totalPages <= 1)
            {
                wearablePageSelector.gameObject.SetActive(false);
                return;
            }

            wearablePageSelector.gameObject.SetActive(true);
            wearablePageSelector.Setup(totalPages, true);
            wearablePageSelector.SelectPage(pageNumber - 1, false);
        }

        public void ShowWearables(IEnumerable<WearableGridItemModel> wearables)
        {
            foreach (WearableGridItemModel wearable in wearables)
                SetWearable(wearable);

            UpdateEmptyState();
        }

        public void ClearWearables()
        {
            foreach ((WearableGridItemComponentView wearableGridItem, PoolableObject poolObj) in wearablePooledObjects)
            {
                wearableGridItem.OnSelected -= HandleWearableSelected;
                wearableGridItem.OnEquipped -= HandleWearableEquipped;
                wearableGridItem.OnUnequipped -= HandleWearableUnequipped;

                poolObj.Release();
            }

            wearablePooledObjects.Clear();
            wearablesById.Clear();

            UpdateEmptyState();
        }

        public void SetWearable(WearableGridItemModel model)
        {
            if (wearablesById.TryGetValue(model.WearableId, out var view))
                view.SetModel(model);
            else
            {
                PoolableObject poolObj = wearableGridItemsPool.Get();
                WearableGridItemComponentView wearableGridItem = poolObj.gameObject.GetComponent<WearableGridItemComponentView>();
                wearablePooledObjects[wearableGridItem] = poolObj;
                wearablesById[model.WearableId] = wearableGridItem;
                wearableGridItem.SetModel(model);
                wearableGridItem.transform.SetParent(wearablesGridContainer.transform, false);
                wearableGridItem.OnSelected += HandleWearableSelected;
                wearableGridItem.OnEquipped += HandleWearableEquipped;
                wearableGridItem.OnUnequipped += HandleWearableUnequipped;
            }

            if (model.IsEquipped)
                infoCardComponentView.Equip(model.WearableId);
            else
                infoCardComponentView.UnEquip(model.WearableId);

            UpdateEmptyState();
        }

        public void ClearWearableSelection()
        {
            foreach (WearableGridItemComponentView view in wearablePooledObjects.Keys)
                view.Unselect();

            selectedWearableItem = null;
            SetInfoCardVisible(false);
        }

        public void SelectWearable(string wearableId)
        {
            selectedWearableItem = wearablesById[wearableId];
            selectedWearableItem.Select();
            SetInfoCardVisible(true);
        }

        public void FillInfoCard(InfoCardComponentModel model) =>
            infoCardComponentView.SetModel(model);

        public void SetInfoCardVisible(bool isVisible) =>
            infoCardComponentView.SetVisible(isVisible);

        public void SetLoadingActive(bool isActive)
        {
            loadingSpinner.SetActive(isActive);
            UpdateEmptyState();
        }

        public void SetWearableBreadcrumb(NftBreadcrumbModel model) =>
            wearablesBreadcrumbComponentView.SetModel(model);

        public void RefreshWearable(string wearableId)
        {
            if (!wearablesById.TryGetValue(wearableId, out var view))
                return;

            view.RefreshControl();
        }

        public void RefreshAllWearables()
        {
            foreach (WearableGridItemComponentView view in wearablePooledObjects.Keys)
                view.RefreshControl();
        }

        private void HandleWearableSelected(WearableGridItemModel model) =>
            OnWearableSelected?.Invoke(model);

        private void HandleWearableEquipped(WearableGridItemModel model) =>
            OnWearableEquipped?.Invoke(model, EquipWearableSource.Wearable);

        private void HandleWearableUnequipped(WearableGridItemModel model) =>
            OnWearableUnequipped?.Invoke(model, UnequipWearableSource.Wearable);

        private void UpdateEmptyState()
        {
            bool isEmpty = wearablesById.Count == 0;

            wearablesGridContainer.gameObject.SetActive(!isEmpty && !loadingSpinner.activeSelf);
            emptyStateContainer.SetActive(isEmpty && !loadingSpinner.activeSelf);
        }
    }
}
