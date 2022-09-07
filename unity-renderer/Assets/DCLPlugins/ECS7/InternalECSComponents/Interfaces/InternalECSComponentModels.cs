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

    public class InternalRenderers
    {
        public IList<Renderer> renderers = new List<Renderer>();
        public bool dirty = true;
    }
    public class InternalVisibility
    {
        public bool visible = true;
        public bool dirty = true;
    }
}