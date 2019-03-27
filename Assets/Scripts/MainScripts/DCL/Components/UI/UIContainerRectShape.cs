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
            public float thickness = 0f;
            public Color color = new Color(0f, 0f, 0f, 1f);
        }

        public UIContainerRectReferencesContainer referencesContainer;

        public override string componentName => "UIContainerRectShape";

        Model model;

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
            model = Utils.SafeFromJson<Model>(newJson);

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<UIContainerRectReferencesContainer>("Prefabs/UIContainerRectShape", model);

                // Configure transform reference used by future children ui components
                transform = referencesContainer.imageRectTransform;
            }

            ReparentComponent(referencesContainer.rectTransform, model.parentComponent);

            referencesContainer.image.enabled = model.visible;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();
            yield return ResizeAlignAndReposition(transform, model, parentRecTransform.rect.width, parentRecTransform.rect.height,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.imageLayoutElement);

            referencesContainer.image.color = model.color * 255f;

            Outline outline = referencesContainer.image.GetComponent<Outline>();

            if (model.thickness > 0f)
            {
                if (outline == null)
                {
                    outline = referencesContainer.image.gameObject.AddComponent<Outline>();
                }

                outline.effectDistance = new Vector2(model.thickness, model.thickness);
            }
            else if (outline != null)
            {
                Utils.SafeDestroy(outline);
                yield return null;
            }

            referencesContainer.image.raycastTarget = model.isPointerBlocker;
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
