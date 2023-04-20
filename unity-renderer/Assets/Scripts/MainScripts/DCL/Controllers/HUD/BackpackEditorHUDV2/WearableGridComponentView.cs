using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class WearableGridComponentView : MonoBehaviour, IWearableGridView
    {
        [SerializeField] private NftBreadcrumbComponentView wearablesBreadcrumbComponentView;
        [SerializeField] private GridContainerComponentView wearablesGridContainer;
        [SerializeField] private WearableGridItemComponentView wearableGridItemPrefab;
        [SerializeField] private PageSelectorComponentView wearablePageSelector;

        private readonly Dictionary<WearableGridItemComponentView, PoolableObject> wearablePooledObjects = new ();
        private readonly Dictionary<string, WearableGridItemComponentView> wearablesById = new ();

        private Pool wearableGridItemsPool;

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
        }

        public void SelectWearable(string wearableId) =>
            wearablesById[wearableId].Select();

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
