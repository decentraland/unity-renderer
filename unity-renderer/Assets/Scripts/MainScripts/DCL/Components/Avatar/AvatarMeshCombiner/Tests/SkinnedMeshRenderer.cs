using DCL.Helpers;
using DCL.Shaders;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Helpers
{
    public static class SkinnedMeshRenderer
    {
        public static UnityEngine.SkinnedMeshRenderer Create(UnityEngine.Material mat)
        {
            return Create( new UnityEngine.Material[] { mat });
        }

        public static UnityEngine.SkinnedMeshRenderer Create(UnityEngine.Material[] mats)
        {
            GameObject tmp_go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = Object.Instantiate(tmp_go.GetComponent<MeshFilter>().sharedMesh);
            Object.Destroy(tmp_go);

            GameObject go = new GameObject();
            var skr = go.AddComponent<UnityEngine.SkinnedMeshRenderer>();
            skr.sharedMesh = mesh;
            skr.sharedMaterials = mats;

            return skr;
        }

        public static UnityEngine.SkinnedMeshRenderer CreateWithOpaqueMat(CullMode cullMode = CullMode.Back, Texture2D albedo = null, Texture2D emission = null)
        {
            var skr = Create(Material.CreateOpaque(cullMode, albedo, emission));
            return skr;
        }

        public static UnityEngine.SkinnedMeshRenderer CreateWithTransparentMat(CullMode cullMode = CullMode.Back, Texture2D albedo = null, Texture2D emission = null)
        {
            var skr = Create(Material.CreateTransparent(cullMode, albedo, emission));
            return skr;
        }

        public static void DestroyAndUnload(UnityEngine.SkinnedMeshRenderer r)
        {
            foreach ( var m in r.sharedMaterials )
            {
                Object.Destroy( m.GetTexture(ShaderUtils.BaseMap));
                Object.Destroy( m.GetTexture(ShaderUtils.EmissionMap));
                Object.Destroy(m);
            }

            Object.Destroy(r.sharedMesh);
            Object.Destroy(r.gameObject);
        }
    }
}