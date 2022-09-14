using System;
using GLTFast;
using GLTFast.Materials;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// With this class we can override the material generation from GLTFast, 
    /// in this case we are using the ShaderGraphMaterialGenerator that comes from GLTFast
    /// </summary>
    internal class GLTFastMaterialGenerator : ShaderGraphMaterialGenerator
    {
            
        private const float CUSTOM_EMISSIVE_FACTOR = 5f;

        public override Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf)
        {
            Material generatedMaterial = base.GenerateMaterial(gltfMaterial, gltf);

            if (gltfMaterial.emissive != Color.black)
            {
                generatedMaterial.SetColor(emissionColorPropId, gltfMaterial.emissive * CUSTOM_EMISSIVE_FACTOR);
            }

            return generatedMaterial;
        }
    }
}