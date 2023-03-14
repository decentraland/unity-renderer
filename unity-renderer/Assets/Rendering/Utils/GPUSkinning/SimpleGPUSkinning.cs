using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
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

            NativeArray<Vector4> bone01data = new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<Vector4> bone23data = new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var bonesPerVertex = sharedMesh.GetBonesPerVertex();
            var boneWeightsNative = sharedMesh.GetAllBoneWeights();
            var boneIndex = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                byte boneCount = bonesPerVertex[i];

                if (boneCount == 0)
                {
                    bone01data[i] = new Vector4(0, 0, 0, 0);
                    bone23data[i] = new Vector4(0, 0, 0, 0);
                }
                else if (boneCount == 1)
                {
                    var bw0 = boneWeightsNative[boneIndex];
                    bone01data[i] = new Vector4(bw0.boneIndex, bw0.weight, 0, 0);
                    bone23data[i] = new Vector4(0, 0, 0, 0);
                }
                else if (boneCount == 2)
                {
                    var bw0 = boneWeightsNative[boneIndex];
                    var bw1 = boneWeightsNative[boneIndex + 1];
                    bone01data[i] = new Vector4(bw0.boneIndex, bw0.weight, bw1.boneIndex, bw1.weight);
                    bone23data[i] = new Vector4(0, 0, 0, 0);
                }
                else if (boneCount == 3)
                {
                    var bw0 = boneWeightsNative[boneIndex];
                    var bw1 = boneWeightsNative[boneIndex + 1];
                    var bw2 = boneWeightsNative[boneIndex + 2];
                    bone01data[i] = new Vector4(bw0.boneIndex, bw0.weight, bw1.boneIndex, bw1.weight);
                    bone23data[i] = new Vector4(bw2.boneIndex, bw2.weight, 0, 0);
                }
                else if (boneCount >= 4)
                {
                    var bw0 = boneWeightsNative[boneIndex];
                    var bw1 = boneWeightsNative[boneIndex + 1];
                    var bw2 = boneWeightsNative[boneIndex + 2];
                    var bw3 = boneWeightsNative[boneIndex + 3];
                    bone01data[i] = new Vector4(bw0.boneIndex, bw0.weight, bw1.boneIndex, bw1.weight);
                    bone23data[i] = new Vector4(bw2.boneIndex, bw2.weight, bw3.boneIndex, bw3.weight);
                }

                boneIndex += boneCount;
            }

            sharedMesh.SetTangents(bone01data);
            sharedMesh.SetUVs(1, bone23data);

            bone01data.Dispose();
            bone23data.Dispose();
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
            if (encodeBindPoses)
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
            renderer.localBounds = new Bounds(new Vector3(0, 2, 0), new Vector3(1, 3, 1));
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
