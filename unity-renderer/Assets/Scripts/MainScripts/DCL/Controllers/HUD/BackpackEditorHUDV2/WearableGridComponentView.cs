using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class WearableGridComponentView : MonoBehaviour, IWearableGridView
    {
        [SerializeField] internal NftBreadcrumbComponentView wearablesBreadcrumbComponentView;
        [SerializeField] internal GridContainerComponentView wearablesGridContainer;
        [SerializeField] internal WearableGridItemComponentView wearableGridItemPrefab;
        [SerializeField] internal PageSelectorComponentView wearablePageSelector;
        [SerializeField] internal InfoCardComponentView infoCardComponentView;

        private readonly Dictionary<WearableGridItemComponentView, PoolableObject> wearablePooledObjects = new ();
        private readonly Dictionary<string, WearableGridItemComponentView> wearablesById = new ();

        private Pool wearableGridItemsPool;
        private WearableGridItemComponentView selectedWearableItem;

        public event Action<int> OnWearablePageChanged;
        public event Action<string> OnFilterWearables;
        public event Action<WearableGridItemModel> OnWearableSelected;
        public event Action<WearableGridItemModel> OnWearableEquipped;
        public event Action<WearableGridItemModel> OnWearableUnequipped;

        private void Awake()
        {
            wearablePageSelector.OnValueChanged += i => OnWearablePageChanged?.Invoke(i);

            wearableGridItemsPool = PoolManager.i.AddPool(
                $"GridWearableItems_{GetInstanceID()}",
                Instantiate(wearableGridItemPrefab).gameObject,
                maxPrewarmCount: 15,
                isPersistent: true);
            wearableGridItemsPool.ForcePrewarm();

            wearablesBreadcrumbComponentView.OnNavigate += reference => OnFilterWearables?.Invoke(reference);

            infoCardComponentView.OnEquipWearable += () => OnWearableEquipped?.Invoke(selectedWearableItem.Model);
            infoCardComponentView.OnUnEquipWearable += () => OnWearableUnequipped?.Invoke(selectedWearableItem.Model);
        }

        public void Dispose()
        {
            if (this && gameObject)
                Destroy(gameObject);
        }

        public void SetWearablePages(int currentPage, int totalPages)
        {
            if (totalPages <= 1)
            {
                wearablePageSelector.gameObject.SetActive(false);
                return;
            }
            wearablePageSelector.gameObject.SetActive(true);
            wearablePageSelector.SetModel(new PageSelectorModel
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
            });
        }

        public void ShowWearables(IEnumerable<WearableGridItemModel> wearables)
        {
            foreach (WearableGridItemModel wearable in wearables)
                SetWearable(wearable);
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
                wearablesGridContainer.AddItem(wearableGridItem);

                wearableGridItem.OnSelected += HandleWearableSelected;
                wearableGridItem.OnEquipped += HandleWearableEquipped;
                wearableGridItem.OnUnequipped += HandleWearableUnequipped;
            }
        }

        public void ClearWearableSelection()
        {
            foreach (WearableGridItemComponentView view in wearablePooledObjects.Keys)
                view.Unselect();

            selectedWearableItem = null;
        }

        public void SelectWearable(string wearableId)
        {
            selectedWearableItem = wearablesById[wearableId];
            selectedWearableItem.Select();
        }

        public void FillInfoCard(InfoCardComponentModel model) =>
            infoCardComponentView.SetModel(model);

        public void SetWearableBreadcrumb(NftBreadcrumbModel model) =>
            wearablesBreadcrumbComponentView.SetModel(model);

        private void HandleWearableSelected(WearableGridItemModel model) =>
            OnWearableSelected?.Invoke(model);

        private void HandleWearableEquipped(WearableGridItemModel model) =>
            OnWearableEquipped?.Invoke(model);

        private void HandleWearableUnequipped(WearableGridItemModel model) =>
            OnWearableUnequipped?.Invoke(model);
    }
}
