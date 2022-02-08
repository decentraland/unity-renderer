﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Models
{
    public static class MeshesInfoUtils
    {
        public static Bounds BuildMergedBounds(Renderer[] renderers)
        {
            Bounds bounds = new Bounds();

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                if (i == 0)
                    bounds = renderers[i].GetSafeBounds();
                else
                    bounds.Encapsulate(renderers[i].GetSafeBounds());
            }

            return bounds;
        }

        /// <summary>
        /// This get the renderer bounds with a check to ensure the renderer is at a safe position.
        /// If the renderer is too far away from 0,0,0, wasm target ensures a crash.
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns>The bounds value if the value is correct, or a mocked bounds object with clamped values if its too far away.</returns>
        public static Bounds GetSafeBounds( this Renderer renderer )
        {
            // World extents are of 4800 world mts, so this limit far exceeds the world size.
            const float POSITION_OVERFLOW_LIMIT = 10000;
            const float POSITION_OVERFLOW_LIMIT_SQR = POSITION_OVERFLOW_LIMIT * POSITION_OVERFLOW_LIMIT;

            if ( renderer.transform.position.sqrMagnitude > POSITION_OVERFLOW_LIMIT_SQR )
                return new Bounds( Vector3.one * POSITION_OVERFLOW_LIMIT, Vector3.one * 0.1f );

            return renderer.bounds;
        }

        public static int ComputeTotalTriangles(HashSet<Renderer> renderers, Dictionary<Mesh, int> meshToTriangleCount)
        {
            int result = 0;

            foreach ( var renderer in renderers )
            {
                switch (renderer)
                {
                    case MeshRenderer r:
                        MeshFilter mf = r.GetComponent<MeshFilter>();

                        if ( mf == null )
                            continue;

                        int triangles = meshToTriangleCount[ mf.sharedMesh ];
                        result += triangles;
                        break;
                    case SkinnedMeshRenderer skr:
                        result += meshToTriangleCount[ skr.sharedMesh ];
                        break;
                }
            }

            return result;
        }

        public static Dictionary<Mesh, int> ExtractMeshToTriangleMap(List<Mesh> meshes)
        {
            Dictionary<Mesh, int> result = new Dictionary<Mesh, int>();

            for ( int i = 0; i < meshes.Count; i++ )
            {
                Mesh mesh = meshes[i];
                result[mesh] = mesh.triangles.Length;
            }

            return result;
        }

        private static Shader hologramShader = null;
        private static List<int> texIdsCache = new List<int>();
        private static List<string> texNameCache = new List<string>();

        public static HashSet<Renderer> ExtractUniqueRenderers(GameObject container)
        {
            return new HashSet<Renderer>(container.GetComponentsInChildren<Renderer>(true));
        }

        public static HashSet<Material> ExtractUniqueMaterials(HashSet<Renderer> renderers)
        {
            if ( hologramShader == null )
                hologramShader = Shader.Find("DCL/FX/Hologram");

            return new HashSet<Material>( renderers.SelectMany( (x) =>
                x.sharedMaterials.Where( (mat) => mat != null && mat.shader != hologramShader )
            ) );
        }

        public static HashSet<Texture> ExtractUniqueTextures(HashSet<Material> materials)
        {
            return new HashSet<Texture>(
                materials.SelectMany(
                    (mat) =>
                    {
                        mat.GetTexturePropertyNameIDs(texIdsCache);
                        mat.GetTexturePropertyNames(texNameCache);
                        List<Texture> result = new List<Texture>();
                        for ( int i = 0; i < texIdsCache.Count; i++ )
                        {
                            var tex = mat.GetTexture(texIdsCache[i]);

                            if ( tex != null )
                            {
                                result.Add(tex);
                            }
                        }

                        return result;
                    } ) );
        }

        public static HashSet<Mesh> ExtractUniqueMeshes(HashSet<Renderer> renderers)
        {
            List<Mesh> result = new List<Mesh>();

            foreach ( Renderer renderer in renderers )
            {
                switch ( renderer )
                {
                    case SkinnedMeshRenderer skr:
                        if ( skr.sharedMesh == null )
                            continue;

                        result.Add(skr.sharedMesh);
                        break;
                    case MeshRenderer mr:
                        MeshFilter mf = mr.GetComponent<MeshFilter>();

                        if ( mf.mesh == null )
                            continue;

                        result.Add( mf.mesh );
                        break;
                }
            }

            // Ensure meshes are unique
            return new HashSet<Mesh>(result);
        }
    }
}