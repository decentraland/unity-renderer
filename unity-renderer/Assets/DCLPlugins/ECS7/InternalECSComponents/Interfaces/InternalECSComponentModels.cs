using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public class InternalTexturizable
    {
        public IList<Renderer> renderers = new List<Renderer>();
    }

    public class InternalMaterial
    {
        public Material material = null;
    }
}