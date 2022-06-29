using System;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;

namespace DCL
{
#pragma warning disable CS4014
    internal class GLTFastMaterialGenerator : IMaterialGenerator
    {
        private readonly Shader defaultShader;
        private Material defaultMaterial;
        public GLTFastMaterialGenerator()
        {
            defaultShader = Shader.Find("DCL/Universal Render Pipeline/Lit");
            defaultMaterial = new Material(defaultShader);
        }
        public Material GetDefaultMaterial()
        {
            return defaultMaterial;
        }
        public Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf)
        {
            throw new NotImplementedException();
        }
        public void SetLogger(ICodeLogger logger)
        {
            
        }
    }
}