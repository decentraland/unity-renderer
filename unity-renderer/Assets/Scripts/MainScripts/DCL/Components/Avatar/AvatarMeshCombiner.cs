using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AvatarShaderTexturePointers
    {
        private static readonly int GLOBAL_AVATAR_TEXTURE_ARRAY = Shader.PropertyToID("_GlobalAvatarTextureArray");
        const int MAX_ID_COUNT = 400;

        public Texture[] textures;
        public Queue<int> availableIds = new Queue<int>();
        private Dictionary<Texture, int> textureToId = new Dictionary<Texture, int>();
        private Texture2DArray textureArray;

        public AvatarShaderTexturePointers ()
        {
            textureArray = new Texture2DArray(256, 256, MAX_ID_COUNT, TextureFormat.ARGB32, true, true);

            for ( int i = 0 ; i < MAX_ID_COUNT; i++ )
            {
                availableIds.Enqueue(i);
            }

            textures = new Texture[MAX_ID_COUNT];
            UpdateShaderData();
        }

        public int AddTexture(Texture texture)
        {
            if ( texture == null )
            {
                //Debug.Log("Adding null texture to global texture cache!");
                return -1;
            }

            if ( textureToId.ContainsKey(texture))
                return textureToId[texture];

            int newId = availableIds.Dequeue();
            textures[newId] = texture;

            Texture2D newTexture = ConvertTexture(texture as Texture2D);
            Graphics.CopyTexture(newTexture, 0, 0, textureArray, newId, 0);

            //Debug.Log($"Adding {newId} texture to global texture cache!", textureArray);
            textureToId.Add(texture, newId);
            //UpdateShaderData();
            return newId;
        }

        public static Texture2D ConvertTexture(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(256, 256);

            source.filterMode = FilterMode.Trilinear;
            rt.filterMode = FilterMode.Trilinear;
            rt.useMipMap = true;

            Graphics.Blit(source, rt);

            Texture2D nTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);

            Graphics.CopyTexture(rt, nTex);

            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }


        public void RemoveTexture(Texture texture)
        {
            if ( !textureToId.ContainsKey(texture))
                return;

            availableIds.Enqueue(textureToId[texture]);
            textureToId.Remove(texture);
        }

        public void UpdateShaderData()
        {
            Shader.SetGlobalTexture(GLOBAL_AVATAR_TEXTURE_ARRAY, textureArray);
        }
    }

    public static class AvatarMeshCombiner
    {
        public static AvatarShaderTexturePointers pointers = new AvatarShaderTexturePointers();

        public static GameObject Combine(SkinnedMeshRenderer bonesContainer, Transform root, System.Func<Renderer, bool> filterFunction = null)
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
            result.layer = root.gameObject.layer;
            result.transform.parent = null;
            result.transform.position = Vector3.zero;

            List<Mesh> bakedMeshes = new List<Mesh>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            //List<Vector3> sourceNormals = new List<Vector3>();
            List<Vector2> texturePointers = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<Vector4> emissionColors = new List<Vector4>();

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

                //Debug.Log("BindPoses name: " + bindPosesContainer.transform.parent.name);
                break;
            }

            if (bindPosesContainer == null)
            {
                Debug.Log("Combine failure!");
                return null;
            }

            {
                Debug.Log("Combining...");

                var bindPoses = bindPosesContainer.sharedMesh.bindposes;

                // Reset bones
                for ( int i = 0 ; i < tmpBones.Length; i++ )
                {
                    Matrix4x4 bindPose = bindPoses[i].inverse;
                    tmpBones[i].position = bindPose.MultiplyPoint3x4(Vector3.zero);
                    tmpBones[i].rotation = bindPose.rotation;

                    Vector3 bindPoseScale = bindPose.lossyScale;
                    Vector3 boneScale = tmpBones[i].lossyScale;

                    tmpBones[i].localScale = new Vector3(bindPoseScale.x / boneScale.x,
                        bindPoseScale.y / boneScale.y,
                        bindPoseScale.z / boneScale.z);
                }
            }

            for (int i = 0; i < rs.Length; i++)
            {
                var r = rs[i];

                if ( r.sharedMesh == null || !r.enabled )
                    continue;

                Mesh mesh = null;

                mesh = new Mesh();
                r.BakeMesh(mesh, true);
                bakedMeshes.Add(mesh);

                //Debug.Log("Adding weights " + r.sharedMesh.boneWeights.Length + " ... vertices = " + mesh.vertices.Length);
                //sourceNormals.AddRange(r.sharedMesh.normals);

                Transform meshTransform = r.transform;

                r.enabled = false;

                bool canMerge = true;

                if ( filterFunction != null )
                    canMerge = filterFunction.Invoke(r);

                if ( !canMerge )
                    continue;

                Transform prevParent = meshTransform.parent;

                r.transform.SetParent(null, true);

                Material mat = r.sharedMaterial;
                mats.Add(mat);

                int id1 = pointers.AddTexture(mat.GetTexture(ShaderUtils.BaseMap));
                int id2 = pointers.AddTexture(mat.GetTexture(ShaderUtils.EmissionMap));

                var meshBoneWeights = r.sharedMesh.boneWeights;

                //Debug.Log($"Setting texture ids... basemap: {id1} ... emission: {id2}");

                texturePointers.AddRange(Enumerable.Repeat(new Vector2(id1, id2), meshBoneWeights.Length));

                Color baseColor = mat.GetColor(ShaderUtils.BaseColor);
                Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);

                colors.AddRange(Enumerable.Repeat(baseColor, meshBoneWeights.Length));
                emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, meshBoneWeights.Length));

                boneWeights.AddRange(meshBoneWeights);

                combineInstances.Add( new CombineInstance()
                {
                    mesh = mesh,
                    transform = meshTransform.localToWorldMatrix
                });

                r.transform.parent = prevParent;
            }

            transform.position = lastPos;

            finalMesh.CombineMeshes(combineInstances.ToArray(), true, true);
            //finalMesh.normals = sourceNormals.ToArray();

            foreach ( var mesh in bakedMeshes )
            {
                UnityEngine.Object.Destroy(mesh);
            }

            var poses = bindPosesContainer.sharedMesh.bindposes;

            List<Matrix4x4> newPoses = new List<Matrix4x4>();

            for ( int i = 0; i < poses.Length; i++ )
            {
                newPoses.Add( poses[i] );
            }

            finalMesh.bindposes = newPoses.ToArray();
            finalMesh.boneWeights = boneWeights.ToArray();
            finalMesh.SetUVs(7, texturePointers);
            finalMesh.SetUVs(6, emissionColors);
            finalMesh.SetColors(colors);
            finalMesh.Optimize();
            finalMesh.UploadMeshData(true);

            var bounds = finalMesh.bounds;
            Vector3 newCenter = bounds.center;
            newCenter.Scale(new Vector3(1, 0, 1));
            bounds.center = newCenter;

            var newSkinnedMeshRenderer = result.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = finalMesh;
            newSkinnedMeshRenderer.bones = bonesContainer.bones;
            newSkinnedMeshRenderer.rootBone = bonesContainer.rootBone;
            newSkinnedMeshRenderer.localBounds = bonesContainer.localBounds;
            newSkinnedMeshRenderer.sharedMaterial = Resources.Load<Material>("OptimizedToonMaterial");
            newSkinnedMeshRenderer.quality = SkinQuality.Bone1;
            newSkinnedMeshRenderer.updateWhenOffscreen = false;

            //result.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            //result.AddComponent<MeshRenderer>().sharedMaterials = newSkinnedMeshRenderer.sharedMaterials;

            result.transform.parent = root;

            return result;
        }
    }
}