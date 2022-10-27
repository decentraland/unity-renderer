namespace DCL.ECSComponents
{
    public static class PBMaterial_Defaults
    {
        public static float GetAlphaTest(this PBMaterial self)
        {
            return self.HasAlphaTest ? self.AlphaTest : 0.5f;
        }

        public static bool GetCastShadows(this PBMaterial self)
        {
            return !self.HasCastShadows || self.CastShadows;
        }

        public static Color3 GetAlbedoColor(this PBMaterial self)
        {
            return self.AlbedoColor ?? new Color3(Color3_Defaults.colorWhite);
        }

        public static Color3 GetEmissiveColor(this PBMaterial self)
        {
            return self.EmissiveColor ?? new Color3(Color3_Defaults.colorBlack);
        }

        public static Color3 GetReflectiveColor(this PBMaterial self)
        {
            return self.ReflectivityColor ?? new Color3(Color3_Defaults.colorWhite);
        }

        public static MaterialTransparencyMode GetTransparencyMode(this PBMaterial self)
        {
            return self.HasTransparencyMode ? self.TransparencyMode : MaterialTransparencyMode.MtmAuto;
        }

        public static float GetMetallic(this PBMaterial self)
        {
            return self.HasMetallic ? self.Metallic : 0.5f;
        }

        public static float GetRoughness(this PBMaterial self)
        {
            return self.HasRoughness ? self.Roughness : 0.5f;
        }

        public static float GetGlossiness(this PBMaterial self)
        {
            return self.HasGlossiness ? self.Glossiness : 1f;
        }

        public static float GetSpecularIntensity(this PBMaterial self)
        {
            return self.HasSpecularIntensity ? self.SpecularIntensity : 1f;
        }

        public static float GetEmissiveIntensity(this PBMaterial self)
        {
            return self.HasEmissiveIntensity ? self.EmissiveIntensity : 2f;
        }

        public static float GetDirectIntensity(this PBMaterial self)
        {
            return self.HasDirectIntensity ? self.DirectIntensity : 1f;
        }
    }
}