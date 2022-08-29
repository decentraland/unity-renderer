using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public class InternalTexturizable
    {
        public IList<Renderer> renderers = new List<Renderer>();
        public bool dirty = true;
    }

    public class InternalMaterial
    {
        public Material material = null;
        public bool castShadows = true;
        public IList<Renderer> renderers = null;
        public bool dirty = true;
    }

    public class InternalColliders
    {
        public IList<Collider> colliders = new List<Collider>();
    }
}