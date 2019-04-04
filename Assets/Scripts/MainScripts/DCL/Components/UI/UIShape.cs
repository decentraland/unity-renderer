using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

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
            public string hAlign = "center";
            public string vAlign = "center";
            public UIValue width = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue height = new UIValue(100, UIValue.Unit.PIXELS);
            public UIValue positionX; 
            public UIValue positionY; 
            public bool isPointerBlocker;
        }

        public override string componentName => "UIShape";
        public RectTransform childHookRectTransform;

        protected Model model = new Model();

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield break;
        }

        protected T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            Transform parent = null;

            if (!string.IsNullOrEmpty(model.parentComponent))
                parent = (scene.disposableComponents[model.parentComponent] as UIShape).childHookRectTransform;
            else
                parent = scene.uiScreenSpaceCanvas.transform;

            GameObject uiGameObject = Object.Instantiate(Resources.Load(prefabPath), parent) as GameObject;
            T referencesContainer = uiGameObject.GetComponentInChildren<T>();

            RectTransform rootRT = uiGameObject.GetComponent<RectTransform>();
            rootRT.SetToMaxStretch();

            childHookRectTransform = referencesContainer.rectTransform;
            return referencesContainer;
        }

        protected void ReparentComponent(RectTransform targetTransform, string targetParent)
        {
            if (!string.IsNullOrEmpty(targetParent))
                targetTransform.SetParent((scene.disposableComponents[targetParent] as UIShape).childHookRectTransform, false);
            else
                targetTransform.SetParent(scene.uiScreenSpaceCanvas.transform, false);
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
            alignedLayoutElement.ignoreLayout = true;

            // Reposition
            Vector3 position = Vector3.zero;
            position.x = model.positionX.GetScaledValue(parentWidth);
            position.y = model.positionY.GetScaledValue(parentHeight);

            targetTransform.localPosition += position;
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
