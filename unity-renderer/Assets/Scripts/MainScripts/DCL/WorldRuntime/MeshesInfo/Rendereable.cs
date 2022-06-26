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
        public long ownerId;
        public GameObject container;
        public Dictionary<Mesh, int> meshToTriangleCount = new Dictionary<Mesh, int>();
        public HashSet<Mesh> meshes = new HashSet<Mesh>();
        public HashSet<Renderer> renderers = new HashSet<Renderer>();
        public HashSet<Material> materials = new HashSet<Material>();
        public HashSet<Texture> textures = new HashSet<Texture>();
        public HashSet<AnimationClip> animationClips = new HashSet<AnimationClip>();
        public int totalTriangleCount = 0;
        public long animationClipSize = 0;
        public long meshDataSize = 0;
        public bool isGLTFast = false;

        public bool Equals(Rendereable other)
        {
            return container == other.container;
        }

        public object Clone()
        {
            var result = (Rendereable) this.MemberwiseClone();
            result.meshToTriangleCount = new Dictionary<Mesh, int>(meshToTriangleCount);
            result.renderers = new HashSet<Renderer>(renderers);
            result.materials = new HashSet<Material>(materials);
            result.textures = new HashSet<Texture>(textures);
            result.meshes = new HashSet<Mesh>(meshes);
            return result;
        }

        public static Rendereable CreateFromGameObject(GameObject go)
        {
            Rendereable rendereable = new Rendereable();
            rendereable.container = go;
            rendereable.renderers = MeshesInfoUtils.ExtractUniqueRenderers(go);
            rendereable.materials = MeshesInfoUtils.ExtractUniqueMaterials(rendereable.renderers);
            rendereable.textures = MeshesInfoUtils.ExtractUniqueTextures(rendereable.materials);
            rendereable.meshes = MeshesInfoUtils.ExtractUniqueMeshes(rendereable.renderers);
            rendereable.meshToTriangleCount = MeshesInfoUtils.ExtractMeshToTriangleMap(rendereable.meshes.ToList());
            rendereable.totalTriangleCount =
                MeshesInfoUtils.ComputeTotalTriangles(rendereable.renderers, rendereable.meshToTriangleCount);

            return rendereable;
        }

        public override string ToString()
        {
            return
                $"Rendereable - owner: {ownerId} ... textures: {textures.Count} ... triangles: {totalTriangleCount} ... materials: {materials.Count} ... meshes: {meshes.Count} ... renderers: {renderers.Count}";
        }
    }
}