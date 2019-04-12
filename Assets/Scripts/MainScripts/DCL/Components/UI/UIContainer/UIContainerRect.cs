using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIContainerRect : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float thickness = 0f;
            public Color color = new Color(0f, 0f, 0f, 1f);
            public bool adaptWidth = false;
            public bool adaptHeight = false;
        }

        public override string componentName => "UIContainerRect";

        new Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        new public UIContainerRectReferencesContainer referencesContainer
        {
            get { return base.referencesContainer as UIContainerRectReferencesContainer; }
            set { base.referencesContainer = value; }
        }

        public UIContainerRect(ParcelScene scene) : base(scene)
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
                referencesContainer = InstantiateUIGameObject<UIContainerRectReferencesContainer>("UIContainerRect");
            }
            else
            {
                ReparentComponent(referencesContainer.rectTransform, model.parentComponent);
            }

            referencesContainer.image.enabled = model.visible;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            yield return ResizeAlignAndReposition(childHookRectTransform,
                                                parentRecTransform.rect.width,
                                                parentRecTransform.rect.height,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.imageLayoutElement);

            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);

            Outline outline = referencesContainer.image.GetComponent<Outline>();

            if (model.thickness > 0f)
            {
                if (outline == null)
                    outline = referencesContainer.image.gameObject.AddComponent<Outline>();

                outline.effectDistance = new Vector2(model.thickness, model.thickness);
            }
            else if (outline != null)
            {
                Utils.SafeDestroy(outline);
                yield return null;
            }

            referencesContainer.canvasGroup.alpha = model.opacity;
            referencesContainer.canvasGroup.blocksRaycasts = model.isPointerBlocker;
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
