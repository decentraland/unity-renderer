using DCL.ECSComponents;
using DCL.Models;

namespace DCLPlugins.ECS7.TestsUtils
{
    public static class ECS7TestUIUtils
    {
        public static PBUiTransform CreatePBUiTransformDefaultModel()
        {
            var model = new PBUiTransform();
            // EntityId long to int check
            model.Parent = (int)SpecialEntityId.SCENE_ROOT_ENTITY;
            model.AlignContent = YGAlign.FlexStart;
            model.AlignSelf = YGAlign.Auto;
            model.AlignItems = YGAlign.Auto;
            model.BorderBottom = 0;
            model.BorderLeft = 0;
            model.BorderRight = 0;
            model.BorderTop = 0;
            model.Direction = YGDirection.Inherit;
            model.Display = YGDisplay.Flex;
            
            model.Flex = 1;
            model.FlexBasis = float.NaN;
            model.FlexBasisUnit = YGUnit.Point;
            model.FlexDirection = YGFlexDirection.Row;
            model.FlexGrow = 0;
            model.FlexShrink = 1;
            model.FlexWrap = YGWrap.NoWrap;
            
            model.JustifyContent = YGJustify.FlexStart;
            model.Width = 800;
            model.WidthUnit = YGUnit.Point;
            model.Height = 800;
            model.HeightUnit = YGUnit.Point;
            model.MarginBottom = 0;
            model.MarginBottomUnit = YGUnit.Point;
            model.MarginLeft = 0;
            model.MarginLeftUnit = YGUnit.Point;
            model.MarginRight = 0;
            model.MarginRightUnit = YGUnit.Point;
            model.MarginTop = 0;
            model.MarginTopUnit = YGUnit.Point;
            
            model.MaxHeight = float.NaN;
            model.MaxHeightUnit = YGUnit.Point;
            model.MaxWidth = float.NaN;
            model.MaxWidthUnit = YGUnit.Point;
            
            model.MinHeight = float.NaN;
            model.MinHeightUnit = YGUnit.Point;
            model.MinWidth = float.NaN;
            model.MinWidthUnit = YGUnit.Point;
            
            model.Overflow = YGOverflow.Visible;
            
            model.PaddingBottom = 0;
            model.PaddingBottomUnit = YGUnit.Point;
            model.PaddingLeft = 0;
            model.PaddingLeftUnit = YGUnit.Point;
            model.PaddingRight = 0;
            model.PaddingRightUnit = YGUnit.Point;
            model.PaddingTop = 0;
            model.PaddingTopUnit = YGUnit.Point;
            
            model.PositionBottom = 0;
            model.PositionBottomUnit = YGUnit.Point;
            model.PositionLeft = 0;
            model.PositionLeftUnit = YGUnit.Point;
            model.PositionRight = 0;
            model.PositionRightUnit = YGUnit.Point;
            model.PositionTop = 0;
            model.PositionTopUnit = YGUnit.Point;

            model.PositionType = YGPositionType.Relative;
            return model;
        }
    }
}