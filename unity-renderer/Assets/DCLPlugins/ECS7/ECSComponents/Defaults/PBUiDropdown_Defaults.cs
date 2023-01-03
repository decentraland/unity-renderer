using Decentraland.Common;

namespace DCL.ECSComponents
{
    public static class PBUiDropdown_Defaults
    {
        public static Color4 GetColor(this PBUiDropdown self) =>
            self.Color ?? Color_Defaults.color4Black;

        public static Font GetFont(this PBUiDropdown self) =>
            self.HasFont ? self.Font : Font.FSansSerif;

        public static float GetFontSize(this PBUiDropdown self) =>
            self.HasFontSize ? self.FontSize : 10;

        public static bool IsInteractable(this PBUiDropdown self) =>
            !self.Disabled;

        public static int GetSelectedIndex(this PBUiDropdown self) =>
            self.SelectedIndex <= -1 ? (self.AcceptEmpty ? -1 : 0) : self.SelectedIndex;

        public static TextAlignMode GetTextAlign(this PBUiDropdown self) =>
            self.HasTextAlign ? self.TextAlign : TextAlignMode.TamMiddleCenter;
    }
}
