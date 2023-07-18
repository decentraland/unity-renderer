using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.HUD.Common
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ContentSizeFitterMaxHeight : UIBehaviour, ILayoutSelfController
    {
        [SerializeField] private float maxHeight;

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
        }

        public virtual void SetLayoutVertical()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Min(LayoutUtility.GetPreferredSize(rectTransform, 1), maxHeight));
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
