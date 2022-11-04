namespace DCL.ECSComponents
{
    public static class PBUiTransform_Defaults
    {
        public static long GetParent(this PBUiTransform self)
        {
            return self.HasParent ? self.Parent : 0;
        }

        public static long GetRightOf(this PBUiTransform self)
        {
            return self.HasRightOf ? self.RightOf : 0;
        }

        public static YGDisplay GetDisplay(this PBUiTransform self)
        {
            return self.HasDisplay ? self.Display : YGDisplay.YgdFlex;
        }

        public static float GetFlexBasis(this PBUiTransform self)
        {
            return self.HasFlexBasis ? self.FlexBasis : float.NaN;
        }

        public static float GetWidth(this PBUiTransform self)
        {
            return self.HasWidth ? self.Width : 0;
        }

        public static float GetHeight(this PBUiTransform self)
        {
            return self.HasHeight ? self.Height : 0;
        }

        public static float GetMinWidth(this PBUiTransform self)
        {
            return self.HasMinWidth ? self.MinWidth : 0;
        }

        public static float GetMinHeight(this PBUiTransform self)
        {
            return self.HasMinHeight ? self.MinHeight : 0;
        }

        public static float GetMaxWidth(this PBUiTransform self)
        {
            return self.HasMaxWidth ? self.MaxWidth : float.NaN;
        }

        public static float GetMaxHeight(this PBUiTransform self)
        {
            return self.HasMaxHeight ? self.MaxHeight : float.NaN;
        }

        public static YGJustify GetJustifyContent(this PBUiTransform self)
        {
            return self.HasJustifyContent ? self.JustifyContent : YGJustify.YgjFlexStart;
        }

        public static float GetFlexShrink(this PBUiTransform self)
        {
            return self.HasFlexShrink ? self.FlexShrink : 1;
        }

        public static float GetFlex(this PBUiTransform self)
        {
            return self.HasFlex ? self.Flex : 1;
        }

        public static float GetFlexGrow(this PBUiTransform self)
        {
            return self.HasFlexGrow ? self.FlexGrow : 0;
        }

        public static float GetMarginBottom(this PBUiTransform self)
        {
            return self.HasMarginBottom ? self.MarginBottom : 0;
        }

        public static float GetMarginLeft(this PBUiTransform self)
        {
            return self.HasMarginLeft ? self.MarginLeft : 0;
        }

        public static float GetMarginRight(this PBUiTransform self)
        {
            return self.HasMarginRight ? self.MarginRight : 0;
        }

        public static float GetMarginTop(this PBUiTransform self)
        {
            return self.HasMarginTop ? self.MarginTop : 0;
        }

        public static float GetPaddingBottom(this PBUiTransform self)
        {
            return self.HasPaddingBottom ? self.PaddingBottom : 0;
        }

        public static float GetPaddingLeft(this PBUiTransform self)
        {
            return self.HasPaddingLeft ? self.PaddingLeft : 0;
        }

        public static float GetPaddingRight(this PBUiTransform self)
        {
            return self.HasPaddingRight ? self.PaddingRight : 0;
        }

        public static float GetPaddingTop(this PBUiTransform self)
        {
            return self.HasPaddingTop ? self.PaddingTop : 0;
        }

        public static float GetPositionBottom(this PBUiTransform self)
        {
            return self.HasPositionBottom ? self.PositionBottom : 0;
        }

        public static float GetPositionLeft(this PBUiTransform self)
        {
            return self.HasPositionLeft ? self.PositionLeft : 0;
        }

        public static float GetPositionRight(this PBUiTransform self)
        {
            return self.HasPositionRight ? self.PositionRight : 0;
        }

        public static float GetPositionTop(this PBUiTransform self)
        {
            return self.HasPositionTop ? self.PositionTop : 0;
        }

        public static YGAlign GetAlignItems(this PBUiTransform self)
        {
            return self.HasAlignItems ? self.AlignItems : YGAlign.YgaStretch;
        }

        public static YGAlign GetAlignSelf(this PBUiTransform self)
        {
            return self.HasAlignSelf ? self.AlignSelf : YGAlign.YgaAuto;
        }

        public static YGAlign GetAlignContent(this PBUiTransform self)
        {
            return self.HasAlignContent ? self.AlignContent : YGAlign.YgaStretch;
        }

        public static YGFlexDirection GetFlexDirection(this PBUiTransform self)
        {
            return self.HasFlexDirection ? self.FlexDirection : YGFlexDirection.YgfdRow;
        }

        public static YGPositionType GetPositionType(this PBUiTransform self)
        {
            return self.HasPositionType ? self.PositionType : YGPositionType.YgptRelative;
        }

        public static YGDirection GetDirection(this PBUiTransform self)
        {
            return self.HasDirection ? self.Direction : YGDirection.YgdInherit;
        }

        public static YGWrap GetFlexWrap(this PBUiTransform self)
        {
            return self.HasFlexWrap ? self.FlexWrap : YGWrap.YgwWrap;
        }

        public static YGUnit GetMarginBottomUnit(this PBUiTransform self)
        {
            return self.HasMarginBottomUnit ? self.MarginBottomUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMarginLeftUnit(this PBUiTransform self)
        {
            return self.HasMarginLeftUnit ? self.MarginLeftUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMarginRightUnit(this PBUiTransform self)
        {
            return self.HasMarginRightUnit ? self.MarginRightUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMarginTopUnit(this PBUiTransform self)
        {
            return self.HasMarginTopUnit ? self.MarginTopUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMaxHeightUnit(this PBUiTransform self)
        {
            return self.HasMaxHeightUnit ? self.MaxHeightUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMaxWidthUnit(this PBUiTransform self)
        {
            return self.HasMaxWidthUnit ? self.MaxWidthUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMinHeightUnit(this PBUiTransform self)
        {
            return self.HasMinHeightUnit ? self.MinHeightUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetMinWidthUnit(this PBUiTransform self)
        {
            return self.HasMinWidthUnit ? self.MinWidthUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPaddingBottomUnit(this PBUiTransform self)
        {
            return self.HasPaddingBottomUnit ? self.PaddingBottomUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPaddingLeftUnit(this PBUiTransform self)
        {
            return self.HasPaddingLeftUnit ? self.PaddingLeftUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPaddingTopUnit(this PBUiTransform self)
        {
            return self.HasPaddingTopUnit ? self.PaddingTopUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPaddingRightUnit(this PBUiTransform self)
        {
            return self.HasPaddingRightUnit ? self.PaddingRightUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPositionBottomUnit(this PBUiTransform self)
        {
            return self.HasPositionBottomUnit ? self.PositionBottomUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPositionLeftUnit(this PBUiTransform self)
        {
            return self.HasPositionLeftUnit ? self.PositionLeftUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPositionRightUnit(this PBUiTransform self)
        {
            return self.HasPositionRightUnit ? self.PositionRightUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetPositionTopUnit(this PBUiTransform self)
        {
            return self.HasPositionTopUnit ? self.PositionTopUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetFlexBasisUnit(this PBUiTransform self)
        {
            return self.HasFlexBasisUnit ? self.FlexBasisUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetWidthUnit(this PBUiTransform self)
        {
            return self.HasWidthUnit ? self.WidthUnit : YGUnit.YguUndefined;
        }

        public static YGUnit GetHeightUnit(this PBUiTransform self)
        {
            return self.HasHeightUnit ? self.HeightUnit : YGUnit.YguUndefined;
        }

        public static YGOverflow GetOverflow(this PBUiTransform self)
        {
            return self.HasOverflow ? self.Overflow : YGOverflow.YgoVisible;
        }
    }
}