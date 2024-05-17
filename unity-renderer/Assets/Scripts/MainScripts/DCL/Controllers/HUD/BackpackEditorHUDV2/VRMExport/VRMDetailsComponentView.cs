using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class VRMDetailsComponentView : BaseComponentView, IVRMDetailsComponentView
    {
        private const string VRM_EXPORT_DETAILS_POOL_ID = "VRMExportDetails";
        [SerializeField] private ButtonComponentView vrmExportButton;
        [SerializeField] private GameObject exportWarningBar;
        [SerializeField] private Button backButton;
        [SerializeField] private VRMDetailItemComponentView vrmDetailPrefab;
        [SerializeField] private Transform wearablesContainer;

        private Pool wearableDetailPool;
        private readonly List<(PoolableObject, VRMDetailItemComponentView)> pooledItems = new ();
        private readonly List<string> equippedItemsURNs = new ();

        public event Action OnBackButtonPressed;
        public event Action OnVRMExportButtonPressed;
        public event Action<VRMItemModel, UnequipWearableSource> OnWearableUnequipped;
        public event Action<VRMItemModel, EquipWearableSource> OnWearableEquipped;

        public override void Awake()
        {
            base.Awake();
            vrmExportButton.onClick.RemoveAllListeners();
            vrmExportButton.onClick.AddListener(OnVRMExportButtonClick);

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => OnBackButtonPressed?.Invoke());

            wearableDetailPool = GetItemDetailPool();
        }

        public override void Dispose()
        {
            base.Dispose();
            TearDown();
            vrmExportButton.onClick.RemoveAllListeners();
            backButton.onClick.RemoveAllListeners();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            TearDown();
        }

        private void TearDown()
        {
            foreach ((PoolableObject pooledItem, VRMDetailItemComponentView component) in pooledItems)
            {
                component.ClearOnWearableUnequippedEvents();
                pooledItem.Release();
            }

            pooledItems.Clear();
        }

        private Pool GetItemDetailPool()
        {
            var entryPool = PoolManager.i.GetPool(VRM_EXPORT_DETAILS_POOL_ID);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                VRM_EXPORT_DETAILS_POOL_ID,
                Instantiate(vrmDetailPrefab).gameObject,
                maxPrewarmCount: 5,
                isPersistent: true);

            entryPool.ForcePrewarm();

            return entryPool;
        }

        private void OnVRMExportButtonClick()
        {
            OnVRMExportButtonPressed?.Invoke();
        }

        private void EnableVRMExport(bool enable)
        {
            vrmExportButton.SetInteractable(enable);
            exportWarningBar.SetActive(!enable);
        }

        public override void RefreshControl() { }

        public void FillVRMBlockingWearablesList(List<NFTDataDTO> itemsToDisplay)
        {
            EnableVRMExport(false);
            equippedItemsURNs.Clear();

            foreach (var itemData in itemsToDisplay)
            {
                equippedItemsURNs.Add(itemData.urn);
                var detailItem = wearableDetailPool.Get();
                var detailComponentView = detailItem.gameObject.GetComponent<VRMDetailItemComponentView>();

                detailComponentView.transform.SetParent(wearablesContainer, false);
                pooledItems.Add((detailItem, detailComponentView));

                var vrmItemModel = new VRMItemModel
                {
                    wearableUrn = itemData.urn,
                    wearableImageUrl = itemData.thumbnail,
                    wearableName = itemData.name,
                    wearableCategoryName = itemData.data.wearable.category,
                    wearableCreatorImageUrl = itemData.creatorImageUrl,
                    wearableCreatorName = itemData.creatorName,
                    wearableCanBeUnEquipped = itemData.canBeUnEquipped,
                };

                detailComponentView.SetModel(vrmItemModel);
                detailComponentView.ClearOnWearableUnequippedEvents();

                detailComponentView.OnUnEquipWearable += () =>
                {
                    equippedItemsURNs.Remove(itemData.urn);

                    if (equippedItemsURNs.Count == 0)
                        EnableVRMExport(true);

                    OnWearableUnequipped?.Invoke(vrmItemModel, UnequipWearableSource.None);
                };

                detailComponentView.OnEquipWearable += () =>
                {
                    equippedItemsURNs.Add(itemData.urn);

                    if (equippedItemsURNs.Count > 0)
                        EnableVRMExport(false);

                    OnWearableEquipped?.Invoke(vrmItemModel, EquipWearableSource.None);
                };
            }
        }
    }
}
