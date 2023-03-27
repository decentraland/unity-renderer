using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.HUD.Common
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ContentSizeFitterMaxWidth : UIBehaviour, ILayoutSelfController
    {
        [SerializeField] private float maxWidth;

        private RectTransform internalRectTransform;
        private RectTransform rectTransform
        {
            get
            {
                if (internalRectTransform == null)
                    internalRectTransform = GetComponent<RectTransform>();
                return internalRectTransform;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        public virtual void SetLayoutHorizontal()
        {
            rectTransform.SetSizeWithCurrentAnchors(0,
                Mathf.Min(maxWidth, LayoutUtility.GetPreferredSize(rectTransform, 0)));
        }

        public virtual void SetLayoutVertical()
        {
        }

        private void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
    #endif
    }
}
