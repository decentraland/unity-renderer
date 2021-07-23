using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class CombineLayer
    {
        public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        public Dictionary<Texture2D, int> idMap = new Dictionary<Texture2D, int>();
        public CullMode cullMode;
        public bool isOpaque;
    }

    public interface IAvatarMeshCombineHelper
    {
        GameObject Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset);
        Mesh CombineSkinnedMeshes(CombineMeshData combineMeshData);
    }

    public class AvatarMeshCombineHelper : IAvatarMeshCombineHelper
    {
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);

        // These must match the channels defined in the material shader.
        // Channels must be defined with the TEXCOORD semantic.
        private const int TEXTURE_POINTERS_UV_CHANNEL_INDEX = 2;
        private const int EMISSION_COLORS_UV_CHANNEL_INDEX = 3;

        public Mesh CombineSkinnedMeshes(CombineMeshData combineMeshData)
        {
            CombineMeshData data = combineMeshData;
            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(combineMeshData.combineInstances.ToArray(), true, true);

            // bindposes and boneWeights have to be reassigned because CombineMeshes doesn't identify
            // different meshes with boneWeights that correspond to the same bones. Also, bindposes are
            // stacked and repeated for each mesh when they shouldn't.
            //
            // Basically, Mesh.CombineMeshes is bugged for this use case and this is a workaround.

            finalMesh.bindposes = combineMeshData.bindPoses;
            finalMesh.boneWeights = data.boneWeights.ToArray();

            finalMesh.SetUVs(EMISSION_COLORS_UV_CHANNEL_INDEX, data.emissionColors);
            finalMesh.SetUVs(TEXTURE_POINTERS_UV_CHANNEL_INDEX, data.texturePointers);
            finalMesh.SetColors(data.colors);
            finalMesh.subMeshCount = data.subMeshes.Count;
            finalMesh.SetSubMeshes(data.subMeshes);
            finalMesh.Optimize();
            finalMesh.RecalculateBounds();

            finalMesh.UploadMeshData(true);

            return finalMesh;
        }

        public GameObject Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset)
        {
            //
            // Reset bones to put character in T pose. Renderers are going to be baked later.
            // This is a workaround, it had to be done because renderers original matrices don't match the T pose.
            // We need wearables in T pose to properly combine the avatar mesh. 
            //
            AvatarMeshCombinerUtils.ResetBones(bonesContainer);

            //
            // Get combined layers. Layers are groups of renderers that have a id -> tex mapping.
            //
            // This id is going to get written to uv channels so the material can use up to 12 textures
            // in a single draw call.
            //
            // Layers are divided accounting for the 12 textures limit and transparency/opaque limit.
            //
            var layers = AvatarMeshCombinerUtils.Slice( renderers );

            if ( layers == null )
            {
                logger.Log("Combine failure!");
                return null;
            }

            // Prepare mesh data for the combine operation
            CombineMeshData data = new CombineMeshData();
            data.Populate(bonesContainer.sharedMesh.bindposes, layers, materialAsset);

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

            Mesh finalMesh = CombineSkinnedMeshes(data);

            GameObject result = new GameObject("Combined Avatar");
            result.layer = bonesContainer.gameObject.layer;
            result.transform.parent = null;
            result.transform.position = Vector3.zero;

            var newSkinnedMeshRenderer = result.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = finalMesh;
            newSkinnedMeshRenderer.bones = bonesContainer.bones;
            newSkinnedMeshRenderer.rootBone = bonesContainer.rootBone;
            newSkinnedMeshRenderer.localBounds = bonesContainer.localBounds;
            newSkinnedMeshRenderer.sharedMaterials = data.materials.ToArray();
            newSkinnedMeshRenderer.quality = SkinQuality.Bone1;
            newSkinnedMeshRenderer.updateWhenOffscreen = false;
            newSkinnedMeshRenderer.skinnedMotionVectors = false;

            logger.Log(null, "Finish combining avatar. Click here to focus on GameObject.", result);

            // The avatar is combined, so we can destroy the baked meshes.
            for ( int i = 0; i < combineInstancesCount; i++ )
            {
                if ( data.combineInstances[i].mesh != null )
                    Object.Destroy(data.combineInstances[i].mesh);
            }

            return result;
        }
    }
}