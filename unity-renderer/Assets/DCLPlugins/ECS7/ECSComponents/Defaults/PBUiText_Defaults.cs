namespace DCL.ECSComponents
{
    public static class PBUiText_Defaults
    {
        public static Color3 GetColor(this PBUiText self)
        {
            return self.Color ?? Color3_Defaults.colorWhite;
        }

        public static TextAlignMode GetTextAlign(this PBUiText self)
        {
            return self.HasTextAlign ? self.TextAlign : TextAlignMode.TamMiddleCenter;
        }

        public static Font GetFont(this PBUiText self)
        {
            return self.HasFont ? self.Font : Font.FSansSerif;
        }

        public static float GetFontSize(this PBUiText self)
        {
            return self.HasFontSize ? self.FontSize : 10;
        }
    }
}