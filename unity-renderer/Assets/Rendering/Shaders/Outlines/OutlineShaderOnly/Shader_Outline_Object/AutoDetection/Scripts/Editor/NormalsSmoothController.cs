
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace NormalsOperations
{
    public class NormalsSmoothController : MonoBehaviour
    {
        [FormerlySerializedAs("angle")] [Range(0, 180)] public float normalAngle = 0;

        private MeshFilter[] mfs;
        private Mesh[] originalMeshes;
        private float _lastAngle = 0;

        private void Awake()
        {
            mfs = GetComponentsInChildren<MeshFilter>();
            originalMeshes = new Mesh[mfs.Length];
            
            for (int i = 0; i < mfs.Length; i++)
            {
                MeshFilter meshFilter = mfs[i];
                originalMeshes[i] = meshFilter.sharedMesh;
                Mesh mesh = new Mesh();
                
                mesh.indexFormat = IndexFormat.UInt32;
                
                BackupMesh(meshFilter.sharedMesh, mesh);
                meshFilter.sharedMesh = mesh;
            }
        
            Smooth();
        }


        private void Update()
        {
            if (!Mathf.Approximately(_lastAngle, normalAngle))
            {
                Smooth();
            }

            _lastAngle = normalAngle;
        }

        private void Smooth()
        {
            foreach (MeshFilter meshFilter in mfs)
            {
                meshFilter.sharedMesh.RecalculateNormals(MeshUpdateFlags.DontRecalculateBounds);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < mfs.Length; i++)
            {
                MeshFilter meshFilter = mfs[i];
                Destroy(meshFilter.sharedMesh);
                meshFilter.sharedMesh = originalMeshes[i];
            }
        }
        
        private void CopyMeshT(Mesh source, Mesh destination)
        {
            destination.vertices = source.vertices;
            destination.triangles = source.triangles;
            destination.normals = source.normals;
            destination.uv = source.uv;
            destination.uv2 = source.uv2;
            destination.uv3 = source.uv3;
            destination.uv4 = source.uv4;
            destination.colors = source.colors;
            destination.colors32 = source.colors32;
            destination.tangents = source.tangents;
            destination.boneWeights = source.boneWeights;
            destination.bindposes = source.bindposes;
            destination.bounds = source.bounds;
            destination.subMeshCount = source.subMeshCount;
            for (int i = 0; i < source.subMeshCount; i++)
            {
                destination.SetTriangles(source.GetTriangles(i), i);
            }
        }
        public static void BackupMesh(Mesh source, Mesh destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            Vector3[] v = new Vector3[source.vertices.Length];
            int[][] t = new int[source.subMeshCount][];
            Vector2[] u = new Vector2[source.uv.Length];
            Vector2[] u2 = new Vector2[source.uv2.Length];
            Vector4[] tan = new Vector4[source.tangents.Length];
            Vector3[] n = new Vector3[source.normals.Length];
            Color32[] c = new Color32[source.colors32.Length];

            Array.Copy(source.vertices, v, v.Length);

            for (int i = 0; i < t.Length; i++)
                t[i] = source.GetTriangles(i);

            Array.Copy(source.uv, u, u.Length);
            Array.Copy(source.uv2, u2, u2.Length);
            Array.Copy(source.normals, n, n.Length);
            Array.Copy(source.tangents, tan, tan.Length);
            Array.Copy(source.colors32, c, c.Length);

            destination.Clear();
            destination.name = source.name;

            destination.vertices = v;

            destination.subMeshCount = t.Length;

            for (int i = 0; i < t.Length; i++)
                destination.SetTriangles(t[i], i);

            destination.uv = u;
            destination.uv2 = u2;
            destination.tangents = tan;
            destination.normals = n;
            destination.colors32 = c;
        }
    }

}