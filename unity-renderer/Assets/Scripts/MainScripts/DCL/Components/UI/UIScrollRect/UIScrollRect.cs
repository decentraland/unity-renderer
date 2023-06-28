using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

namespace DCL.Components
{
    public class UIScrollRect : UIShape<UIScrollRectRefContainer, UIScrollRect.Model>
    {
        [System.Serializable]
        public new class Model : UIShape.Model
        {
            public float valueX = 0;
            public float valueY = 0;
            public Color backgroundColor = Color.clear;
            public bool isHorizontal = false;
            public bool isVertical = true;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public string OnChanged;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UiScrollRect)
                    return Utils.SafeUnimplemented<UIScrollRect, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiScrollRect, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.UiScrollRect.HasName) pb.name = pbModel.UiScrollRect.Name;
                if (pbModel.UiScrollRect.HasParentComponent) pb.parentComponent = pbModel.UiScrollRect.ParentComponent;
                if (pbModel.UiScrollRect.HasVisible) pb.visible = pbModel.UiScrollRect.Visible;
                if (pbModel.UiScrollRect.HasOpacity) pb.opacity = pbModel.UiScrollRect.Opacity;
                if (pbModel.UiScrollRect.HasHAlign) pb.hAlign = pbModel.UiScrollRect.HAlign;
                if (pbModel.UiScrollRect.HasVAlign) pb.vAlign = pbModel.UiScrollRect.VAlign;
                if (pbModel.UiScrollRect.Width != null) pb.width = SDK6DataMapExtensions.FromProtobuf(pb.width, pbModel.UiScrollRect.Width);
                if (pbModel.UiScrollRect.Height != null) pb.height = SDK6DataMapExtensions.FromProtobuf(pb.height, pbModel.UiScrollRect.Height);
                if (pbModel.UiScrollRect.PositionX != null) pb.positionX = SDK6DataMapExtensions.FromProtobuf(pb.positionX, pbModel.UiScrollRect.PositionX);
                if (pbModel.UiScrollRect.PositionY != null) pb.positionY = SDK6DataMapExtensions.FromProtobuf(pb.positionY, pbModel.UiScrollRect.PositionY);
                if (pbModel.UiScrollRect.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.UiScrollRect.IsPointerBlocker;

                if (pbModel.UiScrollRect.HasValueX) pb.valueX = pbModel.UiScrollRect.ValueX;
                if (pbModel.UiScrollRect.HasValueY) pb.valueY = pbModel.UiScrollRect.ValueY;
                if (pbModel.UiScrollRect.BackgroundColor != null) pb.backgroundColor = pbModel.UiScrollRect.BackgroundColor.AsUnityColor();
                if (pbModel.UiScrollRect.IsHorizontal) pb.isHorizontal = pbModel.UiScrollRect.IsHorizontal;
                if (pbModel.UiScrollRect.IsVertical) pb.isVertical = pbModel.UiScrollRect.IsVertical;
                if (pbModel.UiScrollRect.HasPaddingTop) pb.paddingTop = pbModel.UiScrollRect.PaddingTop;
                if (pbModel.UiScrollRect.HasPaddingRight) pb.paddingRight = pbModel.UiScrollRect.PaddingRight;
                if (pbModel.UiScrollRect.HasPaddingBottom) pb.paddingBottom = pbModel.UiScrollRect.PaddingBottom;
                if (pbModel.UiScrollRect.HasPaddingLeft) pb.paddingLeft = pbModel.UiScrollRect.PaddingLeft;
                if (pbModel.UiScrollRect.HasOnChanged) pb.OnChanged = pbModel.UiScrollRect.OnChanged;

                return pb;
            }
        }

        public UIScrollRect(UIShapePool pool) : base(pool)
        {
            this.pool = pool;
            model = new Model();
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScrollRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override void OnChildAttached(UIShape parent, UIShape childComponent)
        {
            base.OnChildAttached(parent, childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            childComponent.OnAppliedChanges += RefreshContainerForShape;
            RefreshContainerForShape(childComponent);
        }

        protected override void OnChildDetached(UIShape parent, UIShape childComponent)
        {
            base.OnChildDetached(parent, childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
        }

        void RefreshContainerForShape(BaseDisposable updatedComponent)
        {
            MarkLayoutDirty( () =>
                {
                    referencesContainer.fitter.RefreshRecursively();
                    AdjustChildHook();
                    referencesContainer.scrollRect.Rebuild(CanvasUpdate.MaxUpdateValue);
                }
            );
        }

        void AdjustChildHook()
        {
            UIScrollRectRefContainer rc = referencesContainer;
            rc.childHookRectTransform.SetParent(rc.layoutElementRT, false);
            rc.childHookRectTransform.SetToMaxStretch();
            rc.childHookRectTransform.SetParent(rc.content, true);
            RefreshDCLLayoutRecursively(false, true);
        }

        protected override void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            base.RefreshDCLLayoutRecursively(refreshSize, refreshAlignmentAndPosition);
            referencesContainer.fitter.RefreshRecursively();
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            UIScrollRectRefContainer rc = referencesContainer;

            rc.contentBackground.color = model.backgroundColor;

            // Apply padding
            rc.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            rc.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            rc.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            rc.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            rc.scrollRect.horizontal = model.isHorizontal;
            rc.scrollRect.vertical = model.isVertical;

            rc.HScrollbar.value = model.valueX;
            rc.VScrollbar.value = model.valueY;

            rc.scrollRect.onValueChanged.AddListener(OnChanged);

            MarkLayoutDirty( () =>
                {
                    referencesContainer.fitter.RefreshRecursively();
                    AdjustChildHook();
                }
            );
            return null;
        }

        void OnChanged(Vector2 scrollingValues) { WebInterface.ReportOnScrollChange(scene.sceneData.sceneNumber, model.OnChanged, scrollingValues, 0); }

        public override void Dispose()
        {
            if (referencesContainer != null)
                referencesContainer.scrollRect.onValueChanged.RemoveAllListeners();

            base.Dispose();
        }
    }
}
