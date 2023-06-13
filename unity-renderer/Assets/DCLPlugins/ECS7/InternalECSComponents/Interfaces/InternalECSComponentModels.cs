using DCL.Components.Video.Plugin;
using DCL.ECSComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace DCL.ECS7.InternalComponents
{
    public class InternalComponent
    {
        public bool dirty => _dirty;
        internal bool _dirty = false;
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
        public bool removed = false;
    }

    public class InternalColliders : InternalComponent
    {
        public KeyValueSet<Collider, uint> colliders = new KeyValueSet<Collider, uint>();
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
        public KeyValueSet<Collider, uint> physicsColliders;
        public KeyValueSet<Collider, uint> pointerColliders;
        public Action<bool> OnSceneBoundsStateChange;
    }

    public class InternalVisibility : InternalComponent
    {
        public bool visible = true;
    }

    public class InternalInputEventResults : InternalComponent
    {
        public struct EventData
        {
            public InputAction button;
            public RaycastHit hit;
            public PointerEventType type;
        }

        public readonly IList<EventData> events = new List<EventData>(20);
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

    public class InternalPointerEvents : InternalComponent
    {
        public readonly struct Entry
        {
            public readonly PointerEventType EventType;
            public readonly Info EventInfo;

            public Entry(PointerEventType eventType, Info eventInfo)
            {
                EventType = eventType;
                EventInfo = eventInfo;
            }
        }

        public readonly struct Info
        {
            public readonly InputAction Button;
            public readonly string HoverText;
            public readonly float MaxDistance;
            public readonly bool ShowFeedback;

            public Info(InputAction button, string hoverText, float maxDistance, bool showFeedback)
            {
                Button = button;
                HoverText = hoverText;
                MaxDistance = maxDistance;
                ShowFeedback = showFeedback;
            }
        }

        public readonly List<Entry> PointerEvents = new List<Entry>();
    }

    public class InternalRegisteredUiPointerEvents : InternalComponent
    {
        public EventCallback<PointerDownEvent> OnPointerDownCallback;
        public EventCallback<PointerUpEvent> OnPointerUpCallback;
        public EventCallback<PointerEnterEvent> OnPointerEnterCallback;
        public EventCallback<PointerLeaveEvent> OnPointerLeaveCallback;
    }

    public class InternalRaycast : InternalComponent
    {
        public PBRaycast raycastModel;
    }

    public class InternalGltfContainerLoadingState : InternalComponent
    {
        public LoadingState LoadingState;
        public bool GltfContainerRemoved;
    }

    public class InternalEngineInfo : InternalComponent
    {
        public uint SceneTick = 0;
        public float SceneInitialRunTime = 0;
        public float SceneInitialFrameCount = Time.frameCount;
    }
}
