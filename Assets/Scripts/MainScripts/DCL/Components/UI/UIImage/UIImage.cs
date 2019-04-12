using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;
using System.Text.RegularExpressions;
using System;

namespace DCL.Components
{
    public class UIImage : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public string source;
            public float sourceLeft = 0f;
            public float sourceTop = 0f;
            public float sourceWidth = 1f;
            public float sourceHeight = 1f;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public bool sizeInPixels = true;
        }

        public override string componentName => "UIImage";

        new public Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        new public UIImageReferencesContainer referencesContainer
        {
            get { return base.referencesContainer as UIImageReferencesContainer; }
            set { base.referencesContainer = value; }
        }

        public UIImage(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIImageShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<UIImageReferencesContainer>("UIImage");
            }
            else
            {
                ReparentComponent(referencesContainer.rectTransform, model.parentComponent);
            }

            // Fetch texture
            if (!string.IsNullOrEmpty(model.source))
            {
                yield return DCLTexture.FetchFromComponent(scene, model.source, (downloadedTexture) =>
                {
                    referencesContainer.image.texture = downloadedTexture;
                });
            }
            else
            {
                referencesContainer.image.texture = null;
            }

            referencesContainer.image.enabled = model.visible;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            yield return ResizeAlignAndReposition(referencesContainer.paddingLayoutRectTransform,
                                                parentRecTransform.rect.width, parentRecTransform.rect.height,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.paddingLayoutElement);

            referencesContainer.image.color = Color.white;

            if (referencesContainer.image.texture != null)
            {
                // Configure uv rect
                Vector2 normalizedSourceCoordinates = new Vector2(model.sourceLeft / referencesContainer.image.texture.width,
                                                                -model.sourceTop / referencesContainer.image.texture.height);

                Vector2 normalizedSourceSize = new Vector2(model.sourceWidth * (model.sizeInPixels ? 1f : parentRecTransform.rect.width) / referencesContainer.image.texture.width,
                                                            model.sourceHeight * (model.sizeInPixels ? 1f : parentRecTransform.rect.height) / referencesContainer.image.texture.height);

                referencesContainer.image.uvRect = new Rect(normalizedSourceCoordinates.x,
                                        normalizedSourceCoordinates.y + (1 - normalizedSourceSize.y),
                                        normalizedSourceSize.x,
                                        normalizedSourceSize.y);
            }

            // Apply padding
            referencesContainer.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            referencesContainer.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            referencesContainer.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            referencesContainer.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            referencesContainer.canvasGroup.blocksRaycasts = model.isPointerBlocker;
            referencesContainer.canvasGroup.alpha = model.opacity;

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRecTransform);
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
