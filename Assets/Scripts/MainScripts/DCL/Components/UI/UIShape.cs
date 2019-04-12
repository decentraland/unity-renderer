using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;
using System;

namespace DCL.Components
{
    [System.Serializable]
    public struct UIValue
    {
        public enum Unit
        {
            PERCENT,
            PIXELS
        }

        public float value;
        public Unit type;

        public UIValue(float value, Unit unitType = Unit.PIXELS)
        {
            this.value = value;
            this.type = unitType;
        }

        public float GetScaledValue(float parentSize)
        {
            float tmpValue = value;

            if (type == Unit.PERCENT)
                tmpValue /= 100;

            return tmpValue * (type == Unit.PIXELS ? 1 : parentSize);
        }
    }

    public class UIShape : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string parentComponent;
            public bool visible = true;
            public float opacity = 1;
            public string hAlign = "center";
            public string vAlign = "center";
            public UIValue width = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue height = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue positionX;
            public UIValue positionY;
            public bool isPointerBlocker = true;
        }

        public override string componentName => "UIShape";
        public UIReferencesContainer referencesContainer;
        public RectTransform childHookRectTransform;

        protected Model model = new Model();
        protected UIShape parentUIComponent;

        public Action OnShapeUpdated;

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public override void OnAppliedChanges()
        {
            if (OnShapeUpdated != null)
                OnShapeUpdated.Invoke();
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield break;
        }

        protected T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            GameObject uiGameObject = null;

            if (!string.IsNullOrEmpty(model.parentComponent))
            {
                if (scene.disposableComponents.ContainsKey(model.parentComponent))
                    parentUIComponent = (scene.disposableComponents[model.parentComponent] as UIShape);
                else
                    parentUIComponent = scene.uiScreenSpace as UIShape;
            }
            else
            {
                parentUIComponent = scene.uiScreenSpace as UIShape;
            }

            parentUIComponent.OnChildComponentAttached(this);

            uiGameObject = UnityEngine.Object.Instantiate(Resources.Load(prefabPath), parentUIComponent.childHookRectTransform) as GameObject;
            referencesContainer = uiGameObject.GetComponent<T>();

            referencesContainer.rectTransform.SetToMaxStretch();

            childHookRectTransform = referencesContainer.childHookRectTransform;

            referencesContainer.owner = this;

            return referencesContainer as T;
        }

        protected void ReparentComponent(RectTransform targetTransform, string targetParent)
        {
            bool targetParentExists = targetParent != null && scene.disposableComponents.ContainsKey(targetParent);

            if (parentUIComponent != null && targetParentExists)
            {
                if (parentUIComponent == scene.disposableComponents[targetParent])
                    return;

                parentUIComponent.OnChildComponentDettached(this);
            }

            if (!string.IsNullOrEmpty(targetParent) && targetParentExists)
            {
                parentUIComponent = scene.disposableComponents[targetParent] as UIShape;
            }
            else
            {
                parentUIComponent = scene.uiScreenSpace as UIShape;
            }

            targetTransform.SetParent(parentUIComponent.childHookRectTransform, false);
            parentUIComponent.OnChildComponentAttached(this);
        }

        protected virtual void OnChildComponentAttached(UIShape childComponent)
        {
        }

        protected virtual void OnChildComponentDettached(UIShape childComponent)
        {
        }

        protected IEnumerator ResizeAlignAndReposition(
            RectTransform targetTransform,
            float parentWidth,
            float parentHeight,
            LayoutGroup alignmentLayout,
            LayoutElement alignedLayoutElement)
        {
            // Resize
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, model.width.GetScaledValue(parentWidth));
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, model.height.GetScaledValue(parentHeight));

            targetTransform.ForceUpdateRectTransforms();

            // Alignment (Alignment uses size so we should always align AFTER reisizing)
            alignedLayoutElement.ignoreLayout = false;
            ConfigureAlignment(alignmentLayout);
            LayoutRebuilder.ForceRebuildLayoutImmediate(alignmentLayout.transform as RectTransform);

            // Reposition
            Vector3 position = Vector3.zero;
            position.x = model.positionX.GetScaledValue(parentWidth);
            position.y = model.positionY.GetScaledValue(parentHeight);

            if (position != Vector3.zero)
            {
                alignedLayoutElement.ignoreLayout = true;

                targetTransform.localPosition += position;
            }

            yield break;
        }

        protected void ConfigureAlignment(LayoutGroup layout)
        {
            switch (model.vAlign)
            {
                case "top":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.UpperLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.UpperRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.UpperCenter;
                            break;
                    }
                    break;
                case "bottom":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.LowerLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.LowerRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.LowerCenter;
                            break;
                    }
                    break;
                default: // center
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.MiddleLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.MiddleRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.MiddleCenter;
                            break;
                    }
                    break;
            }
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(childHookRectTransform.gameObject);

            base.Dispose();
        }
    }
}
