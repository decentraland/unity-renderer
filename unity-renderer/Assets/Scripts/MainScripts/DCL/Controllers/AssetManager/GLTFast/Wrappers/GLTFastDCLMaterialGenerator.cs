using DCL.Helpers;
using GLTFast;
using GLTFast.Materials;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using GLTFastMaterial = GLTFast.Schema.Material;
using Material = UnityEngine.Material;

namespace DCL.GLTFast.Wrappers
{
    public class GLTFastDCLMaterialGenerator : MaterialGenerator
    {
        // Historically we have no data on why we have this intensity
        private const float EMISSIVE_HDR_INTENSITY = 5f;
        
        private const string OCCLUSION_KEYWORD = "_OCCLUSION";
        private const string EMISSION_KEYWORD = "_EMISSION";
        private const string ALPHA_PREMULTIPLY_KEYWORD = "_ALPHAPREMULTIPLY_ON";
        private const string ALPHA_TEST_KEYWORD = "_ALPHATEST_ON";
        private const string SPECGLOSSMAP_KEYWORD = "_SPECGLOSSMAP";
        private const string NORMALMAP_KEYWORD = "_NORMALMAP";
        private const string METALLICSPECGLOSSMAP_KEYWORD = "_METALLICSPECGLOSSMAP";
        
        private const string RENDERER_TYPE = "RenderType";

        // NOTE(Kinerius) All those Shader properties that are not supported are there in purpose since they are being
        //                used by GLTFast shaders and might be specs from GLTF format that we have to support in the future

        static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        static readonly int SpecGlossMapScaleTransform = Shader.PropertyToID("_SpecGlossMap_ST"); // we dont support this yet
        static readonly int SpecGlossMapRotation = Shader.PropertyToID("_SpecGlossMapRotation"); // we dont support this yet
        static readonly int SpecGlossMapUVChannelPropId = Shader.PropertyToID("_SpecGlossMapUVChannel"); // we dont support this yet

        static readonly int MetallicRoughnessMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        static readonly int MetallicRoughnessMapScaleTransformPropId = Shader.PropertyToID("metallicRoughnessTexture_ST"); // we dont support this yet
        static readonly int MetallicRoughnessMapRotationPropId = Shader.PropertyToID("metallicRoughnessTextureRotation"); // we dont support this yet
        static readonly int MetallicRoughnessMapUVChannelPropId = Shader.PropertyToID("metallicRoughnessTextureUVChannel"); // we dont support this yet

        static readonly int SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        static readonly int GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        static readonly int Glossiness = Shader.PropertyToID("_Glossiness");

        static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int BaseMapScaleTransform = Shader.PropertyToID("_BaseMap_ST"); // we dont support this yet
        static readonly int BaseMapRotation = Shader.PropertyToID("_BaseMapRotation"); // we dont support this yet
        static readonly int BaseMapUVs = Shader.PropertyToID("_BaseMapUVs");

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        static readonly int NormalMapUVs = Shader.PropertyToID("_NormalMapUVs");
        static readonly int MetallicMapUVs = Shader.PropertyToID("_MetallicMapUVs");
        static readonly int EmissiveMapUVs = Shader.PropertyToID("_EmissiveMapUVs");

        static readonly int Metallic = Shader.PropertyToID("_Metallic");
        static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
        static readonly int Cutoff = Shader.PropertyToID("_Cutoff");

        static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
        static readonly int BumpMapRotationPropId = Shader.PropertyToID("_BumpMapRotation"); // we dont support this yet
        static readonly int BumpMapScaleTransformPropId = Shader.PropertyToID("_BumpMap_ST"); // we dont support this yet
        static readonly int BumpMapUVChannelPropId = Shader.PropertyToID("_BumpMapUVChannel"); // we dont support this yet

        static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        static readonly int OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");
        static readonly int OcclusionMapRotation = Shader.PropertyToID("_OcclusionMapRotation"); // we dont support this yet
        static readonly int OcclusionMapScaleTransform = Shader.PropertyToID("_OcclusionMap_ST"); // we dont support this yet
        static readonly int OcclusionMapUVChannel = Shader.PropertyToID("_OcclusionMapUVChannel"); // we dont support this yet

        static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly int EmissionMapRotation = Shader.PropertyToID("_EmissionMapRotation"); // we dont support this yet
        static readonly int EmissionMapScaleTransform = Shader.PropertyToID("_EmissionMap_ST"); // we dont support this yet
        static readonly int EmissionMapUVChannel = Shader.PropertyToID("_EmissionMapUVChannel"); // we dont support this yet

        static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        static readonly int Cull = Shader.PropertyToID("_Cull");

        private Material material;
        private readonly Shader shader;

        public GLTFastDCLMaterialGenerator(string shaderName) { shader = Shader.Find(shaderName); }
        public override Material GetDefaultMaterial() { return new Material(shader); }
        
        /// <summary>
        /// Here we convert a GLTFMaterial into our Material using our shaders
        /// </summary>
        /// <param name="gltfMaterial"></param>
        /// <param name="gltf"></param>
        /// <returns></returns>
        public override Material GenerateMaterial(GLTFastMaterial gltfMaterial, IGltfReadable gltf)
        {
            material = GetDefaultMaterial();
            material.name = gltfMaterial.name;

            if (gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness != null)
            {
                var specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;

                SetColor(specGloss.diffuseColor);
                SetSpecularColor(specGloss.specularColor);
                SetGlossiness(specGloss.glossinessFactor);
                SetBaseMapTexture(specGloss.diffuseTexture, gltf);
                SetSpecularMapTexture(specGloss.specularGlossinessTexture, gltf);
            }
            // If there's a specular-glossiness extension, ignore metallic-roughness
            // (according to extension specification)
            else
            {
                PbrMetallicRoughness roughness = gltfMaterial.pbrMetallicRoughness;

                if (roughness != null)
                {
                    SetColor(roughness.baseColor);
                    SetBaseMapTexture(roughness.baseColorTexture, gltf);
                    SetMetallic(roughness.metallicFactor);
                    SetMetallicRoughnessTexture( gltf, roughness.metallicRoughnessTexture, roughness.roughnessFactor);
                }
            }

            SetBumpMapTexture( gltfMaterial.normalTexture, gltf);
            SetOcclusionTexture(  gltfMaterial.occlusionTexture, gltf);
            SetEmissiveColor( gltfMaterial.emissive);
            SetEmissiveTexture(  gltfMaterial.emissiveTexture, gltf);

            SetAlphaMode(gltfMaterial.alphaModeEnum, gltfMaterial.alphaCutoff);
            SetDoubleSided(gltfMaterial.doubleSided);
            
            SRPBatchingHelper.OptimizeMaterial(material);
            
            return material;
        }
        
        private void SetEmissiveColor( Color gltfMaterialEmissive)
        {
            if (gltfMaterialEmissive != Color.black)
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor(EmissionColor, gltfMaterialEmissive * EMISSIVE_HDR_INTENSITY);
                material.EnableKeyword(EMISSION_KEYWORD);
            }
        }
        
        private void SetEmissiveTexture(TextureInfo emissiveTexture, IGltfReadable gltf)
        {
            if (TrySetTexture(
                    emissiveTexture,
                    material,
                    gltf,
                    EmissionMap,
                    EmissionMapRotation,
                    EmissionMapScaleTransform,
                    EmissionMapUVChannel
                ))
            {
                material.SetInt(EmissiveMapUVs, emissiveTexture.texCoord);
                material.EnableKeyword(EMISSION_KEYWORD);
            }
        }
        
        private void SetOcclusionTexture(OcclusionTextureInfo occlusionTexture, IGltfReadable gltf)
        {
            if (TrySetTexture(
                    occlusionTexture,
                    material,
                    gltf,
                    OcclusionMap,
                    OcclusionMapRotation,
                    OcclusionMapScaleTransform,
                    OcclusionMapUVChannel
                ))
            {
                material.EnableKeyword(OCCLUSION_KEYWORD);
                material.SetFloat(OcclusionStrength, occlusionTexture.strength);
            }
        }
        
        private void SetBumpMapTexture(NormalTextureInfo textureInfo, IGltfReadable gltf)
        {
            if (TrySetTexture(
                    textureInfo,
                    material,
                    gltf, 
                    BumpMap,
                    BumpMapScaleTransformPropId,
                    BumpMapRotationPropId,
                    BumpMapUVChannelPropId
                ))
            {
                material.SetInt(NormalMapUVs, textureInfo.texCoord);
                material.SetFloat(BumpScale, textureInfo.scale);
                material.EnableKeyword(NORMALMAP_KEYWORD);
            }
        }

        private void SetMetallicRoughnessTexture(IGltfReadable gltf, TextureInfo textureInfo, float roughnessFactor)
        {
            if (TrySetTexture(
                    textureInfo,
                    material,
                    gltf,
                    MetallicRoughnessMapPropId,
                    MetallicRoughnessMapScaleTransformPropId,
                    MetallicRoughnessMapRotationPropId,
                    MetallicRoughnessMapUVChannelPropId
                ))
            {
                SetSmoothness(1);
                material.SetInt(MetallicMapUVs, textureInfo.texCoord);
                material.EnableKeyword(METALLICSPECGLOSSMAP_KEYWORD);
            }
            else
            {
                SetSmoothness(1 - roughnessFactor);
            }
        }

        private void SetSmoothness(float roughnessFactor) { material.SetFloat(Smoothness, roughnessFactor); }

        private void SetMetallic(float metallicFactor) { material.SetFloat(Metallic, metallicFactor); }

        private void SetSpecularMapTexture(TextureInfo textureInfo, IGltfReadable gltf)
        {
            if(TrySetTexture(
                textureInfo,
                material,
                gltf,
                SpecGlossMap,
                SpecGlossMapScaleTransform,
                SpecGlossMapRotation,
                SpecGlossMapUVChannelPropId))
            {
                material.SetFloat(SmoothnessTextureChannel, 0);
                material.EnableKeyword(SPECGLOSSMAP_KEYWORD);
            }
        }

        private void SetBaseMapTexture(TextureInfo textureInfo, IGltfReadable gltf)
        {
            TrySetTexture(
                textureInfo,
                material,
                gltf,
                BaseMap,
                BaseMapRotation,
                BaseMapScaleTransform,
                BaseMapUVs
            );
        }

        private void SetSpecularColor(Color color) { material.SetVector(SpecColor, color); }

        private void SetGlossiness(float glossiness)
        {
            material.SetFloat(GlossMapScale, glossiness);
            material.SetFloat(Glossiness, glossiness);
        }

        private void SetColor(Color color) { material.SetColor(BaseColor, color); }

        private void SetAlphaMode(GLTFastMaterial.AlphaMode alphaMode, float alphaCutoff)
        {
            switch (alphaMode)
            {
                case GLTFastMaterial.AlphaMode.MASK:
                    material.SetOverrideTag(RENDERER_TYPE, "TransparentCutout");
                    material.SetInt(SrcBlend, (int)BlendMode.One);
                    material.SetInt(DstBlend, (int)BlendMode.Zero);
                    material.SetInt(ZWrite, 1);
                    material.SetFloat(AlphaClip, 1);
                    material.EnableKeyword(ALPHA_TEST_KEYWORD);
                    material.DisableKeyword(ALPHA_PREMULTIPLY_KEYWORD);
                    material.renderQueue = (int)RenderQueue.AlphaTest;

                    if (material.HasProperty(Cutoff))
                        material.SetFloat(Cutoff, alphaCutoff);

                    break;

                case GLTFastMaterial.AlphaMode.BLEND:
                    material.SetOverrideTag(RENDERER_TYPE, "Transparent");
                    material.SetInt(SrcBlend, (int)BlendMode.SrcAlpha);
                    material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ZWrite, 0);
                    material.DisableKeyword(ALPHA_TEST_KEYWORD);
                    material.DisableKeyword(ALPHA_PREMULTIPLY_KEYWORD);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.SetFloat(Cutoff, 0);
                    break;
                default:
                    material.SetOverrideTag(RENDERER_TYPE, "Opaque");
                    material.SetInt(SrcBlend, (int)BlendMode.One);
                    material.SetInt(DstBlend, (int)BlendMode.Zero);
                    material.SetInt(ZWrite, 1);
                    material.DisableKeyword(ALPHA_TEST_KEYWORD);
                    material.DisableKeyword(ALPHA_PREMULTIPLY_KEYWORD);
                    material.renderQueue = (int)RenderQueue.Geometry;
                    material.SetFloat(Cutoff, 0);
                    break;
            }
        }

        private void SetDoubleSided(bool doubleSided)
        {
            if (doubleSided)
            {
                material.SetInt(Cull, (int)CullMode.Off);
            }
            else
            {
                material.SetInt(Cull, (int)CullMode.Back);
            }

            material.doubleSidedGI = doubleSided;
        }
    }
}