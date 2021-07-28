using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace DCL
{
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

    public class FlattenedMaterialsData
    {
        public List<Material> materials = new List<Material>();
        public List<Vector3> texturePointers = new List<Vector3>();
        public List<Color> colors = new List<Color>();
        public List<Vector4> emissionColors = new List<Vector4>();
    }

    public struct AvatarMeshCombinerOutput
    {
        public Mesh mesh;
        public Material[] materials;
    }

    public static class AvatarMeshCombiner
    {
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);

        // These must match the channels defined in the material shader.
        // Channels must be defined with the TEXCOORD semantic.
        private const int TEXTURE_POINTERS_UV_CHANNEL_INDEX = 2;
        private const int EMISSION_COLORS_UV_CHANNEL_INDEX = 3;

        public static AvatarMeshCombinerOutput CombineSkinnedMeshes(Matrix4x4[] bindPoses, List<CombineLayer> layers, Material materialAsset)
        {
            AvatarMeshCombinerOutput result;

            var renderers = layers.SelectMany( (x) => x.renderers ).ToList();

            var boneWeights = AvatarMeshCombinerUtils.ComputeBoneWeights( layers );
            var combineInstancesData = AvatarMeshCombinerUtils.ComputeCombineInstancesData( layers );
            var subMeshDescriptors = AvatarMeshCombinerUtils.ComputeSubMeshes( layers );
            var flattenedMaterialsData = AvatarMeshCombinerUtils.FlattenMaterials( layers, materialAsset );

            int combineInstancesCount = combineInstancesData.Count;

            for ( int i = 0; i < combineInstancesCount; i++)
            {
                Mesh mesh = new Mesh();

                // Important note: It seems that mesh normals are scaled by the matrix when using BakeMesh.
                //                 This is wrong and and shouldn't happen, so we have to arrange them manually.
                //
                //                 We DON'T do this yet because the meshes can be read-only, so the original
                //                 normals can't be extracted. For normals, visual artifacts are minor because
                //                 toon shader doesn't use any kind of normal mapping.
                renderers[i].BakeMesh(mesh, true);

                var combinedInstance = combineInstancesData[i];
                combinedInstance.mesh = mesh;

                combineInstancesData[i] = combinedInstance;
            }

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(combineInstancesData.ToArray(), true, true);

            // The avatar is combined, so we can destroy the baked meshes.
            for ( int i = 0; i < combineInstancesCount; i++ )
            {
                if ( combineInstancesData[i].mesh != null )
                    Object.Destroy(combineInstancesData[i].mesh);
            }

            // bindposes and boneWeights have to be reassigned because CombineMeshes doesn't identify
            // different meshes with boneWeights that correspond to the same bones. Also, bindposes are
            // stacked and repeated for each mesh when they shouldn't.
            //
            // Basically, Mesh.CombineMeshes is bugged for this use case and this is a workaround.

            finalMesh.bindposes = bindPoses;
            finalMesh.boneWeights = boneWeights.ToArray();

            finalMesh.SetUVs(EMISSION_COLORS_UV_CHANNEL_INDEX, flattenedMaterialsData.emissionColors);
            finalMesh.SetUVs(TEXTURE_POINTERS_UV_CHANNEL_INDEX, flattenedMaterialsData.texturePointers);
            finalMesh.SetColors(flattenedMaterialsData.colors);

            if ( subMeshDescriptors.Count > 1 )
            {
                finalMesh.subMeshCount = subMeshDescriptors.Count;
                finalMesh.SetSubMeshes(subMeshDescriptors);
            }

            finalMesh.Optimize();
            finalMesh.UploadMeshData(true);

            result.mesh = finalMesh;
            result.materials = flattenedMaterialsData.materials.ToArray();

            return result;
        }
    }
}