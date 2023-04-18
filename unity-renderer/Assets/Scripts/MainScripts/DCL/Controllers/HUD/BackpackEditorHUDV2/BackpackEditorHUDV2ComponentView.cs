using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        private const int AVATAR_SECTION_INDEX = 0;
        private const int EMOTES_SECTION_INDEX = 1;

        [SerializeField] private SectionSelectorComponentView sectionSelector;
        [SerializeField] private GameObject avatarSection;
        [SerializeField] private GameObject emotesSection;
        [SerializeField] private GridContainerComponentView wearablesGridContainer;
        [SerializeField] private WearableGridItemComponentView wearableGridItemPrefab;
        [SerializeField] private PageSelectorComponentView wearablePageSelector;
        [SerializeField] private PageSelectorComponentView emotePageSelector;

        public override bool isVisible => gameObject.activeInHierarchy;

        private readonly Dictionary<WearableGridItemComponentView, PoolableObject> wearablePooledObjects = new ();
        private readonly Dictionary<string, WearableGridItemComponentView> wearablesById = new ();

        private Transform thisTransform;
        private Pool wearableGridItemsPool;

        public event Action<int> OnWearablePageChanged;
        public event Action<WearableGridItemModel> OnWearableSelected;
        public event Action<WearableGridItemModel> OnWearableEquipped;
        public event Action<WearableGridItemModel> OnWearableUnequipped;

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;

            wearablePageSelector.OnValueChanged += i => OnWearablePageChanged?.Invoke(i);

            wearableGridItemsPool = PoolManager.i.AddPool(
                $"GridWearableItems_{GetInstanceID()}",
                Instantiate(wearableGridItemPrefab).gameObject,
                maxPrewarmCount: 15,
                isPersistent: true);
            wearableGridItemsPool.ForcePrewarm();
        }

        private void Start()
        {
            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                avatarSection.SetActive(isSelected);
            });
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.AddListener((isSelected) =>
            {
                emotesSection.SetActive(isSelected);
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            sectionSelector.GetSection(AVATAR_SECTION_INDEX).onSelect.RemoveAllListeners();
            sectionSelector.GetSection(EMOTES_SECTION_INDEX).onSelect.RemoveAllListeners();
        }

        public static BackpackEditorHUDV2ComponentView Create() =>
            Instantiate(Resources.Load<BackpackEditorHUDV2ComponentView>("BackpackEditorHUDV2"));

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            gameObject.SetActive(false);
        }

        public override void RefreshControl()
        {
        }

        public void Show() =>
            Show(true);

        public void Hide() =>
            Hide(true);

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null)
                return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
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

        public void ShowEmotes(IEnumerable<EmoteGridItemModel> emotes)
        {
            throw new NotImplementedException("Insert emotes into the grid");
        }

        public void ClearEmotes()
        {
            throw new NotImplementedException("Clear emotes from the grid");
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

        private void HandleWearableSelected(WearableGridItemModel model) =>
            OnWearableSelected?.Invoke(model);

        private void HandleWearableEquipped(WearableGridItemModel model) =>
            OnWearableEquipped?.Invoke(model);

        private void HandleWearableUnequipped(WearableGridItemModel model) =>
            OnWearableUnequipped?.Invoke(model);
    }
}
