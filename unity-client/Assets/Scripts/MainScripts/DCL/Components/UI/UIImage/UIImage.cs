using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
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
        }

        public override string referencesContainerPrefabName => "UIImage";

        DCLTexture dclTexture = null;

        public UIImage(ParcelScene scene) : base(scene)
        {
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError("Aborted UIImageShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        Coroutine fetchRoutine;

        public override IEnumerator ApplyChanges(string newJson)
        {
            // Fetch texture
            if (!string.IsNullOrEmpty(model.source))
            {
                if (dclTexture == null || (dclTexture != null && dclTexture.id != model.source))
                {
                    if (fetchRoutine != null)
                    {
                        scene.StopCoroutine(fetchRoutine);
                        fetchRoutine = null;
                    }

                    IEnumerator fetchIEnum = DCLTexture.FetchTextureComponent(scene, model.source, (downloadedTexture) =>
                    {
                        referencesContainer.image.texture = downloadedTexture.texture;
                        fetchRoutine = null;
                        dclTexture?.DetachFrom(this);
                        dclTexture = downloadedTexture;
                        dclTexture.AttachTo(this);
                    });

                    fetchRoutine = scene.StartCoroutine(fetchIEnum);
                }
            }
            else
            {
                referencesContainer.image.texture = null;
                dclTexture?.DetachFrom(this);
                dclTexture = null;
            }

            referencesContainer.image.enabled = model.visible;
            referencesContainer.image.color = Color.white;

            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            if (referencesContainer.image.texture != null)
            {
                // Configure uv rect
                Vector2 normalizedSourceCoordinates = new Vector2(
                    model.sourceLeft / referencesContainer.image.texture.width,
                    -model.sourceTop / referencesContainer.image.texture.height);


                Vector2 normalizedSourceSize = new Vector2(
                    model.sourceWidth * (model.sizeInPixels ? 1f : parentRecTransform.rect.width) /
                    referencesContainer.image.texture.width,
                    model.sourceHeight * (model.sizeInPixels ? 1f : parentRecTransform.rect.height) /
                    referencesContainer.image.texture.height);

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

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRecTransform);
            return null;
        }

        public override void Dispose()
        {
            dclTexture?.DetachFrom(this);
            dclTexture = null;

            Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}
