using UnityEngine;

namespace DCL
{
    public readonly struct AssetPromise_Material_Model
    {
        public enum TransparencyMode
        {
            Opaque = 0,
            AlphaTest = 1,
            AlphaBlend = 2,
            AlphaTestAndAlphaBlend = 3,
            Auto = 4
        }

        public readonly struct Texture
        {
            public readonly string src;
            public readonly TextureWrapMode wrapMode;
            public readonly FilterMode filterMode;

            public Texture(string src, TextureWrapMode wrapMode, FilterMode filterMode)
            {
                this.src = src;
                this.wrapMode = wrapMode;
                this.filterMode = filterMode;
            }
        }

        public readonly bool isPbrMaterial;
        public readonly Texture? albedoTexture;
        public readonly float alphaTest;

        public readonly Texture? alphaTexture;
        public readonly Texture? emissiveTexture;
        public readonly Texture? bumpTexture;

        public readonly Color albedoColor;
        public readonly Color emissiveColor;
        public readonly Color reflectivityColor;

        public readonly TransparencyMode transparencyMode;

        public readonly float metallic;
        public readonly float roughness;
        public readonly float glossiness;

        public readonly float specularIntensity;
        public readonly float emissiveIntensity;
        public readonly float directIntensity;

        public static AssetPromise_Material_Model CreateBasicMaterial(Texture albedoTexture, float alphaTest)
        {
            Color defaultColor = Color.white;
            return new AssetPromise_Material_Model(false, albedoTexture, null, null, null,
                alphaTest, defaultColor, defaultColor, defaultColor, TransparencyMode.Auto,
                0, 0, 0, 0, 0, 0);
        }

        public static AssetPromise_Material_Model CreatePBRMaterial(Texture? albedoTexture, Texture? alphaTexture,
            Texture? emissiveTexture, Texture? bumpTexture, float alphaTest, Color albedoColor, Color emissiveColor,
            Color reflectivityColor, TransparencyMode transparencyMode, float metallic, float roughness, float glossiness,
            float specularIntensity, float emissiveIntensity, float directIntensity)
        {
            return new AssetPromise_Material_Model(true, albedoTexture, alphaTexture,
                emissiveTexture, bumpTexture, alphaTest, albedoColor, emissiveColor,
                reflectivityColor, transparencyMode, metallic, roughness, glossiness,
                specularIntensity, emissiveIntensity, directIntensity);
        }

        public AssetPromise_Material_Model(bool isPbrMaterial, Texture? albedoTexture, Texture? alphaTexture,
            Texture? emissiveTexture, Texture? bumpTexture, float alphaTest, Color albedoColor, Color emissiveColor,
            Color reflectivityColor, TransparencyMode transparencyMode, float metallic, float roughness, float glossiness,
            float specularIntensity, float emissiveIntensity, float directIntensity)
        {
            this.isPbrMaterial = isPbrMaterial;
            this.albedoTexture = albedoTexture;
            this.alphaTexture = alphaTexture;
            this.emissiveTexture = emissiveTexture;
            this.bumpTexture = bumpTexture;
            this.alphaTest = alphaTest;
            this.albedoColor = albedoColor;
            this.emissiveColor = emissiveColor;
            this.reflectivityColor = reflectivityColor;
            this.transparencyMode = transparencyMode;
            this.metallic = metallic;
            this.roughness = roughness;
            this.glossiness = glossiness;
            this.specularIntensity = specularIntensity;
            this.emissiveIntensity = emissiveIntensity;
            this.directIntensity = directIntensity;
        }
    }
}