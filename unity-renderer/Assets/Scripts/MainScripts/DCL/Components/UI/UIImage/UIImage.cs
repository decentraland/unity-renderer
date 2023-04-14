using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIImage : UIShape<UIImageReferencesContainer, UIImage.Model>
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

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
        }

        public override string referencesContainerPrefabName => "UIImage";

        DCLTexture dclTexture = null;
        private readonly DCLTexture.Fetcher dclTextureFetcher = new DCLTexture.Fetcher();
        private bool isDisposed;

        public UIImage()
        {
            model = new Model();
        }

        public override int GetClassId() { return (int) CLASS_ID.UI_IMAGE_SHAPE; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null) { Debug.LogError("Aborted UIImageShape attachment to an entity. UIShapes shouldn't be attached to entities."); }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            // Fetch texture
            if (!string.IsNullOrEmpty(model.source))
            {
                if (dclTexture == null || (dclTexture != null && dclTexture.id != model.source))
                {
                    dclTextureFetcher.Fetch(scene, model.source,
                                          fetchedDCLTexture =>
                                          {
                                              if (isDisposed)
                                                  return false;

                                              referencesContainer.image.texture = fetchedDCLTexture.texture;
                                              dclTexture?.DetachFrom(this);
                                              dclTexture = fetchedDCLTexture;
                                              dclTexture.AttachTo(this);

                                              ConfigureImage();
                                              return true;
                                          })
                                     .Forget();

                    return null;
                }
            }
            else
            {
                referencesContainer.image.texture = null;
                dclTexture?.DetachFrom(this);
                dclTexture = null;
            }

            ConfigureImage();
            return null;
        }

        private void ConfigureImage()
        {
            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            ConfigureUVRect(parentRecTransform, dclTexture?.resizingFactor ?? 1);

            referencesContainer.image.enabled = model.visible;
            referencesContainer.image.color = Color.white;

            ConfigureUVRect(parentRecTransform, dclTexture?.resizingFactor ?? 1);

            // Apply padding
            referencesContainer.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            referencesContainer.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            referencesContainer.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            referencesContainer.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            Utils.ForceRebuildLayoutImmediate(parentRecTransform);
        }

        private void ConfigureUVRect(RectTransform parentRecTransform, float resizingFactor)
        {
            if (referencesContainer.image.texture == null)
                return;

            // Configure uv rect
            Vector2 normalizedSourceCoordinates = new Vector2(
                model.sourceLeft * resizingFactor / referencesContainer.image.texture.width,
                -model.sourceTop * resizingFactor / referencesContainer.image.texture.height);

            Vector2 normalizedSourceSize = new Vector2(
                model.sourceWidth * resizingFactor * (model.sizeInPixels ? 1f : parentRecTransform.rect.width) /
                referencesContainer.image.texture.width ,
                model.sourceHeight * resizingFactor * (model.sizeInPixels ? 1f : parentRecTransform.rect.height) /
                referencesContainer.image.texture.height);

            referencesContainer.image.uvRect = new Rect(normalizedSourceCoordinates.x,
                normalizedSourceCoordinates.y + (1 - normalizedSourceSize.y),
                normalizedSourceSize.x,
                normalizedSourceSize.y);
        }

        public override void Dispose()
        {
            isDisposed = true;

            dclTextureFetcher.Dispose();

            dclTexture?.DetachFrom(this);

            if (referencesContainer != null)
            {
                referencesContainer.image.texture = null;
                Utils.SafeDestroy(referencesContainer.gameObject);
                referencesContainer = null;
            }

            base.Dispose();
        }
    }
}
