using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public class UITransformHandler : IECSComponentHandler<PBUiTransform>
    {
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private readonly int componentId;

        public UITransformHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer, int componentId)
        {
            this.internalUiContainer = internalUiContainer;
            this.componentId = componentId;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                containerModel.components.Remove(componentId);

                // do parent detach only if not child of root entity
                // since ui element without transform should be always attached
                // to the root entity
                if (containerModel.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
                {
                    containerModel.parentElement?.Remove(containerModel.rootElement);
                    containerModel.parentElement = null;
                }
                containerModel.parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
                internalUiContainer.PutFor(scene, entity, containerModel);
            }
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer(entity.entityId);

            containerModel.components.Add(componentId);

            if (containerModel.parentId != model.Parent)
            {
                containerModel.parentId = model.Parent;
                containerModel.parentElement?.Remove(containerModel.rootElement);
                containerModel.parentElement = null;
            }
            containerModel.shouldSort = containerModel.rigthOf != model.RightOf;
            containerModel.rigthOf = model.RightOf;

            VisualElement element = containerModel.rootElement;

            SetUpVisualElement(element, model);
            internalUiContainer.PutFor(scene, entity, containerModel);
        }

        private static void SetUpVisualElement(VisualElement element, PBUiTransform model)
        {
            element.style.display = GetDisplay(model.Display);
            element.style.overflow = GetOverflow(model.Overflow);

            // Flex
            element.style.flexDirection = GetFlexDirection(model.FlexDirection);
            if (model.FlexBasisUnit != YGUnit.YguUndefined)
            {
                element.style.flexBasis = model.FlexBasisUnit == YGUnit.YguAuto ? new StyleLength(StyleKeyword.Auto) : new Length(model.FlexBasis, GetUnit(model.FlexBasisUnit));
            }

            element.style.flexGrow = model.FlexGrow;
            element.style.flexShrink = model.GetFlexShrink();
            element.style.flexWrap = GetWrap(model.GetFlexWrap());

            // Align
            element.style.alignContent = GetAlign(model.GetAlignContent());
            element.style.alignItems = GetAlign(model.GetAlignItems());
            element.style.alignSelf = GetAlign(model.AlignSelf);
            element.style.justifyContent = GetJustify(model.JustifyContent);

            // Layout size
            if (model.HeightUnit != YGUnit.YguUndefined)
            {
                element.style.height = new Length(model.Height, GetUnit(model.HeightUnit));
            }
            if (model.WidthUnit != YGUnit.YguUndefined)
            {
                element.style.width = new Length(model.Width, GetUnit(model.WidthUnit));
            }
            if (model.MaxWidthUnit != YGUnit.YguUndefined)
            {
                element.style.maxWidth = model.MaxWidthUnit == YGUnit.YguAuto ? new StyleLength(StyleKeyword.Auto) : new Length(model.MaxWidth, GetUnit(model.MaxWidthUnit));
            }
            if (model.MaxHeightUnit != YGUnit.YguUndefined)
            {
                element.style.maxHeight = model.MaxHeightUnit == YGUnit.YguAuto ? new StyleLength(StyleKeyword.Auto) : new Length(model.MaxHeight, GetUnit(model.MaxHeightUnit));
            }
            if (model.MinHeightUnit != YGUnit.YguUndefined)
            {
                element.style.minHeight = new Length(model.MinHeight, GetUnit(model.MinHeightUnit));
            }
            if (model.MinWidthUnit != YGUnit.YguUndefined)
            {
                element.style.minWidth = new Length(model.MinWidth, GetUnit(model.MinWidthUnit));
            }

            // Paddings
            if (model.PaddingBottomUnit != YGUnit.YguUndefined)
            {
                element.style.paddingBottom = new Length(model.PaddingBottom, GetUnit(model.PaddingBottomUnit));
            }
            if (model.PaddingLeftUnit != YGUnit.YguUndefined)
            {
                element.style.paddingLeft = new Length(model.PaddingLeft, GetUnit(model.PaddingLeftUnit));
            }
            if (model.PaddingRightUnit != YGUnit.YguUndefined)
            {
                element.style.paddingRight = new Length(model.PaddingRight, GetUnit(model.PaddingRightUnit));
            }
            if (model.PaddingTopUnit != YGUnit.YguUndefined)
            {
                element.style.paddingTop = new Length(model.PaddingTop, GetUnit(model.PaddingTopUnit));
            }

            // Margins
            if (model.MarginLeftUnit != YGUnit.YguUndefined)
            {
                element.style.marginLeft = new Length(model.MarginLeft, GetUnit(model.MarginLeftUnit));
            }
            if (model.MarginRightUnit != YGUnit.YguUndefined)
            {
                element.style.marginRight = new Length(model.MarginRight, GetUnit(model.MarginRightUnit));
            }
            if (model.MarginBottomUnit != YGUnit.YguUndefined)
            {
                element.style.marginBottom = new Length(model.MarginBottom, GetUnit(model.MarginBottomUnit));
            }
            if (model.MarginTopUnit != YGUnit.YguUndefined)
            {
                element.style.marginTop = new Length(model.MarginTop, GetUnit(model.MarginTopUnit));
            }

            // Position
            element.style.position = GetPosition(model.PositionType);

            if (model.PositionTopUnit != YGUnit.YguUndefined)
                element.style.top = new Length(model.PositionTop, GetUnit(model.PositionTopUnit));

            if (model.PositionBottomUnit != YGUnit.YguUndefined)
                element.style.bottom = new Length(model.PositionBottom, GetUnit(model.PositionBottomUnit));

            if (model.PositionRightUnit != YGUnit.YguUndefined)
                element.style.right = new Length(model.PositionRight, GetUnit(model.PositionRightUnit));

            if (model.PositionLeftUnit != YGUnit.YguUndefined)
                element.style.left = new Length(model.PositionLeft, GetUnit(model.PositionLeftUnit));
        }

        private static LengthUnit GetUnit(YGUnit unit)
        {
            switch (unit)
            {
                case YGUnit.YguPercent:
                    return LengthUnit.Percent;
                default:
                    return LengthUnit.Pixel;
            }
        }

        private static StyleEnum<Overflow> GetOverflow(YGOverflow overflow)
        {
            switch (overflow)
            {
                case YGOverflow.YgoHidden:
                    return Overflow.Hidden;
                default:
                    return Overflow.Visible;
            }
        }

        private static StyleEnum<DisplayStyle> GetDisplay(YGDisplay display)
        {
            switch (display)
            {
                case YGDisplay.YgdNone:
                    return DisplayStyle.None;
                default:
                    return DisplayStyle.Flex;
            }
        }

        private static StyleEnum<Justify> GetJustify(YGJustify justify)
        {
            switch (justify)
            {
                case YGJustify.YgjFlexStart:
                    return Justify.FlexStart;
                case YGJustify.YgjCenter:
                    return Justify.Center;
                case YGJustify.YgjFlexEnd:
                    return Justify.FlexEnd;
                case YGJustify.YgjSpaceBetween:
                    return Justify.SpaceBetween;
                case YGJustify.YgjSpaceAround:
                    return Justify.SpaceAround;
                default:
                    return Justify.FlexStart;
            }
        }

        private static StyleEnum<Wrap> GetWrap(YGWrap wrap)
        {
            switch (wrap)
            {
                case YGWrap.YgwNoWrap:
                    return Wrap.NoWrap;
                case YGWrap.YgwWrap:
                    return Wrap.Wrap;
                case YGWrap.YgwWrapReverse:
                    return Wrap.WrapReverse;
                default:
                    return Wrap.Wrap;
            }
        }

        private static StyleEnum<FlexDirection> GetFlexDirection(YGFlexDirection direction)
        {
            switch (direction)
            {
                case YGFlexDirection.YgfdColumn:
                    return FlexDirection.Column;
                case YGFlexDirection.YgfdColumnReverse:
                    return FlexDirection.ColumnReverse;
                case YGFlexDirection.YgfdRow:
                    return FlexDirection.Row;
                case YGFlexDirection.YgfdRowReverse:
                    return FlexDirection.RowReverse;
                default:
                    return FlexDirection.Row;
            }
        }

        private static StyleEnum<UnityEngine.UIElements.Position> GetPosition(YGPositionType positionType)
        {
            switch (positionType)
            {
                case YGPositionType.YgptAbsolute:
                    return UnityEngine.UIElements.Position.Absolute;
                default:
                    return UnityEngine.UIElements.Position.Relative;
            }
        }

        private static StyleEnum<Align> GetAlign(YGAlign align)
        {
            switch (align)
            {
                case YGAlign.YgaAuto:
                    return Align.Auto;
                case YGAlign.YgaFlexStart:
                    return Align.FlexStart;
                case YGAlign.YgaCenter:
                    return Align.Center;
                case YGAlign.YgaFlexEnd:
                    return Align.FlexEnd;
                case YGAlign.YgaStretch:
                    return Align.Stretch;
                default:
                    return Align.Auto;
            }
        }
    }
}
