using UnityEngine;

namespace MaterialHelpers
{
    public static class GLTFastMaterialConverter
    {
        public static void Convert(Material fromGltfast, Material toDCLLit) 
        { 
            toDCLLit.CopyPropertiesFromMaterial(fromGltfast);
            
            toDCLLit.SetTexture("_BaseMap", fromGltfast.GetTexture("baseColorTexture"));
            toDCLLit.SetTexture("_SpecGlossMap", fromGltfast.GetTexture("baseColorTexture"));
            toDCLLit.SetTexture("_MetallicGlossMap", fromGltfast.GetTexture("metallicRoughnessTexture"));
            toDCLLit.SetTexture("_EmissionMap", fromGltfast.GetTexture("emissiveTexture"));
            toDCLLit.SetTexture("_BumpMap", fromGltfast.GetTexture("normalTexture"));
            toDCLLit.SetTexture("_OcclusionMap", fromGltfast.GetTexture("occlusionTexture"));
            
            toDCLLit.SetFloat("_BaseMapUVs", fromGltfast.GetFloat("_MainTexUVChannel"));
            toDCLLit.SetFloat("_EmissiveMapUVs", fromGltfast.GetFloat("_EmissionMapUVChannel"));
            toDCLLit.SetFloat("_MetallicMapUVs", fromGltfast.GetFloat("_MetallicGlossMapUVChannel"));
            toDCLLit.SetColor("_EmissionColor", fromGltfast.GetColor("_EmissionColor") * 5 );
        }
  
    }
}