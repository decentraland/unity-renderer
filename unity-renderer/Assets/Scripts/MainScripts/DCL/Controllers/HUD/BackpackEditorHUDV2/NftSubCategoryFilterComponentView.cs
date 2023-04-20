using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class NftSubCategoryFilterComponentView : BaseComponentView<NftSubCategoryFilterModel>
    {
        [SerializeField] private Button navigateButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text categoryName;
        [SerializeField] private Image icon;

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

            icon.sprite = model.Icon;
        }
    }
}
