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
            model.AlignContent = YGAlign.YgaFlexStart;
            model.AlignSelf = YGAlign.YgaAuto;
            model.AlignItems = YGAlign.YgaAuto;
            model.BorderBottom = 0;
            model.BorderLeft = 0;
            model.BorderRight = 0;
            model.BorderTop = 0;
            model.Direction = YGDirection.YgdInherit;
            model.Display = YGDisplay.YgdFlex;
            
            model.Flex = 1;
            model.FlexBasis = float.NaN;
            model.FlexBasisUnit = YGUnit.YguPoint;
            model.FlexDirection = YGFlexDirection.YgfdRow;
            model.FlexGrow = 0;
            model.FlexShrink = 1;
            model.FlexWrap = YGWrap.YgwNoWrap;
            
            model.JustifyContent = YGJustify.YgjFlexStart;
            model.Width = 800;
            model.WidthUnit = YGUnit.YguPoint;
            model.Height = 800;
            model.HeightUnit = YGUnit.YguPoint;
            model.MarginBottom = 0;
            model.MarginBottomUnit = YGUnit.YguPoint;
            model.MarginLeft = 0;
            model.MarginLeftUnit = YGUnit.YguPoint;
            model.MarginRight = 0;
            model.MarginRightUnit = YGUnit.YguPoint;
            model.MarginTop = 0;
            model.MarginTopUnit = YGUnit.YguPoint;
            
            model.MaxHeight = float.NaN;
            model.MaxHeightUnit = YGUnit.YguPoint;
            model.MaxWidth = float.NaN;
            model.MaxWidthUnit = YGUnit.YguPoint;
            
            model.MinHeight = float.NaN;
            model.MinHeightUnit = YGUnit.YguPoint;
            model.MinWidth = float.NaN;
            model.MinWidthUnit = YGUnit.YguPoint;
            
            model.Overflow = YGOverflow.YgoVisible;
            
            model.PaddingBottom = 0;
            model.PaddingBottomUnit = YGUnit.YguPoint;
            model.PaddingLeft = 0;
            model.PaddingLeftUnit = YGUnit.YguPoint;
            model.PaddingRight = 0;
            model.PaddingRightUnit = YGUnit.YguPoint;
            model.PaddingTop = 0;
            model.PaddingTopUnit = YGUnit.YguPoint;
            
            model.PositionBottom = 0;
            model.PositionBottomUnit = YGUnit.YguPoint;
            model.PositionLeft = 0;
            model.PositionLeftUnit = YGUnit.YguPoint;
            model.PositionRight = 0;
            model.PositionRightUnit = YGUnit.YguPoint;
            model.PositionTop = 0;
            model.PositionTopUnit = YGUnit.YguPoint;

            model.PositionType = YGPositionType.YgptRelative;
            return model;
        }
    }
}