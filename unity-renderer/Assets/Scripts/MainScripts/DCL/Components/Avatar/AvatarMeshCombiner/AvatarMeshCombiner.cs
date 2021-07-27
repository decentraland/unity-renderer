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
    }


    public struct AvatarMeshCombinerOutput
    {
        public Mesh mesh;
        public Material[] materials;
    }

    public interface IAvatarMeshCombiner
    {
        AvatarMeshCombinerOutput CombineSkinnedMeshes(CombineMeshData combineMeshData);
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
            CombineMeshData data = new CombineMeshData();

            data.bindPoses = bindPoses;
            data.renderers = layers.SelectMany( (x) => x.renderers ).ToList();

            data.ComputeBoneWeights( layers );
            data.ComputeCombineInstancesData( layers );
            data.ComputeSubMeshes( layers );
            data.FlattenMaterials( layers, materialAsset );

            int combineInstancesCount = data.combineInstances.Count;

            for ( int i = 0; i < combineInstancesCount; i++)
            {
                Mesh mesh = new Mesh();

                // Important note: It seems that mesh normals are scaled by the matrix when using BakeMesh.
                //                 This is wrong and and shouldn't happen, so we have to arrange them manually.
                //
                //                 We DON'T do this yet because the meshes can be read-only, so the original
                //                 normals can't be extracted. For normals, visual artifacts are minor because
                //                 toon shader doesn't use any kind of normal mapping.
                data.renderers[i].BakeMesh(mesh, true);

                var combinedInstance = data.combineInstances[i];
                combinedInstance.mesh = mesh;

                data.combineInstances[i] = combinedInstance;
            }

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(data.combineInstances.ToArray(), true, true);

            // The avatar is combined, so we can destroy the baked meshes.
            for ( int i = 0; i < combineInstancesCount; i++ )
            {
                if ( data.combineInstances[i].mesh != null )
                    Object.Destroy(data.combineInstances[i].mesh);
            }

            // bindposes and boneWeights have to be reassigned because CombineMeshes doesn't identify
            // different meshes with boneWeights that correspond to the same bones. Also, bindposes are
            // stacked and repeated for each mesh when they shouldn't.
            //
            // Basically, Mesh.CombineMeshes is bugged for this use case and this is a workaround.

            finalMesh.bindposes = data.bindPoses;
            finalMesh.boneWeights = data.boneWeights.ToArray();

            finalMesh.SetUVs(EMISSION_COLORS_UV_CHANNEL_INDEX, data.emissionColors);
            finalMesh.SetUVs(TEXTURE_POINTERS_UV_CHANNEL_INDEX, data.texturePointers);
            finalMesh.SetColors(data.colors);

            if ( data.subMeshes.Count > 1 )
            {
                finalMesh.subMeshCount = data.subMeshes.Count;
                finalMesh.SetSubMeshes(data.subMeshes);
            }

            finalMesh.Optimize();

            finalMesh.UploadMeshData(true);

            result.mesh = finalMesh;
            result.materials = data.materials.ToArray();

            return result;
        }
    }
}