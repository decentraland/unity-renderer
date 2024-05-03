using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class VRMDetailItemComponentView : BaseComponentView<VRMItemModel>
    {
        [SerializeField] private NftTypeIconSO typeIcons;
        [SerializeField] private ImageComponentView wearableImage;
        [SerializeField] private TMP_Text wearableName;
        [SerializeField] private ImageComponentView wearableTypeImage;
        [SerializeField] private TMP_Text wearableTypeName;
        [SerializeField] private ImageComponentView wearableCreatorImage;
        [SerializeField] private TMP_Text wearableCreatorName;
        [SerializeField] private Button actionButton;

        public event Action OnUnEquipWearable;

        public void Start()
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                OnUnEquipWearable?.Invoke();
            });
        }

        public void ClearOnWearableUnequippedEvents()
        {
            OnUnEquipWearable = null;
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetWearableImage(model.wearableImageUrl);
            SetWearableName(model.wearableName);

            SetWearableCategoryImage(model.wearableCategoryName);
            SetWearableCategoryName(model.wearableCategoryName);

            SetWearableCreatorImage(model.wearableCreatorImageUrl);
            SetWearableCreatorName(model.wearableCreatorName);
        }

        private void SetWearableImage(string modelWearableImageUrl)
        {
            model.wearableImageUrl = modelWearableImageUrl;

            if (string.IsNullOrEmpty(modelWearableImageUrl))
            {
                wearableImage.SetImage(Texture2D.grayTexture);
                return;
            }

            wearableImage.SetImage(modelWearableImageUrl);
        }

        private void SetWearableName(string name)
        {
            model.wearableName = name;
            wearableName.text = name;
        }

        private void SetWearableCategoryImage(string categoryName)
        {
            var categoryIcon = typeIcons.GetTypeImage(categoryName);

            if (categoryIcon)
                wearableTypeImage.SetImage(categoryIcon);
            else
                wearableTypeImage.SetImage(Texture2D.grayTexture);
        }

        private void SetWearableCategoryName(string categoryName)
        {
            var readableName = WearableItem.CATEGORIES_READABLE_MAPPING[categoryName];
            model.wearableCategoryName = readableName;
            wearableTypeName.text = readableName;
        }

        private void SetWearableCreatorImage(string creatorImageUrl)
        {
            model.wearableCreatorImageUrl = creatorImageUrl;

            if (string.IsNullOrEmpty(creatorImageUrl))
            {
                wearableCreatorImage.SetImage(Texture2D.grayTexture);
                return;
            }

            wearableCreatorImage.SetImage(creatorImageUrl);
        }

        private void SetWearableCreatorName(string creatorName)
        {
            model.wearableCreatorName = creatorName;
            wearableCreatorName.text = creatorName;
        }
    }
}
