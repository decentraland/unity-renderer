using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIImageShape : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public string source;
            public float opacity = 1f;
            public float sourceLeft = 0f;
            public float sourceTop = 0f;
            public float sourceWidth = 1f;
            public float sourceHeight = 1f;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
        }

        public UIImageReferencesContainer referencesContainer;
        public override string componentName => "UIImageShape";

        Model model = new Model();
        bool isLoadingTexture = false;
        string loadedSource = "";

        public UIImageShape(ParcelScene scene) : base(scene)
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
                referencesContainer = InstantiateUIGameObject<UIImageReferencesContainer>("Prefabs/UIImageShape", model);

                // Configure transform reference used by future children ui components
                transform = referencesContainer.imageRectTransform;
            }

            ReparentComponent(referencesContainer.rectTransform, model.parentComponent);

            // Fetch texture
            if (!string.IsNullOrEmpty(model.source))
            {
                if (!isLoadingTexture && model.source != loadedSource)
                {
                    if (scene.sceneData.HasContentsUrl(model.source))
                    {
                        isLoadingTexture = true;

                        yield return Utils.FetchTexture(scene.sceneData.GetContentsUrl(model.source), (Texture downloadedTexture) =>
                        {
                            isLoadingTexture = false;
                            loadedSource = model.source;

                            referencesContainer.image.texture = downloadedTexture;
                        });
                    }
                    else
                    {
                        Debug.Log($"texture {model.source} for {componentName} with id {id} wasn't found in the scene mappings");

                        yield break;
                    }
                }
            }
            else if (referencesContainer.image.texture != null)
            {
                Utils.SafeDestroy(referencesContainer.image.texture);
                yield return null;
            }

            referencesContainer.image.enabled = model.visible;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();
            yield return ResizeAlignAndReposition(referencesContainer.paddingLayoutRectTransform,
                                                model, parentRecTransform.rect.width, parentRecTransform.rect.height,
                                                referencesContainer.alignmentLayoutGroup,
                                                referencesContainer.paddingLayoutElement);

            referencesContainer.image.color = new Color(255f, 255f, 255f, 255f * model.opacity);

            referencesContainer.image.raycastTarget = model.isPointerBlocker;

            // Configure uv rect
            Vector2 normalizedSourceCoordinates = new Vector2(model.sourceLeft / referencesContainer.paddingLayoutRectTransform.rect.width,
                                                             -model.sourceTop / referencesContainer.paddingLayoutRectTransform.rect.height);

            Vector2 normalizedSourceSize = new Vector2(model.sourceWidth * (model.sizeInPixels ? 1f : parentRecTransform.rect.width) / referencesContainer.paddingLayoutRectTransform.rect.width,
                                                        model.sourceHeight * (model.sizeInPixels ? 1f : parentRecTransform.rect.height) / referencesContainer.paddingLayoutRectTransform.rect.height);

            referencesContainer.image.uvRect = new Rect(normalizedSourceCoordinates.x,
                                    normalizedSourceCoordinates.y + (1 - normalizedSourceSize.y),
                                    normalizedSourceSize.x,
                                    normalizedSourceSize.y);

            // Apply padding
            referencesContainer.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            referencesContainer.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            referencesContainer.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            referencesContainer.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRecTransform);
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
