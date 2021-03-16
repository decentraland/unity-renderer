using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIContainerRect : UIShape<UIContainerRectReferencesContainer, UIContainerRect.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float thickness = 0f;
            public Color color = Color.clear;
            public bool adaptWidth = false;
            public bool adaptHeight = false;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public override string referencesContainerPrefabName => "UIContainerRect";

        public UIContainerRect()
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_CONTAINER_RECT;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);

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
                Object.DestroyImmediate(outline, false);
            }

            return null;
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}