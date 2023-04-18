using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class InfoCardComponentView : BaseComponentView<InfoCardComponentModel>, IInfoCardComponentView
    {
        [SerializeField] internal NftTypeIconSO typeIcons;
        [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
        [SerializeField] private TMP_Text wearableName;
        [SerializeField] private TMP_Text wearableDescription;
        [SerializeField] private Image categoryImage;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button viewMore;
        [SerializeField] private DynamicListComponentView removesList;
        [SerializeField] private DynamicListComponentView hidesList;
        [SerializeField] private DynamicListComponentView hiddenBy;

        public void Start()
        {
            equipButton.onClick.RemoveAllListeners();
            viewMore.onClick.RemoveAllListeners();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetName(model.name);
            SetDescription(model.description);
            SetCategory(model.category);
        }

        public void SetName(string nameText)
        {
            model.name = nameText;
            wearableName.text = nameText;
        }

        public void SetDescription(string description)
        {
            model.description = description;
            wearableDescription.text = description;
        }

        public void SetCategory(string category)
        {
            model.category = category;
            categoryImage.sprite = typeIcons.GetTypeImage(category);
        }

        public void SetHidesList(List<string> hideList)
        {
            hidesList.RemoveIcons();
            foreach (string hideCategory in hideList)
                hidesList.AddIcon(typeIcons.GetTypeImage(hideCategory));
        }

        public void SetRemovesList(List<string> removeList)
        {
            removesList.RemoveIcons();
            foreach (string removeCategory in removeList)
                removesList.AddIcon(typeIcons.GetTypeImage(removeCategory));
        }

        public void SetHiddenBy()
        {

        }
    }
}
