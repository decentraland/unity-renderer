using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

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
        [SerializeField] private ButtonComponentView actionButton;

        private bool isUnEquipAction;
        private const string UNEQUIP_TEXT = "unequip";
        private const string EQUIP_TEXT = "equip";

        public event Action OnUnEquipWearable;
        public event Action OnEquipWearable;

        public void Start()
        {
            isUnEquipAction = true;
            actionButton.SetText(UNEQUIP_TEXT);
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if(isUnEquipAction)
                {
                    actionButton.SetText(EQUIP_TEXT);
                    isUnEquipAction = false;
                    OnUnEquipWearable?.Invoke();
                }
                else
                {
                    actionButton.SetText(UNEQUIP_TEXT);
                    isUnEquipAction = true;
                    OnEquipWearable?.Invoke();
                }
            });
        }

        public void ClearOnWearableUnequippedEvents()
        {
            OnUnEquipWearable = null;
            OnEquipWearable = null;
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
            string readableName = WearableItem.CATEGORIES_READABLE_MAPPING[categoryName];
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
