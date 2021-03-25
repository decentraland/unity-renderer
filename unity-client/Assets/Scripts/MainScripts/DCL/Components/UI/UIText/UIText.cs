using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL.Components
{
    public class UIText : UIShape<UITextReferencesContainer, UIText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;

            public Model()
            {
                textModel = new TextShape.Model();
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                Model model = Utils.SafeFromJson<Model>(json);
                textModel = (TextShape.Model) textModel.GetDataFromJSON(json);
                model.textModel = textModel;
                return model;
            }
        }

        public override string referencesContainerPrefabName => "UIText";

        public UIText()
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_TEXT_SHAPE;
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError("Aborted UITextShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            model = (Model) newModel;

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