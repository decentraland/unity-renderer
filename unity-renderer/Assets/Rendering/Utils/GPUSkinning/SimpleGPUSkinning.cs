using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GPUSkinning
{
    public interface IGPUSkinning
    {
        Renderer renderer { get; }
        void Update();
    }

    public static class GPUSkinningUtils
    {
        /// <summary>
        /// This must be done once per SkinnedMeshRenderer before animating.
        /// </summary>
        /// <param name="skr"></param>
        public static void EncodeBindPosesIntoMesh(SkinnedMeshRenderer skr, int bone01DataUVChannel = -1, int bone23DataUVChannel = 1)
        {
            Mesh sharedMesh = skr.sharedMesh;
            int vertexCount = sharedMesh.vertexCount;
            Vector4[] bone01data = new Vector4[vertexCount];
            Vector4[] bone23data = new Vector4[vertexCount];

            BoneWeight[] boneWeights = sharedMesh.boneWeights;

            for ( int i = 0; i < vertexCount; i ++ )
            {
                BoneWeight boneWeight = boneWeights[i];
                bone01data[i].x = boneWeight.boneIndex0;
                bone01data[i].y = boneWeight.weight0;
                bone01data[i].z = boneWeight.boneIndex1;
                bone01data[i].w = boneWeight.weight1;

                bone23data[i].x = boneWeight.boneIndex2;
                bone23data[i].y = boneWeight.weight2;
                bone23data[i].z = boneWeight.boneIndex3;
                bone23data[i].w = boneWeight.weight3;
            }

            if(bone01DataUVChannel >= 0)
                skr.sharedMesh.SetUVs(bone01DataUVChannel, bone01data);
            else
                skr.sharedMesh.tangents = bone01data;
            
            skr.sharedMesh.SetUVs(bone23DataUVChannel, bone23data);
        }
    }

    public class SimpleGPUSkinning : IGPUSkinning
    {
        private static readonly int BONE_MATRICES = Shader.PropertyToID("_Matrices");
        private static readonly int BIND_POSES = Shader.PropertyToID("_BindPoses");
        private static readonly int RENDERER_WORLD_INVERSE = Shader.PropertyToID("_WorldInverse");

        public Renderer renderer { get; private set; }

        private Transform[] bones;
        private Matrix4x4[] boneMatrices;

        public SimpleGPUSkinning (SkinnedMeshRenderer skr, bool encodeBindPoses = true, int bone01DataUVChannel = -1, int bone23DataUVChannel = 1)
        {
            Bounds targetBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1000, 1000, 1000));
            
            if ( encodeBindPoses )
                GPUSkinningUtils.EncodeBindPosesIntoMesh(skr, bone01DataUVChannel, bone23DataUVChannel);

            boneMatrices = new Matrix4x4[skr.bones.Length];

            GameObject go = skr.gameObject;

            if (!go.TryGetComponent(out MeshFilter meshFilter))
                meshFilter = go.AddComponent<MeshFilter>();

            meshFilter.sharedMesh = skr.sharedMesh;

            renderer = go.AddComponent<MeshRenderer>();
            
            // TODO: Find a way of using sharedMaterials here, maybe caching 1 "shareable" material with the GPU_SKINNING
            // keyword enabled? otherwise, if we use always sharedMaterials here, all the non-skinned meshes break
            renderer.materials = skr.materials;
            
            for (var i = 0; i < renderer.materials.Length; i++)
            {
                var material = renderer.materials[i];
                
                InitializeShaderMatrix(material, BIND_POSES);
                material.SetMatrixArray(BIND_POSES, skr.sharedMesh.bindposes.ToArray());
                material.EnableKeyword("_GPU_SKINNING");
            }

            bones = skr.bones;
            
            bool isAvatar = bone01DataUVChannel == -1;
            if (isAvatar)
            {
                meshFilter.mesh.bounds = new Bounds(new Vector3(0, 2, 0), new Vector3(1, 3, 1));
            }
            else
            {
                meshFilter.mesh.bounds = targetBounds;
            }
            
            UpdateMatrices();

            Object.Destroy(skr);
        }

        public void Update()
        {
            if (renderer == null || !renderer.isVisible)
                return;

            UpdateMatrices();
        }

        private void UpdateMatrices()
        {
            int bonesLength = bones.Length;
            for (int i = 0; i < bonesLength; i++)
            {
                Transform bone = bones[i];
                boneMatrices[i] = bone.localToWorldMatrix;
            }

            // TODO: Find a way of using sharedMaterials here, maybe caching 1 "shareable" material with the GPU_SKINNING
            // keyword enabled? otherwise, if we use always sharedMaterials here, all the non-skinned meshes break
            for (int index = 0; index < renderer.materials.Length; index++)
            {
                Material material = renderer.materials[index];
                
                InitializeShaderMatrix(material, BONE_MATRICES);
                material.SetMatrix(RENDERER_WORLD_INVERSE, renderer.transform.worldToLocalMatrix);
                material.SetMatrixArray(BONE_MATRICES, boneMatrices);
            }
        }

        // HACK for avoiding: "Property (_Matrices) exceeds previous array size (76 vs 63). Cap to previous size. Restart Unity to recreate the arrays."
        // TODO: Can we optimize this extra matrix spaces somehow? Maybe keeping an int and using it in the shader to make sure it doesn't
        // traverse unneeded matrix elements.
        private void InitializeShaderMatrix(Material material, int matrixPropertyID)
        {
            material.SetMatrixArray(matrixPropertyID, new Matrix4x4[200]);
        }
    }
}