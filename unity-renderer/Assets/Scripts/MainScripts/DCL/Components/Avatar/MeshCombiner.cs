using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public static class MeshCombiner
    {
        private static Matrix4x4[] bindposes;

        public static GameObject Combine(SkinnedMeshRenderer bonesContainer, Transform root, bool useBakeMesh = true, int index = 0)
        {
            Transform transform = root;

            var rs = root.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            var tmpBones = bonesContainer.bones;

            List<CombineInstance> combineInstances = new List<CombineInstance>();
            Mesh finalMesh = new Mesh();

            Vector3 lastPos = transform.position;
            transform.position = Vector3.zero;

            List<Material> mats = new List<Material>();

            GameObject result = new GameObject("Combined Avatar");
            Debug.Log("Combining...", result);
            result.transform.parent = null;
            result.transform.position = Vector3.zero;

            List<Mesh> bakedMeshes = new List<Mesh>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<Vector3> sourceNormals = new List<Vector3>();

            SkinnedMeshRenderer bindPosesContainer = null;

            // Recipe:

            // - Re-arrange boneWeights manually
            // - Re-arrange normals manually
            //     - It seems that normals are scaled by the matrix when using CombineMeshes.
            //       This is wrong and and shouldn't happen, so we have to arrange them manually.

            // - Call BakeMesh with useScale to TRUE

            // - Doesn't matter if we have an animation or not, BUT IT HAS TO BE BAKED BEFORE THE ANIMATION STARTS
            //      - If animation already was played, we re-position all bones according to original mesh bindposes matrix
            //        matrix has to be inversed and multiplied by 0 to get position. rotation and scale can be extracted directly
            //        using Matrix4x4 methods.

            // - Set bounds using combined mesh bounds

            // - Root bone has to be correctly aligned, or the mesh is not going to be baked correctly

            for ( int ri = 0; ri < rs.Length; ri++ )
            {
                if ( rs[ri].sharedMesh == null || !rs[ri].enabled )
                    continue;

                bindPosesContainer = rs[ri];

                Debug.Log("BindPoses name: " + bindPosesContainer.transform.parent.name);

                if ( bindposes == null )
                    bindposes = rs[ri].sharedMesh.bindposes;

                break;
            }

            if (bindPosesContainer == null)
            {
                Debug.Log("Combine failure!");
                return null;
            }

            {
                Debug.Log("Reset Bones...");

                var bindPoses = bindPosesContainer.sharedMesh.bindposes;

                // Reset bones
                for ( int i = 0 ; i < tmpBones.Length; i++ )
                {
                    Matrix4x4 bindPose = bindPoses[i].inverse;
                    Vector3 pos = bindPose.MultiplyPoint3x4(Vector3.zero);
                    Quaternion rot = bindPose.rotation;
                    tmpBones[i].position = pos;
                    tmpBones[i].rotation = rot;
                    //Debug.Log("Lossy Scale = " + bindPose.lossyScale.x);
                    //tmpBones[i].localScale = new Vector3(1 / bindPose.lossyScale.x, 1 / bindPose.lossyScale.y, 1 / bindPose.lossyScale.z);
                }
            }

            int subMeshCounter = 0;

            for (int i = 0; i < rs.Length; i++)
            {
                var r = rs[i];

                if ( r.sharedMesh == null || !r.enabled )
                    continue;

                Mesh mesh = null;

                if ( useBakeMesh )
                {
                    mesh = new Mesh();
                    r.BakeMesh(mesh, true);
                    bakedMeshes.Add(mesh);
                }
                else
                {
                    mesh = r.sharedMesh;
                }

                //Debug.Log("Adding weights " + r.sharedMesh.boneWeights.Length + " ... vertices = " + mesh.vertices.Length);
                //sourceNormals.AddRange(r.sharedMesh.normals);

                Transform meshTransform = r.transform;

                r.enabled = false;
                Transform prevParent = meshTransform.parent;
                string parentName = prevParent.name;
                r.transform.SetParent(null, true);

                if ( subMeshCounter < 4 && !parentName.Contains("Mask") )
                {
                    subMeshCounter++;
                    mats.Add(r.sharedMaterial);
                    boneWeights.AddRange(r.sharedMesh.boneWeights);

                    combineInstances.Add( new CombineInstance()
                    {
                        mesh = mesh,
                        transform = meshTransform.localToWorldMatrix
                    });
                }

                r.transform.parent = prevParent;
            }

            transform.position = lastPos;

            finalMesh.CombineMeshes(combineInstances.ToArray(), false, true);
            //finalMesh.normals = sourceNormals.ToArray();

            if (useBakeMesh)
            {
                foreach ( var mesh in bakedMeshes )
                {
                    UnityEngine.Object.Destroy(mesh);
                }
            }

            var poses = bindPosesContainer.sharedMesh.bindposes;

            List<Matrix4x4> newPoses = new List<Matrix4x4>();

            for ( int i = 0; i < poses.Length; i++ )
            {
                newPoses.Add( poses[i] );
            }

            finalMesh.bindposes = newPoses.ToArray();

            finalMesh.boneWeights = boneWeights.ToArray();
            finalMesh.UploadMeshData(true);

            var bounds = finalMesh.bounds;
            Vector3 newCenter = bounds.center;
            newCenter.Scale(new Vector3(1, 0, 1));
            bounds.center = newCenter;

            var newSkinnedMeshRenderer = result.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = finalMesh;
            newSkinnedMeshRenderer.bones = bonesContainer.bones;
            newSkinnedMeshRenderer.rootBone = bonesContainer.rootBone;
            newSkinnedMeshRenderer.localBounds = bounds;
            newSkinnedMeshRenderer.sharedMaterials = mats.Take(4).ToArray();
            newSkinnedMeshRenderer.enabled = false;

            result.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            result.AddComponent<MeshRenderer>().sharedMaterials = newSkinnedMeshRenderer.sharedMaterials;

            result.transform.parent = root;

            return result;
        }
    }
}