using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class EmoteGridItemComponentView : BaseComponentView<EmoteGridItemModel>
    {
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal GameObject readyContainer;
        [SerializeField] internal GameObject equippedContainer;
        [SerializeField] internal GameObject selectedContainer;
        [SerializeField] internal GameObject hoverUnequippedContainer;
        [SerializeField] internal GameObject hoverEquippedContainer;
        [SerializeField] internal GameObject isNewContainer;
        [SerializeField] internal ImageComponentView image;

        public override void OnFocus()
        {
            base.OnFocus();
            RefreshControl();
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();
            RefreshControl();
        }

        public override void RefreshControl()
        {
            loadingContainer.SetActive(model.IsLoading);
            readyContainer.SetActive(!model.IsLoading);
            equippedContainer.SetActive(model.IsEquipped);
            selectedContainer.SetActive(model.IsSelected);
            hoverEquippedContainer.SetActive(model.IsEquipped && isFocused);
            hoverUnequippedContainer.SetActive(!model.IsEquipped && isFocused);
            isNewContainer.SetActive(model.IsNew);
            image.SetImage(model.ImageUrl);
        }
    }
}
