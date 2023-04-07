using System;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        public override bool isVisible => gameObject.activeInHierarchy;

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
            throw new NotImplementedException();
        }

        public void Show() =>
            Show(true);

        public void Hide() =>
            Hide(true);
    }
}
