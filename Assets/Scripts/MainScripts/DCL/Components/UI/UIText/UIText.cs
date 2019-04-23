using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;
using TMPro;

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

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UITextShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (!scene.isTestScene)
                model.textModel = JsonUtility.FromJson<TextShape.Model>(newJson);

            TextShape.ApplyModelChanges(referencesContainer.text, model.textModel);
            yield break;
        }

        public override void RefreshDCLLayout(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            if (!model.textModel.resizeToFit)
            {
                base.RefreshDCLLayout(refreshSize, refreshAlignmentAndPosition);
            }
            else
            {
                if (refreshSize)
                {
                    referencesContainer.text.Rebuild(CanvasUpdate.LatePreRender);
                    referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, referencesContainer.text.renderedWidth);
                    referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, referencesContainer.text.renderedHeight);
                    referencesContainer.layoutElementRT.ForceUpdateRectTransforms();
                }

                if (refreshAlignmentAndPosition)
                {
                    RefreshDCLAlignmentAndPosition();
                }
            }
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);
            base.Dispose();
        }
    }
}
