using System;
using TMPro;
using UnityEngine;

namespace UIComponents.Scripts.Components.Tooltip
{
    public class TooltipComponentView : BaseComponentView<TooltipComponentModel>
    {
        [SerializeField] private TMP_Text text;

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            base.Show(instant);
        }

        public override void RefreshControl()
        {
            SetText(model.message);
        }

        private void SetText(string newText)
        {
            model.message = newText;

            if (text != null)
                text.text = newText;
        }
    }
}
