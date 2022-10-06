using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.ECSComponents
{
    public class UITransformHandler : IECSComponentHandler<PBUiTransform>
    {
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private bool debugRandomColorSet = false;

        public UITransformHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer)
        {
            this.internalUiContainer = internalUiContainer;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            debugRandomColorSet = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                containerModel.hasTransform = false;

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
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();

            containerModel.hasTransform = true;

            if (containerModel.parentId != model.Parent)
            {
                containerModel.parentId = model.Parent;
                containerModel.parentElement?.Remove(containerModel.rootElement);
                containerModel.parentElement = null;
            }

            VisualElement element = containerModel.rootElement;

            if (!debugRandomColorSet)
            {
                element.style.backgroundColor = Random.ColorHSV(); // temp for debugging
                debugRandomColorSet = true;
            }

            SetUpVisualElement(element, model);
            internalUiContainer.PutFor(scene, entity, containerModel);
        }

        private static void SetUpVisualElement(VisualElement element, PBUiTransform model)
        {
            element.style.display = GetDisplay(model.Display);
            element.style.overflow = GetOverflow(model.Overflow);

            // Flex
            element.style.flexDirection = GetFlexDirection(model.FlexDirection);
            if (!float.IsNaN(model.FlexBasis))
                element.style.flexBasis = new Length(model.FlexBasis, GetUnit(model.FlexBasisUnit));
            else
                element.style.flexBasis = new StyleLength(StyleKeyword.Auto);

            element.style.flexGrow = model.FlexGrow;
            element.style.flexShrink = model.FlexShrink;
            element.style.flexWrap = GetWrap(model.FlexWrap);

            // Align 
            if (model.AlignContent != YGAlign.FlexStart)
                element.style.alignContent = GetAlign(model.AlignContent);
            if (model.AlignItems != YGAlign.Auto)
                element.style.alignItems = GetAlign(model.AlignItems);
            if (model.AlignSelf != YGAlign.Auto)
                element.style.alignSelf = GetAlign(model.AlignSelf);
            element.style.justifyContent = GetJustify(model.JustifyContent);

            // Layout size
            if (!float.IsNaN(model.Height))
                element.style.height = new Length(model.Height, GetUnit(model.HeightUnit));
            if (!float.IsNaN(model.Width))
                element.style.width = new Length(model.Width, GetUnit(model.WidthUnit));

            if (!float.IsNaN(model.MaxWidth))
                element.style.maxWidth = new Length(model.MaxWidth, GetUnit(model.MaxWidthUnit));
            else
                element.style.maxWidth = new StyleLength(StyleKeyword.Auto);
            if (!float.IsNaN(model.MaxHeight))
                element.style.maxHeight = new Length(model.MaxHeight, GetUnit(model.MaxHeightUnit));
            else
                element.style.maxHeight = new StyleLength(StyleKeyword.Auto);

            if (!float.IsNaN(model.MinHeight))
                element.style.minHeight = new Length(model.MinHeight, GetUnit(model.MinHeightUnit));
            if (!float.IsNaN(model.MinWidth))
                element.style.minWidth = new Length(model.MinWidth, GetUnit(model.MinWidthUnit));

            // Paddings
            if (!Mathf.Approximately(model.PaddingBottom, 0))
                element.style.paddingBottom = new Length(model.PaddingBottom, GetUnit(model.PaddingBottomUnit));
            if (!Mathf.Approximately(model.PaddingLeft, 0))
                element.style.paddingLeft = new Length(model.PaddingLeft, GetUnit(model.PaddingLeftUnit));
            if (!Mathf.Approximately(model.PaddingRight, 0))
                element.style.paddingRight = new Length(model.PaddingRight, GetUnit(model.PaddingRightUnit));
            if (!Mathf.Approximately(model.PaddingTop, 0))
                element.style.paddingTop = new Length(model.PaddingTop, GetUnit(model.PaddingTopUnit));

            // Margins
            if (!Mathf.Approximately(model.MarginLeft, 0))
                element.style.marginLeft = new Length(model.MarginLeft, GetUnit(model.MarginLeftUnit));
            if (!Mathf.Approximately(model.MarginRight, 0))
                element.style.marginRight = new Length(model.MarginRight, GetUnit(model.MarginRightUnit));
            if (!Mathf.Approximately(model.MarginBottom, 0))
                element.style.marginBottom = new Length(model.MarginBottom, GetUnit(model.MarginBottomUnit));
            if (!Mathf.Approximately(model.MarginTop, 0))
                element.style.marginTop = new Length(model.MarginTop, GetUnit(model.MarginTopUnit));

            // Borders
            element.style.borderBottomWidth = model.BorderBottom;
            element.style.borderLeftWidth = model.BorderLeft;
            element.style.borderRightWidth = model.BorderRight;
            element.style.borderTopWidth = model.BorderTop;

            // Position
            element.style.position = GetPosition(model.PositionType);
        }

        private static LengthUnit GetUnit(YGUnit unit)
        {
            switch (unit)
            {
                case YGUnit.Point:
                    return LengthUnit.Pixel;
                case YGUnit.Percent:
                    return LengthUnit.Percent;
                default:
                    return LengthUnit.Pixel;
            }
        }

        private static StyleEnum<Overflow> GetOverflow(YGOverflow overflow)
        {
            switch (overflow)
            {
                case YGOverflow.Visible:
                    return Overflow.Visible;
                case YGOverflow.Hidden:
                    return Overflow.Hidden;
                default:
                    return Overflow.Visible;
            }
        }

        private static StyleEnum<DisplayStyle> GetDisplay(YGDisplay display)
        {
            switch (display)
            {
                case YGDisplay.Flex:
                    return DisplayStyle.Flex;
                case YGDisplay.None:
                    return DisplayStyle.None;
                default:
                    return DisplayStyle.Flex;
            }
        }

        private static StyleEnum<Justify> GetJustify(YGJustify justify)
        {
            switch (justify)
            {
                case YGJustify.FlexStart:
                    return Justify.FlexStart;
                case YGJustify.Center:
                    return Justify.Center;
                case YGJustify.FlexEnd:
                    return Justify.FlexEnd;
                case YGJustify.SpaceBetween:
                    return Justify.SpaceBetween;
                case YGJustify.SpaceAround:
                    return Justify.SpaceAround;
                default:
                    return Justify.FlexStart;
            }
        }

        private static StyleEnum<Wrap> GetWrap(YGWrap wrap)
        {
            switch (wrap)
            {
                case YGWrap.NoWrap:
                    return Wrap.NoWrap;
                case YGWrap.Wrap:
                    return Wrap.Wrap;
                case YGWrap.WrapReverse:
                    return Wrap.WrapReverse;
                default:
                    return Wrap.Wrap;
            }
        }

        private static StyleEnum<FlexDirection> GetFlexDirection(YGFlexDirection direction)
        {
            switch (direction)
            {
                case YGFlexDirection.Column:
                    return FlexDirection.Column;
                case YGFlexDirection.ColumnReverse:
                    return FlexDirection.ColumnReverse;
                case YGFlexDirection.Row:
                    return FlexDirection.Row;
                case YGFlexDirection.RowReverse:
                    return FlexDirection.RowReverse;
                default:
                    return FlexDirection.Row;
            }
        }

        private static StyleEnum<Position> GetPosition(YGPositionType positionType)
        {
            switch (positionType)
            {
                case YGPositionType.Relative:
                    return Position.Relative;
                case YGPositionType.Absolute:
                    return Position.Absolute;
                default:
                    return Position.Relative;
            }
        }

        private static StyleEnum<Align> GetAlign(YGAlign align)
        {
            switch (align)
            {
                case YGAlign.Auto:
                    return Align.Auto;
                case YGAlign.FlexStart:
                    return Align.FlexStart;
                case YGAlign.Center:
                    return Align.Center;
                case YGAlign.FlexEnd:
                    return Align.FlexEnd;
                case YGAlign.Stretch:
                    return Align.Stretch;
                default:
                    return Align.Auto;
            }
        }
    }
}