using System.Collections.Generic;
using DCL.ECSComponents;
using UnityEngine;
using RaycastHit = DCL.ECSComponents.RaycastHit;

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

    public class InternalInputEventResults
    {
        public class EventData
        {
            public ActionButton button;
            public RaycastHit hit;
            public PointerEventType type;
            public int timestamp;
            public float analog;
        }

        public Queue<EventData> events;
        public bool dirty;
        public int lastTimestamp;
    }    
}
