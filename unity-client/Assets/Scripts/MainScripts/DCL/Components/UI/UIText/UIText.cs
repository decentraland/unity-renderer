using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class UIText : UIShape<UITextReferencesContainer, UIText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;
        }

        public override string referencesContainerPrefabName => "UIText";

        public UIText(ParcelScene scene) : base(scene)
        {
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_TEXT_SHAPE;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError("Aborted UITextShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (!scene.isTestScene)
            {
                model.textModel = Utils.SafeFromJson<TextShape.Model>(newJson);
            }

            yield return TextShape.ApplyModelChanges(scene, referencesContainer.text, model.textModel);

            RefreshAll();
        }

        protected override void RefreshDCLSize(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            if (model.textModel.adaptWidth || model.textModel.adaptHeight)
                referencesContainer.text.ForceMeshUpdate(false);

            Bounds b = referencesContainer.text.textBounds;

            float width, height;

            if (model.textModel.adaptWidth)
            {
                width = b.size.x;
            }
            else
            {
                width = model.width.GetScaledValue(parentTransform.rect.width);
            }

            if (model.textModel.adaptHeight)
            {
                height = b.size.y;
            }
            else
            {
                height = model.height.GetScaledValue(parentTransform.rect.height);
            }

            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}