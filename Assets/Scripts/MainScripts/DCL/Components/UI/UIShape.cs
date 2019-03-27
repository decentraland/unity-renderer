using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIShape : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string id;
            public string parentComponent;
            public bool visible = true;
            public string hAlign = "center";
            public string vAlign = "center";
            public bool sizeInPixels = true; // If false, the size is 0~1 based on the parent size
            public float width = 100f;
            public float height = 100f;
            public bool positionInPixels = true;
            public Vector2 position; // If false, the position is 0~1 based on the parent size
            public bool isPointerBlocker = false;
        }

        public override string componentName => "UIShape";
        public RectTransform transform;

        Model model = new Model();

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield break;
        }

        protected T InstantiateUIGameObject<T>(string prefabPath, Model model) where T : UIReferencesContainer
        {
            GameObject uiGameObject = GameObject.Instantiate(Resources.Load(prefabPath)) as GameObject;
            T referencesContainer = uiGameObject.GetComponent<T>();

            referencesContainer.name = componentName + " - " + id;

            return referencesContainer;
        }

        protected void ReparentComponent(RectTransform targetTransform, string targetParent)
        {
            if (!string.IsNullOrEmpty(targetParent))
                targetTransform.SetParent((scene.disposableComponents[targetParent] as UIShape).transform);
            else
                targetTransform.SetParent(scene.uiScreenSpaceCanvas.transform);

            targetTransform.ResetLocalTRS();
            targetTransform.sizeDelta = Vector2.zero;
            targetTransform.ForceUpdateRectTransforms();
        }

        protected IEnumerator ResizeAlignAndReposition(
            RectTransform targetTransform,
            Model model,
            float parentWidth,
            float parentHeight,
            LayoutGroup alignmentLayout,
            LayoutElement alignedLayoutElement)
        {
            // Resize
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, model.width * (model.sizeInPixels ? 1f : parentWidth));
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, model.height * (model.sizeInPixels ? 1f : parentHeight));
            targetTransform.ForceUpdateRectTransforms();

            // Alignment (Alignment uses size so we should always align AFTER reisizing)
            alignedLayoutElement.ignoreLayout = false;
            ConfigureAlignment(alignmentLayout, model);

            LayoutRebuilder.ForceRebuildLayoutImmediate(alignmentLayout.transform as RectTransform);

            alignedLayoutElement.ignoreLayout = true;

            // Reposition
            targetTransform.localPosition += new Vector3(model.position.x * (model.positionInPixels ? 1f : parentWidth), model.position.y * (model.positionInPixels ? 1f : parentHeight), 0f);
            yield break;
        }

        protected void ConfigureAlignment(LayoutGroup layout, Model model)
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
            Utils.SafeDestroy(transform.gameObject);

            base.Dispose();
        }
    }
}
