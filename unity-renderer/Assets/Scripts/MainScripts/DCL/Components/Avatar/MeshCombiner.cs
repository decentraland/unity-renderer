using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public static class MeshCombiner
    {
        public static GameObject Combine(SkinnedMeshRenderer bonesContainer, Transform root, bool useBakeMesh = true)
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

            bool bonesResetted = false;

            foreach ( var r in rs )
            {
                if ( r.sharedMesh == null || !r.enabled )
                    continue;

                if ( bindPosesContainer == null )
                    bindPosesContainer = r;

                if (!bonesResetted)
                {
                    bonesResetted = true;

                    var bindPoses = bindPosesContainer.sharedMesh.bindposes;
                    // Reset bones
                    Debug.Log("Reset Bones... bindPoses name: " + bindPosesContainer.transform.parent.name);
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

                boneWeights.AddRange(r.sharedMesh.boneWeights);

                //Debug.Log("Adding weights " + r.sharedMesh.boneWeights.Length + " ... vertices = " + mesh.vertices.Length);
                //sourceNormals.AddRange(r.sharedMesh.normals);

                mats.Add(r.sharedMaterial);

                Transform meshTransform = r.transform;

                r.enabled = false;
                Transform prevParent = meshTransform.parent;
                r.transform.SetParent(null, true);

                combineInstances.Add( new CombineInstance()
                {
                    mesh = mesh,
                    transform = meshTransform.localToWorldMatrix
                });

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
            newSkinnedMeshRenderer.rootBone = bindPosesContainer.rootBone;
            newSkinnedMeshRenderer.localBounds = bounds;
            newSkinnedMeshRenderer.sharedMaterials = mats.ToArray();

            // result.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            // result.AddComponent<MeshRenderer>().sharedMaterial = mats[0];

            result.transform.parent = root;

            return result;
        }
    }
}