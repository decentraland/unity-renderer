using System;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        public static BackpackEditorHUDV2ComponentView Create() =>
            Instantiate(Resources.Load<BackpackEditorHUDV2ComponentView>("BackpackEditorHUDV2"));

        public override void Show(bool instant = false)
        {
            base.Show(instant);
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
        }

        public override void RefreshControl()
        {
            throw new NotImplementedException();
        }
    }
}
