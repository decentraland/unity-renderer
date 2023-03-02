namespace DCL.ECSComponents
{
    public static class PBUiTransform_Defaults
    {
        public static float GetFlexShrink(this PBUiTransform self)
        {
            return self.HasFlexShrink ? self.FlexShrink : 1;
        }

        public static YGAlign GetAlignItems(this PBUiTransform self)
        {
            return self.HasAlignItems ? self.AlignItems : YGAlign.YgaStretch;
        }

        public static YGAlign GetAlignContent(this PBUiTransform self)
        {
            return self.HasAlignContent ? self.AlignContent : YGAlign.YgaStretch;
        }

        public static YGWrap GetFlexWrap(this PBUiTransform self)
        {
            return self.HasFlexWrap ? self.FlexWrap : YGWrap.YgwNoWrap;
        }
    }
}
