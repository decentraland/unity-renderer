using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIContainerRectShape : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float thickness = 0;
            public Color color = new Color(0f, 0f, 0f, 255f);
            public bool isPointerBlocker = false;
            public float width = 1f; // 0~1 size based on the parent size (unless sizeInPixels)
            public float height = 1f; // 0~1 size based on the parent size (unless sizeInPixels)
            public Vector2 position = Vector2.zero; // 0~1 position based on the parent size
            public bool sizeInPixels = false;
            public string hAlign = "center";
            public string vAlign = "center";
            public bool alignmentUsesSize = true;
        }

        public override string componentName => "UIContainerRectShape";

        Model model = new Model();
        Image image = null;

        public UIContainerRectShape(ParcelScene scene) : base(scene)
        {
        } 

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            // Unity takes 2 frames to update the canvas info (LayoutRebuilder.ForceRebuildLayoutImmediate() and Canvas.ForceUpdateCanvases() don't seem to work)
            yield return null;
            yield return null;

            model = Utils.SafeFromJson<Model>(newJson);

            if (image == null)
            {
                GameObject imageGameObject = new GameObject(componentName + " - " + model.id);
                image = imageGameObject.AddComponent<Image>();

                transform = imageGameObject.GetComponent<RectTransform>();

                if (!string.IsNullOrEmpty(model.parentComponent))
                {
                    transform.SetParent((scene.disposableComponents[model.parentComponent] as UIShape).transform);
                }
                else
                {
                    transform.SetParent(scene.uiScreenSpaceCanvas.transform);
                }
            }

            transform.ResetLocalTRS();

            RectTransform parentRecTransform = transform.GetComponentInParent<RectTransform>();

            image.enabled = model.visible;

            // We put the anchors on the parent edges/corners to get the parent's correct size
            transform.anchorMin = Vector3.zero;
            transform.anchorMax = Vector3.one;

            float parentWidth = parentRecTransform.rect.width - parentRecTransform.sizeDelta.x;
            float parentHeight = parentRecTransform.rect.height - parentRecTransform.sizeDelta.y;

            ConfigureAnchorBasedOnAlignment(parentWidth, parentHeight);

            // Resize
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, model.width * (!model.sizeInPixels ? parentWidth : 1f));
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, model.height * (!model.sizeInPixels ? parentHeight : 1f));
            transform.ForceUpdateRectTransforms();

            // Reposition
            transform.localPosition += new Vector3(model.position.x * parentWidth, model.position.y * parentHeight, 0f);

            image.color = model.color;

            Outline outline = image.GetComponent<Outline>();
            if (model.thickness > 0f)
            {
                if (outline == null)
                    outline = image.gameObject.AddComponent<Outline>();

                outline.effectDistance = new Vector2(model.thickness, model.thickness);
            }
            else if (outline != null)
            {
                GameObject.Destroy(outline);
            }

            image.raycastTarget = model.isPointerBlocker;
        }

        void ConfigureAnchorBasedOnAlignment(float parentWidth, float parentHeight)
        {
            Vector2 auxVector = Vector2.zero;

            float normalizedWidth;
            float normalizedHeight;

            if (model.sizeInPixels)
            {
                normalizedWidth = model.width / parentWidth;
                normalizedHeight = model.height / parentHeight;
            }
            else
            {
                normalizedWidth = model.width;
                normalizedHeight = model.height;
            }

            switch (model.hAlign)
            {
                case "left":
                    auxVector.x = 0f + (model.alignmentUsesSize ? normalizedWidth / 2 : 0f);
                    break;

                case "right":
                    auxVector.x = 1f - (model.alignmentUsesSize ? normalizedWidth / 2 : 0f);
                    break;

                default: // "center"
                    auxVector.x = 0.5f;
                    break;
            }

            switch (model.vAlign)
            {
                case "top":
                    auxVector.y = 1f - (model.alignmentUsesSize ? normalizedHeight / 2 : 0f);
                    break;

                case "bottom":
                    auxVector.y = 0f + (model.alignmentUsesSize ? normalizedHeight / 2 : 0f);
                    break;

                default: // "center"
                    auxVector.y = 0.5f;
                    break;
            }

            transform.anchorMin = auxVector;
            transform.anchorMax = transform.anchorMin;
        }
    }
}
