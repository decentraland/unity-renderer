using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

namespace DCL.Components
{
    public class UIContainerRect : UIShape<UIContainerRectReferencesContainer, UIContainerRect.Model>
    {
        [System.Serializable]
        public new class Model : UIShape.Model
        {
            public float thickness;
            public Color color = Color.clear;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UiContainerRect)
                    return Utils.SafeUnimplemented<UIContainerRect, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiContainerRect, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.UiContainerRect.HasName) pb.name = pbModel.UiContainerRect.Name;
                if (pbModel.UiContainerRect.HasParentComponent) pb.parentComponent = pbModel.UiContainerRect.ParentComponent;
                if (pbModel.UiContainerRect.HasVisible) pb.visible = pbModel.UiContainerRect.Visible;
                if (pbModel.UiContainerRect.HasOpacity) pb.opacity = pbModel.UiContainerRect.Opacity;
                if (pbModel.UiContainerRect.HasHAlign) pb.hAlign = pbModel.UiContainerRect.HAlign;
                if (pbModel.UiContainerRect.HasVAlign) pb.vAlign = pbModel.UiContainerRect.VAlign;
                if (pbModel.UiContainerRect.Width != null) pb.width = SDK6DataMapExtensions.FromProtobuf(pb.width, pbModel.UiContainerRect.Width);
                if (pbModel.UiContainerRect.Height != null) pb.height = SDK6DataMapExtensions.FromProtobuf(pb.height, pbModel.UiContainerRect.Height);
                if (pbModel.UiContainerRect.PositionX != null) pb.positionX = SDK6DataMapExtensions.FromProtobuf(pb.positionX, pbModel.UiContainerRect.PositionX);
                if (pbModel.UiContainerRect.PositionY != null) pb.positionY = SDK6DataMapExtensions.FromProtobuf(pb.positionY, pbModel.UiContainerRect.PositionY);
                if (pbModel.UiContainerRect.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.UiContainerRect.IsPointerBlocker;

                if (pbModel.UiContainerRect.Color != null) pb.color = pbModel.UiContainerRect.Color.AsUnityColor();
                if (pbModel.UiContainerRect.HasThickness) pb.thickness = pbModel.UiContainerRect.Thickness;

                return pb;
            }
        }

        public override string referencesContainerPrefabName => "UIContainerRect";

        public UIContainerRect() { model = new Model(); }

        public override int GetClassId() { return (int) CLASS_ID.UI_CONTAINER_RECT; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);
            referencesContainer.image.raycastTarget = model.color.a >= RAYCAST_ALPHA_THRESHOLD;

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
