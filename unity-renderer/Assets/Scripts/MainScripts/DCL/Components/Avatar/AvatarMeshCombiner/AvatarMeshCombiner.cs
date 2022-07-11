using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    /// <summary>
    /// This class is used by the AvatarMeshCombiner to combine meshes. Each layer represents a new generated sub-mesh.
    /// </summary>
    public class CombineLayer
    {
        public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        public Dictionary<Texture2D, int> textureToId = new Dictionary<Texture2D, int>();
        public CullMode cullMode;
        public bool isOpaque;

        public override string ToString()
        {
            string rendererString = $"renderer count: {renderers?.Count ?? 0}";
            string textureIdString = "texture ids: {";

            foreach ( var kvp in textureToId )
            {
                textureIdString += $" tx hash: {kvp.Key.GetHashCode()} id: {kvp.Value} ";
            }

            textureIdString += "}";

            return $"cullMode: {cullMode} - isOpaque: {isOpaque} - {rendererString} - {textureIdString}";
        }
    }

    /// <summary>
    /// This class is used to determine the original materials uniform properties
    /// will be passed on to UV values. texturePointers and emissionColors are bound to UV channels.
    /// Colors are bound to the color channel.
    /// </summary>
    public class FlattenedMaterialsData
    {
        public List<Material> materials = new List<Material>();
        public NativeArray<Vector3> texturePointers;
        public NativeArray<Vector4> colors;
        public NativeArray<Vector4> emissionColors;
        public FlattenedMaterialsData(int vertexCount)
        {
            texturePointers = new NativeArray<Vector3>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            colors = new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            emissionColors = new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        }
    }

    /// <summary>
    /// This utility class can combine avatar meshes into a single optimal mesh.
    /// </summary>
    public static class AvatarMeshCombiner
    {
        public struct Output
        {
            public bool isValid;
            public Mesh mesh;
            public Material[] materials;
        }

        // These must match the channels defined in the material shader.
        // Channels must be defined with the TEXCOORD semantic.
        private const int TEXTURE_POINTERS_UV_CHANNEL_INDEX = 2;
        private const int EMISSION_COLORS_UV_CHANNEL_INDEX = 3;

        /// <summary>
        /// CombineSkinnedMeshes combines a list of skinned mesh renderers that share the same bone structure into a
        /// single mesh with the fewest numbers of sub-meshes as possible. It will return a list of materials that match
        /// the number of sub-meshes to be used with a Renderer component.
        /// <br/>
        /// The sub-meshes are divided according to the following constraints:
        /// <ul>
        /// <li>Culling mode</li>
        /// <li>Transparency or opacity of each renderer to be combined</li>
        /// <li>Texture count of the accumulated renderers, only including emission and albedo textures.</li>
        /// </ul>
        /// </summary>
        /// <param name="bindPoses">Bindposes that will be used by all the renderers.</param>
        /// <param name="bones">Bones that will be used by all the renderers.</param>
        /// <param name="renderers">Renderers to be combined.</param>
        /// <param name="materialAsset">Material asset to be used in the resulting Output object. This Material will be instantiated for each sub-mesh generated.</param>
        /// <returns>An Output object with the mesh and materials. Output.isValid will return true if the combining is successful.</returns>
        public static Output CombineSkinnedMeshes(Matrix4x4[] bindPoses, Transform[] bones, SkinnedMeshRenderer[] renderers, Material materialAsset, bool keepPose = true)
        {
            Output result = new Output();
            (Vector3 pos, Quaternion rot, Vector3 scale)[] bonesTransforms = null;

            if(keepPose)
                bonesTransforms = bones.Select(x => (x.position, x.rotation, x.localScale)).ToArray();

            //
            // Reset bones to put character in T pose. Renderers are going to be baked later.
            // This is a workaround, it had to be done because renderers original matrices don't match the T pose.
            // We need wearables in T pose to properly combine the avatar mesh. 
            //
            AvatarMeshCombinerUtils.ResetBones(bindPoses, bones);

            //
            // Get combined layers. Layers are groups of renderers that have a id -> tex mapping.
            //
            // This id is going to get written to uv channels so the material can use up to 12 textures
            // in a single draw call.
            //
            // Layers are divided accounting for the 12 textures limit and transparency/opaque limit.
            //
            List<CombineLayer> layers = CombineLayerUtils.Slice( renderers );

            if ( layers == null )
            {
                result.isValid = false;
                return result;
            }

            // Here, the final combined mesh is generated. This mesh still doesn't have the UV encoded
            // samplers and some needed attributes. Those will be added below.
            var combineInstancesData = AvatarMeshCombinerUtils.ComputeCombineInstancesData( layers );
            Mesh finalMesh = AvatarMeshCombinerUtils.CombineMeshesWithLayers(combineInstancesData, layers);

            // Note about bindPoses and boneWeights reassignment:
            //
            // This has to be done because CombineMeshes doesn't identify different meshes
            // with boneWeights that correspond to the same bones. Also, bindposes are
            // stacked and repeated for each mesh when they shouldn't.
            //
            // This is OK when combining multiple SkinnedMeshRenderers that animate independently,
            // but not in our use case. 

            finalMesh.bindposes = bindPoses;

            var bonesPerVertex = AvatarMeshCombinerUtils.CombineBonesPerVertex(layers);
            var boneWeights = AvatarMeshCombinerUtils.CombineBonesWeights(layers);
            finalMesh.SetBoneWeights(bonesPerVertex, boneWeights);
            
            bonesPerVertex.Dispose();
            boneWeights.Dispose();
            
            var flattenedMaterialsData = AvatarMeshCombinerUtils.FlattenMaterials( layers, materialAsset );
            finalMesh.SetUVs(EMISSION_COLORS_UV_CHANNEL_INDEX, flattenedMaterialsData.emissionColors);
            finalMesh.SetUVs(TEXTURE_POINTERS_UV_CHANNEL_INDEX, flattenedMaterialsData.texturePointers);
            finalMesh.SetColors(flattenedMaterialsData.colors);

            flattenedMaterialsData.emissionColors.Dispose();
            flattenedMaterialsData.texturePointers.Dispose();
            flattenedMaterialsData.colors.Dispose();

            // Each layer corresponds with a subMesh. This is to take advantage of the sharedMaterials array.
            //
            // When a renderer has many sub-meshes, each materials array element correspond to the sub-mesh of
            // the same index. Each layer needs to be renderer with its own material, so it becomes very useful.
            //
            var subMeshDescriptors = AvatarMeshCombinerUtils.ComputeSubMeshes( layers );
            if ( subMeshDescriptors.Count > 1 )
            {
                finalMesh.subMeshCount = subMeshDescriptors.Count;
                finalMesh.SetSubMeshes(subMeshDescriptors);
            }

            finalMesh.Optimize();

            result.mesh = finalMesh;
            result.materials = flattenedMaterialsData.materials.ToArray();
            result.isValid = true;

            if (keepPose)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].position = bonesTransforms[i].pos;
                    bones[i].rotation = bonesTransforms[i].rot;
                    bones[i].localScale = bonesTransforms[i].scale;
                }
            }

            return result;
        }
    }
}