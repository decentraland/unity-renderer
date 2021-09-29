using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// The Rendereable object represents any loaded object that should be visible in the world.
    /// 
    /// In the future, we may want to add a Renderer[] list here.
    ///
    /// With this in place, the SceneBoundsChecker and CullingController implementations can
    /// be changed to be reactive, and lots of FindObjects and GetComponentsInChildren calls can be
    /// saved.
    /// </summary>
    public class Rendereable : ICloneable
    {
        public GameObject container;
        public List<Mesh> meshes = new List<Mesh>();
        public Dictionary<Mesh, int> meshToTriangleCount = new Dictionary<Mesh, int>();
        public List<Renderer> renderers = new List<Renderer>();
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
            result.meshes = new List<Mesh>(meshes);
            return result;
        }
    }
}