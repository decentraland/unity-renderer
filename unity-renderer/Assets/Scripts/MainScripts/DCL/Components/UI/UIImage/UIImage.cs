using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

namespace DCL.Components
{
    public class UIImage : UIShape<UIImageReferencesContainer, UIImage.Model>
    {
        [System.Serializable]
        public new class Model : UIShape.Model
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

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UiImage)
                    return Utils.SafeUnimplemented<UIImage, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiImage, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.UiImage.HasName) pb.name = pbModel.UiImage.Name;
                if (pbModel.UiImage.HasParentComponent) pb.parentComponent = pbModel.UiImage.ParentComponent;
                if (pbModel.UiImage.HasVisible) pb.visible = pbModel.UiImage.Visible;
                if (pbModel.UiImage.HasOpacity) pb.opacity = pbModel.UiImage.Opacity;
                if (pbModel.UiImage.HasHAlign) pb.hAlign = pbModel.UiImage.HAlign;
                if (pbModel.UiImage.HasVAlign) pb.vAlign = pbModel.UiImage.VAlign;
                if (pbModel.UiImage.Width != null) pb.width = SDK6DataMapExtensions.FromProtobuf(pb.width, pbModel.UiImage.Width);
                if (pbModel.UiImage.Height != null) pb.height = SDK6DataMapExtensions.FromProtobuf(pb.height, pbModel.UiImage.Height);
                if (pbModel.UiImage.PositionX != null) pb.positionX = SDK6DataMapExtensions.FromProtobuf(pb.positionX, pbModel.UiImage.PositionX);
                if (pbModel.UiImage.PositionY != null) pb.positionY = SDK6DataMapExtensions.FromProtobuf(pb.positionY, pbModel.UiImage.PositionY);
                if (pbModel.UiImage.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.UiImage.IsPointerBlocker;

                if (pbModel.UiImage.HasSource) pb.source = pbModel.UiImage.Source;
                if (pbModel.UiImage.HasSourceLeft) pb.sourceLeft = pbModel.UiImage.SourceLeft;
                if (pbModel.UiImage.HasSourceTop) pb.sourceTop = pbModel.UiImage.SourceTop;
                if (pbModel.UiImage.HasSourceWidth) pb.sourceWidth = pbModel.UiImage.SourceWidth;
                if (pbModel.UiImage.HasSourceHeight) pb.sourceHeight = pbModel.UiImage.SourceHeight;
                if (pbModel.UiImage.HasPaddingTop) pb.paddingTop = pbModel.UiImage.PaddingTop;
                if (pbModel.UiImage.HasPaddingRight) pb.paddingRight = pbModel.UiImage.PaddingRight;
                if (pbModel.UiImage.HasPaddingBottom) pb.paddingBottom = pbModel.UiImage.PaddingBottom;
                if (pbModel.UiImage.HasPaddingLeft) pb.paddingLeft = pbModel.UiImage.PaddingLeft;
                if (pbModel.UiImage.HasSizeInPixels) pb.sizeInPixels = pbModel.UiImage.SizeInPixels;

                if (pbModel.UiImage.HasOnClick) pb.onClick = pbModel.UiImage.OnClick;


                return pb;
            }
        }

        DCLTexture dclTexture = null;
        private readonly DCLTexture.Fetcher dclTextureFetcher = new ();
        private bool isDisposed;

        public UIImage(UIShapePool pool) : base(pool)
        {
            this.pool = pool;
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.UI_IMAGE_SHAPE;

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null) =>
            Debug.LogError("Aborted UIImageShape attachment to an entity. UIShapes shouldn't be attached to entities.");

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
                referencesContainer = null;
            }

            base.Dispose();
        }
    }
}
