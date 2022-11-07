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
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();

            containerModel.components.Add(componentId);

            if (containerModel.parentId != model.GetParent())
            {
                containerModel.parentId = model.GetParent();
                containerModel.parentElement?.Remove(containerModel.rootElement);
                containerModel.parentElement = null;
            }
            containerModel.shouldSort = containerModel.rigthOf != model.GetRightOf();
            containerModel.rigthOf = model.GetRightOf();

            VisualElement element = containerModel.rootElement;

            SetUpVisualElement(element, model);
            internalUiContainer.PutFor(scene, entity, containerModel);
        }

        private static void SetUpVisualElement(VisualElement element, PBUiTransform model)
        {
            element.style.display = GetDisplay(model.GetDisplay());
            element.style.overflow = GetOverflow(model.GetOverflow());

            // Flex
            element.style.flexDirection = GetFlexDirection(model.GetFlexDirection());
            if (!float.IsNaN(model.GetFlexBasis()))
                element.style.flexBasis = new Length(model.GetFlexBasis(), GetUnit(model.GetFlexBasisUnit()));
            else
                element.style.flexBasis = new StyleLength(StyleKeyword.Auto);

            element.style.flexGrow = model.GetFlexGrow();
            element.style.flexShrink = model.GetFlexShrink();
            element.style.flexWrap = GetWrap(model.GetFlexWrap());

            // Align 
            if (model.GetAlignContent() != YGAlign.YgaFlexStart)
                element.style.alignContent = GetAlign(model.GetAlignContent());
            if (model.GetAlignItems() != YGAlign.YgaAuto)
                element.style.alignItems = GetAlign(model.GetAlignItems());
            if (model.GetAlignSelf() != YGAlign.YgaAuto)
                element.style.alignSelf = GetAlign(model.GetAlignSelf());
            element.style.justifyContent = GetJustify(model.GetJustifyContent());

            // Layout size
            if (!float.IsNaN(model.GetHeight()))
                element.style.height = new Length(model.GetHeight(), GetUnit(model.GetHeightUnit()));
            if (!float.IsNaN(model.GetWidth()))
                element.style.width = new Length(model.GetWidth(), GetUnit(model.GetWidthUnit()));

            if (!float.IsNaN(model.GetMaxWidth()))
                element.style.maxWidth = new Length(model.GetMaxWidth(), GetUnit(model.GetMaxWidthUnit()));
            else
                element.style.maxWidth = new StyleLength(StyleKeyword.Auto);
            if (!float.IsNaN(model.GetMaxHeight()))
                element.style.maxHeight = new Length(model.GetMaxHeight(), GetUnit(model.GetMaxHeightUnit()));
            else
                element.style.maxHeight = new StyleLength(StyleKeyword.Auto);

            if (!float.IsNaN(model.GetMinHeight()))
                element.style.minHeight = new Length(model.GetMinHeight(), GetUnit(model.GetMinHeightUnit()));
            if (!float.IsNaN(model.GetMinWidth()))
                element.style.minWidth = new Length(model.GetMinWidth(), GetUnit(model.GetMinWidthUnit()));

            // Paddings
            if (!Mathf.Approximately(model.GetPaddingBottom(), 0))
                element.style.paddingBottom = new Length(model.GetPaddingBottom(), GetUnit(model.GetPaddingBottomUnit()));
            if (!Mathf.Approximately(model.GetPaddingLeft(), 0))
                element.style.paddingLeft = new Length(model.GetPaddingLeft(), GetUnit(model.GetPaddingLeftUnit()));
            if (!Mathf.Approximately(model.GetPaddingRight(), 0))
                element.style.paddingRight = new Length(model.GetPaddingRight(), GetUnit(model.GetPaddingRightUnit()));
            if (!Mathf.Approximately(model.GetPaddingTop(), 0))
                element.style.paddingTop = new Length(model.GetPaddingTop(), GetUnit(model.GetPaddingTopUnit()));

            // Margins
            if (!Mathf.Approximately(model.GetMarginLeft(), 0))
                element.style.marginLeft = new Length(model.GetMarginLeft(), GetUnit(model.GetMarginLeftUnit()));
            if (!Mathf.Approximately(model.GetMarginRight(), 0))
                element.style.marginRight = new Length(model.GetMarginRight(), GetUnit(model.GetMarginRightUnit()));
            if (!Mathf.Approximately(model.GetMarginBottom(), 0))
                element.style.marginBottom = new Length(model.GetMarginBottom(), GetUnit(model.GetMarginBottomUnit()));
            if (!Mathf.Approximately(model.GetMarginTop(), 0))
                element.style.marginTop = new Length(model.GetMarginTop(), GetUnit(model.GetMarginTopUnit()));

            // Position
            element.style.position = GetPosition(model.GetPositionType());
        }

        private static LengthUnit GetUnit(YGUnit unit)
        {
            switch (unit)
            {
                case YGUnit.YguPoint:
                    return LengthUnit.Pixel;
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
                case YGOverflow.YgoVisible:
                    return Overflow.Visible;
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
                case YGDisplay.YgdFlex:
                    return DisplayStyle.Flex;
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
                case YGPositionType.YgptRelative:
                    return UnityEngine.UIElements.Position.Relative;
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