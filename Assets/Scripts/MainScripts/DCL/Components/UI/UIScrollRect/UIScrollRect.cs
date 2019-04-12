using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;
using DCL.Interface;

namespace DCL.Components
{
    public class UIScrollRect : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float valueX = 0;
            public float valueY = 0;
            public Color borderColor;
            public Color backgroundColor = Color.clear;
            public bool isHorizontal = false;
            public bool isVertical = true;
            public float paddingTop;
            public float paddingRight;
            public float paddingBottom;
            public float paddingLeft;
            public string OnChanged;
        }

        new public UIScrollRectRefContainer referencesContainer
        {
            get { return base.referencesContainer as UIScrollRectRefContainer; }
            set { base.referencesContainer = value; }
        }

        public override string componentName => "UIScrollRect";

        new Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        public UIScrollRect(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIScrollRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        protected override void OnChildComponentAttached(UIShape childComponent)
        {
            base.OnChildComponentAttached(childComponent);
            referencesContainer.isDirty = true;
            childComponent.OnShapeUpdated += SetContentDirty;
        }

        protected override void OnChildComponentDettached(UIShape childComponent)
        {
            base.OnChildComponentDettached(childComponent);
            referencesContainer.isDirty = true;
            childComponent.OnShapeUpdated -= SetContentDirty;
        }

        void SetContentDirty()
        {
            referencesContainer.isDirty = true;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<UIScrollRectRefContainer>("UIScrollRect");
            }
            else
            {
                ReparentComponent(referencesContainer.rectTransform, model.parentComponent);
            }

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            yield return ResizeAlignAndReposition(  referencesContainer.layoutElementRT,
                                                    parentRecTransform.rect.width,
                                                    parentRecTransform.rect.height,
                                                    referencesContainer.alignmentLayoutGroup,
                                                    referencesContainer.layoutElement);

            UIScrollRectRefContainer rc = referencesContainer;

            rc.canvasGroup.blocksRaycasts = model.isPointerBlocker;
            rc.canvasGroup.alpha = model.opacity;

            rc.contentBackground.color = model.backgroundColor;

            // Apply padding
            rc.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            rc.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            rc.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            rc.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            rc.isHorizontal = model.isHorizontal;
            rc.isVertical = model.isVertical;

            rc.HScrollbar.value = model.valueX;
            rc.VScrollbar.value = model.valueY;

            rc.scrollRect.onValueChanged.AddListener(OnChanged);

            referencesContainer.isDirty = true;
            yield break;
        }

        void OnChanged(Vector2 scrollingValues)
        {
            WebInterface.ReportOnScrollChange(scene.sceneData.id, model.OnChanged, scrollingValues, "");
        }
        
        public override void Dispose()
        {
            referencesContainer.scrollRect.onValueChanged.RemoveAllListeners();
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
