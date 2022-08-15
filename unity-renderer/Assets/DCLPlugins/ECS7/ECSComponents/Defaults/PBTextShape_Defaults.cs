namespace DCL.ECSComponents
{
    public static class PBTextShape_Defaults
    {
        public static bool GetVisible(this PBTextShape self)
        {
            return !self.HasVisible || self.Visible;
        }
        
        public static float GetOpacity(this PBTextShape self)
        {
            return self.HasOpacity ? self.Opacity : 1.0f;
        }

        public static float GetFontSize(this PBTextShape self)
        {
            return self.HasFontSize ? self.FontSize : 10.0f;
        }
        
        public static string GetHTextAlign(this PBTextShape self)
        {
            return self.HasHTextAlign ? self.HTextAlign : "center";
        }
        
        public static string GetVTextAlign(this PBTextShape self)
        {
            return self.HasVTextAlign ? self.VTextAlign : "center";
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
        
        public static Color3 GetTextColor(this PBTextShape self)
        {
            return self.TextColor ?? new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };
        }
    }
}