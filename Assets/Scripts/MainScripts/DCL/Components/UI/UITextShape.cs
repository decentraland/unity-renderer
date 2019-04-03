using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;
using TMPro;

namespace DCL.Components.UI
{
    public class TextShape : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public Components.TextShape.Model textModel;
        }

        public UITextReferencesContainer referencesContainer;
        public override string componentName => "UITextShape";

        new public Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        public TextShape(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UITextShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = JsonUtility.FromJson<Model>(newJson);

            if (!scene.isTestScene)
                model.textModel = JsonUtility.FromJson<Components.TextShape.Model>(newJson);

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<UITextReferencesContainer>("Prefabs/UITextShape");

                // Configure transform reference used by future children ui components
                transform = referencesContainer.textRectTransform;
            }

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();
            float parentWidth = parentRecTransform.rect.width - parentRecTransform.sizeDelta.x;
            float parentHeight = parentRecTransform.rect.height - parentRecTransform.sizeDelta.y;

            yield return ResizeAlignAndReposition(transform, parentWidth, parentHeight,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.alignedLayoutElement);

            DCL.Components.TextShape.ApplyModelChanges(referencesContainer.text, model.textModel);

            referencesContainer.text.raycastTarget = model.isPointerBlocker;
            referencesContainer.text.enabled = model.visible;

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRecTransform);
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
