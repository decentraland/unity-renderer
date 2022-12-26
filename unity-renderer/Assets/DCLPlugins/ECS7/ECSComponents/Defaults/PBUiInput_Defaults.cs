using Decentraland.Common;

namespace DCL.ECSComponents
{
    public static class PBUiInput_Defaults
    {
        private static readonly Color4 PLACEHOLDER_COLOR = new () { R = 0.3f, G = 0.3f, B = 0.3f, A = 1.0f };

        public static Color4 GetColor(this PBUiInput self) =>
            self.Color ?? Color_Defaults.color4Black;

        public static Color4 GetPlaceholderColor(this PBUiInput self) =>
            self.PlaceholderColor ?? PLACEHOLDER_COLOR;

        public static bool IsInteractable(this PBUiInput self) =>
            !self.Disabled;

        public static TextAlignMode GetTextAlign(this PBUiInput self) =>
            self.HasTextAlign ? self.TextAlign : TextAlignMode.TamMiddleCenter;

        public static Font GetFont(this PBUiInput self) =>
            self.HasFont ? self.Font : Font.FSansSerif;

        public static float GetFontSize(this PBUiInput self) =>
            self.HasFontSize ? self.FontSize : 10;
    }
}
