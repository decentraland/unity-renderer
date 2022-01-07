using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GPUSkinning
{
    public interface IGPUSkinning
    {
        Renderer renderer { get; }
        void Update();
        void Prepare(SkinnedMeshRenderer skr, bool encodeBindPoses = false);
    }

    public static class GPUSkinningUtils
    {
        /// <summary>
        /// This must be done once per SkinnedMeshRenderer before animating.
        /// </summary>
        /// <param name="skr"></param>
        public static void EncodeBindPosesIntoMesh(SkinnedMeshRenderer skr)
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

            skr.sharedMesh.tangents = bone01data;
            skr.sharedMesh.SetUVs(1, bone23data);
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

        public void Prepare(SkinnedMeshRenderer skr, bool encodeBindPoses = false)
        {
            if ( encodeBindPoses )
                GPUSkinningUtils.EncodeBindPosesIntoMesh(skr);

            boneMatrices = new Matrix4x4[skr.bones.Length];

            GameObject go = skr.gameObject;

            if (!go.TryGetComponent(out MeshFilter meshFilter))
                meshFilter = go.AddComponent<MeshFilter>();

            meshFilter.sharedMesh = skr.sharedMesh;

            renderer = go.AddComponent<MeshRenderer>();
            renderer.enabled = skr.enabled;
            renderer.sharedMaterials = skr.sharedMaterials;
            foreach (Material material in renderer.sharedMaterials)
            {
                material.SetMatrixArray(BIND_POSES, skr.sharedMesh.bindposes.ToArray());
                material.EnableKeyword("_GPU_SKINNING");
            }

            bones = skr.bones;
            meshFilter.mesh.bounds = new Bounds(new Vector3(0, 2, 0), new Vector3(1, 3, 1));
            UpdateMatrices();

            Object.Destroy(skr);
        }

        public void Update()
        {
            if (!renderer.gameObject.activeInHierarchy)
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

            for (int index = 0; index < renderer.sharedMaterials.Length; index++)
            {
                Material material = renderer.sharedMaterials[index];
                material.SetMatrix(RENDERER_WORLD_INVERSE, renderer.transform.worldToLocalMatrix);
                material.SetMatrixArray(BONE_MATRICES, boneMatrices);
            }
        }
    }
}