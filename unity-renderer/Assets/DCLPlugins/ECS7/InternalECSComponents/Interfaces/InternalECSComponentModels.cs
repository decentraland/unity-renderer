using DCL.Components.Video.Plugin;
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

    public class InternalVideoMaterial : InternalComponent
    {
        public readonly struct VideoTextureData
        {
            public readonly long videoId;
            public readonly int textureType;

            public VideoTextureData(long videoId, int textureType)
            {
                this.videoId = videoId;
                this.textureType = textureType;
            }
        }

        public Material material = null;
        public IList<VideoTextureData> videoTextureDatas;
    }

    public class InternalVideoPlayer : InternalComponent
    {
        public readonly struct MaterialAssigned
        {
            public readonly Material material;
            public readonly int textureType;

            public MaterialAssigned(Material material, int textureType)
            {
                this.material = material;
                this.textureType = textureType;
            }
        }

        public WebVideoPlayer videoPlayer = null;
        public IList<MaterialAssigned> assignedMaterials;
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
        public long lastEntity;
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
