using Decentraland.Common;

namespace DCL.ECSComponents
{
    public static class PBMaterial_Defaults
    {
        public static float GetAlphaTest(this PBMaterial self)
        {
            if (self.Pbr != null && self.Pbr.HasAlphaTest)
                return self.Pbr.HasAlphaTest ? self.Pbr.AlphaTest : 0.5f;

            if (self.Unlit != null && self.Unlit.HasAlphaTest)
                return self.Unlit.HasAlphaTest ? self.Unlit.AlphaTest : 0.5f;

            return 0.5f;
        }

        public static bool GetCastShadows(this PBMaterial self)
        {
            // Note: HasCastShadows represent the existence of the parameter and not the value of castShadows itself.
            if (self.Pbr != null)
                return !self.Pbr.HasCastShadows || self.Pbr.CastShadows;

            if (self.Unlit != null)
                return !self.Unlit.HasCastShadows || self.Unlit.CastShadows;

            return true;
        }

        public static Color4 GetDiffuseColor(this PBMaterial self)
        {
            if (self.Unlit != null)
                return self.Unlit.DiffuseColor ?? new Color4(Color_Defaults.color4White);

            return new Color4(Color_Defaults.color4White);
        }

        public static Color4 GetAlbedoColor(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.AlbedoColor ?? new Color4(Color_Defaults.color4White);

            return new Color4(Color_Defaults.color4White);
        }

        public static Color3 GetEmissiveColor(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.EmissiveColor ?? new Color3(Color_Defaults.colorBlack);

            return new Color3(Color_Defaults.colorBlack);
        }

        public static Color3 GetReflectiveColor(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.ReflectivityColor ?? new Color3(Color_Defaults.colorWhite);

            return new Color3(Color_Defaults.colorWhite);
        }

        public static MaterialTransparencyMode GetTransparencyMode(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasTransparencyMode ? self.Pbr.TransparencyMode : MaterialTransparencyMode.MtmAuto;

            return MaterialTransparencyMode.MtmAuto;
        }

        public static float GetMetallic(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasMetallic ? self.Pbr.Metallic : 0.5f;

            return 0.5f;
        }

        public static float GetRoughness(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasRoughness ? self.Pbr.Roughness : 0.5f;

            return 0.5f;
        }

        public static float GetGlossiness(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasGlossiness ? self.Pbr.Glossiness : 1f;

            return 1f;
        }

        public static float GetSpecularIntensity(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasSpecularIntensity ? self.Pbr.SpecularIntensity : 1f;

            return 1f;
        }

        public static float GetEmissiveIntensity(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasEmissiveIntensity ? self.Pbr.EmissiveIntensity : 2f;

            return 2f;
        }

        public static float GetDirectIntensity(this PBMaterial self)
        {
            if (self.Pbr != null)
                return self.Pbr.HasDirectIntensity ? self.Pbr.DirectIntensity : 1f;

            return 1f;
        }
    }
}
