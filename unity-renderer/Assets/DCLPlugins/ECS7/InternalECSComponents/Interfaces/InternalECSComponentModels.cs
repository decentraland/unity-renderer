using System.Collections.Generic;
using DCL.ECSComponents;
using UnityEngine;
using UnityEngine.UIElements;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace DCL.ECS7.InternalComponents
{
    public class InternalComponent
    {
        public bool dirty => _dirty;
        internal bool _dirty = true;
    }

    public class InternalTexturizable : InternalComponent
    {
        public IList<Renderer> renderers = new List<Renderer>();
    }

    public class InternalMaterial : InternalComponent
    {
        public Material material = null;
        public bool castShadows = true;
    }

    public class InternalColliders : InternalComponent
    {
        public IList<Collider> colliders = new List<Collider>();
    }

    public class InternalRenderers : InternalComponent
    {
        public IList<Renderer> renderers = new List<Renderer>();
    }

    public class InternalAudioSource : InternalComponent
    {
        public AudioSource audioSource;
    }

    public class InternalSceneBoundsCheck : InternalComponent
    {
        public Vector3 entityPosition = Vector3.zero;
        public Bounds entityLocalMeshBounds = new Bounds();
        public bool meshesDirty = false;
        public IList<Renderer> renderers;
        public IList<Collider> physicsColliders;
        public IList<Collider> pointerColliders;
        public AudioSource audioSource;
    }

    public class InternalVisibility : InternalComponent
    {
        public bool visible = true;
    }

    public class InternalInputEventResults : InternalComponent
    {
        public class EventData
        {
            public InputAction button;
            public RaycastHit hit;
            public PointerEventType type;
            public int timestamp;
        }

        public Queue<EventData> events;
        public int lastTimestamp;
    }

    public class InternalUiContainer : InternalComponent
    {
        public readonly VisualElement rootElement = new VisualElement();
        public readonly HashSet<int> components = new HashSet<int>();
        public VisualElement parentElement;
        public long parentId;
        public long rigthOf;
        public bool shouldSort;

        public InternalUiContainer(long entityId)
        {
            rootElement.name += $"(Id: {entityId})";
        }
    }
}
