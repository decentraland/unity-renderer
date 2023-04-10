using System;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentView : BaseComponentView<BackpackEditorHUDModel>, IBackpackEditorHUDView
    {
        public override bool isVisible => gameObject.activeInHierarchy;

        private Transform thisTransform;

        public override void Awake()
        {
            base.Awake();
            thisTransform = transform;
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
    }
}
