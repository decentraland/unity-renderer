using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// The Rendereable object represents any loaded object that should be visible in the world.
    /// 
    /// With this in place, the SceneBoundsChecker, CullingController and SceneMetricsCounter
    /// implementations can  be changed to be reactive, and lots of FindObjects and GetComponentsInChildren
    /// calls can be saved.
    /// </summary>
    public class Rendereable : ICloneable
    {
        public string ownerId;
        public GameObject container;
        public List<Mesh> meshes = new List<Mesh>();
        public Dictionary<Mesh, int> meshToTriangleCount = new Dictionary<Mesh, int>();
        public List<Renderer> renderers = new List<Renderer>();
        public List<Material> materials = new List<Material>();
        public List<Texture> textures = new List<Texture>();
        public int totalTriangleCount = 0;

        public bool Equals(Rendereable other)
        {
            return container == other.container;
        }

        public object Clone()
        {
            var result = (Rendereable)this.MemberwiseClone();
            result.meshToTriangleCount = new Dictionary<Mesh, int>(meshToTriangleCount);
            result.renderers = new List<Renderer>(renderers);
            result.materials = new List<Material>(materials);
            result.textures = new List<Texture>(textures);
            result.meshes = new List<Mesh>(meshes);
            return result;
        }

        private static List<int> texIdsCache = new List<int>();
        private static Shader hologramShader = null;

        public static Rendereable CreateFromGameObject(GameObject go)
        {
            if ( hologramShader == null )
                hologramShader = Shader.Find("DCL/FX/Hologram");

            Rendereable rendereable = new Rendereable();
            rendereable.container = go;
            rendereable.renderers = go.GetComponentsInChildren<Renderer>().ToList();

            // Get unique materials that are not holograms
            rendereable.materials = rendereable.renderers.SelectMany( (x) =>
                x.sharedMaterials.Where( (mat) => mat.shader != hologramShader )
            ).Distinct().ToList();

            // Get unique textures within the materials
            rendereable.textures = rendereable.materials.SelectMany( (mat) =>
            {
                mat.GetTexturePropertyNameIDs(texIdsCache);
                List<Texture> result = new List<Texture>();
                for ( int i = 0; i < texIdsCache.Count; i++ )
                {
                    result.Add( mat.GetTexture(texIdsCache[i]));
                }

                return result;
            }).Distinct().ToList();

            rendereable.meshes = MeshesInfoUtils.ExtractMeshes(go);
            rendereable.meshToTriangleCount = MeshesInfoUtils.ExtractMeshToTriangleMap(rendereable.meshes);
            rendereable.totalTriangleCount = MeshesInfoUtils.ComputeTotalTriangles(rendereable.renderers, rendereable.meshToTriangleCount);
            return rendereable;
        }
    }
}