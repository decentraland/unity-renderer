using Decentraland.Common;
using UnityEngine;

namespace DCL.ECSComponents
{
    public static class PBTextShape_Defaults
    {
        public static float GetFontSize(this PBTextShape self)
        {
            return self.HasFontSize ? self.FontSize : 10.0f;
        }

        public static TextAlignMode GetTextAlign(this PBTextShape self)
        {
            return self.HasTextAlign ? self.TextAlign : TextAlignMode.TamMiddleCenter;
        }

        public static float GetWidth(this PBTextShape self)
        {
            return self.HasWidth ? self.Width : 1.0f;
        }

        public static float GetHeight(this PBTextShape self)
        {
            return self.HasHeight ? self.Height : 1.0f;
        }

        public static Color3 GetShadowColor(this PBTextShape self)
        {
            return self.ShadowColor ?? new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };
        }

        public static Color3 GetOutlineColor(this PBTextShape self)
        {
            return self.OutlineColor ?? new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };
        }

        public static Color4 GetTextColor(this PBTextShape self)
        {
            return self.TextColor ?? new Color4() { R = 1.0f, G = 1.0f, B = 1.0f, A = 1.0f};
        }

        public static bool GetTextWrapping(this PBTextShape self)
        {
            return self.HasTextWrapping && self.TextWrapping;
        }

        public static int GetLineCount(this PBTextShape self)
        {
            return self.HasLineCount ? self.LineCount : 0;
        }

        public static bool GetFontAutoSize(this PBTextShape self)
        {
            return self.HasFontAutoSize && self.FontAutoSize;
        }

        public static Vector4 GetPadding(this PBTextShape self)
        {
            return new Vector4(
                x: self.HasPaddingLeft ? self.PaddingLeft : 0,
                y: self.HasPaddingTop ? self.PaddingTop : 0,
                z: self.HasPaddingRight ? self.PaddingRight : 0,
                w: self.HasPaddingBottom ? self.PaddingBottom : 0
            );
        }

        public static float GetLineSpacing(this PBTextShape self)
        {
            return self.HasLineSpacing ? self.LineSpacing : 0;
        }

        public static Font GetFont(this PBTextShape self)
        {
            return self.HasFont ? self.Font : Font.FSansSerif;
        }

        public static float GetShadowOffsetX(this PBTextShape self)
        {
            return self.HasShadowOffsetX ? self.ShadowOffsetX : 0;
        }

        public static float GetShadowOffsetY(this PBTextShape self)
        {
            return self.HasShadowOffsetY ? self.ShadowOffsetY : 0;
        }

        public static float GetShadowBlur(this PBTextShape self)
        {
            return self.HasShadowBlur ? self.ShadowBlur : 0;
        }

        public static float GetOutlineWidth(this PBTextShape self)
        {
            return self.HasOutlineWidth ? self.OutlineWidth : 0;
        }
    }
}
