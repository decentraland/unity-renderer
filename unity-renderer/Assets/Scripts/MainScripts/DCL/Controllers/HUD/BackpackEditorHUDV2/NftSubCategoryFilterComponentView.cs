using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class NftSubCategoryFilterComponentView : BaseComponentView<NftSubCategoryFilterModel>
    {
        [SerializeField] internal Button navigateButton;
        [SerializeField] internal Button exitButton;
        [SerializeField] internal TMP_Text categoryName;
        [SerializeField] internal Image icon;
        [SerializeField] internal Image backgroundImage;
        [SerializeField] internal Color selectedBackgroundColor;
        [SerializeField] internal Color selectedFontColor;
        [SerializeField] internal Color unselectedBackgroundColor;
        [SerializeField] internal Color unselectedFontColor;
        [SerializeField] internal Color selectedIconColor;
        [SerializeField] internal Color unselectedIconColor;

        public event Action<NftSubCategoryFilterModel> OnNavigate;
        public event Action<NftSubCategoryFilterModel> OnExit;

        public NftSubCategoryFilterModel Model => model;

        public override void Awake()
        {
            base.Awake();

            navigateButton.onClick.AddListener(() => OnNavigate?.Invoke(model));
            exitButton.onClick.AddListener(() => OnExit?.Invoke(model));
        }

        public override void RefreshControl()
        {
            categoryName.text = model.Name;

            if (model.ShowResultCount)
                categoryName.text += $" ({model.ResultCount})";

            exitButton.gameObject.SetActive(model.ShowRemoveButton);

            icon.sprite = model.Icon;
            icon.gameObject.SetActive(model.Icon != null);

            backgroundImage.color = model.IsSelected ? selectedBackgroundColor : unselectedBackgroundColor;
            categoryName.color = model.IsSelected ? selectedFontColor : unselectedFontColor;
            icon.color = model.IsSelected ? selectedIconColor : unselectedIconColor;
        }
    }
}
