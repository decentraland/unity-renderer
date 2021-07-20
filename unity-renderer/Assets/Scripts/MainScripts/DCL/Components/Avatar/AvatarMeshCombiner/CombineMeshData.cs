using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class CombineMeshData
    {
        // Mesh stuff
        public List<CombineInstance> combineInstances = new List<CombineInstance>();
        public Matrix4x4[] bindPoses;
        public List<SubMeshDescriptor> subMeshes = new List<SubMeshDescriptor>();

        // Combined vertex attributes
        public List<BoneWeight> boneWeights = new List<BoneWeight>();
        public List<Vector2> texturePointers = new List<Vector2>();
        public List<Color> colors = new List<Color>();
        public List<Vector4> emissionColors = new List<Vector4>();

        // Renderers
        public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        public List<Material> materials = new List<Material>();

        public void Populate(Matrix4x4[] bindPoses, List<CombineLayer> layers, Material materialAsset)
        {
            this.bindPoses = bindPoses;
            this.renderers = layers.SelectMany( (x) => x.renderers ).ToList();

            this.ComputeVertexAttributesData( layers, materialAsset );
            this.ComputeCombineInstancesData( layers );
            this.ComputeSubMeshes( layers );
        }
    }
}