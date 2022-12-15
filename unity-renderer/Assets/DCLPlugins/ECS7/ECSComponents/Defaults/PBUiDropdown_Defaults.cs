namespace DCL.ECSComponents
{
    public static class PBUiDropdown_Defaults
    {
        public static Color4 GetColor(this PBUiDropdown self) =>
            self.Color ?? Color_Defaults.color4White;

        public static Font GetFont(this PBUiDropdown self) =>
            self.HasFont ? self.Font : Font.FSansSerif;

        public static float GetFontSize(this PBUiDropdown self) =>
            self.HasFontSize ? self.FontSize : 10;

        public static TextAlignMode GetTextAlign(this PBUiDropdown self) =>
            self.HasTextAlign ? self.TextAlign : TextAlignMode.TamMiddleCenter;
    }
}
