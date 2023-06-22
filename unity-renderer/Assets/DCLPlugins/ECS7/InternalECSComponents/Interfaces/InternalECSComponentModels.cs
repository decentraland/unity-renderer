using DCL.Components.Video.Plugin;
using DCL.ECSComponents;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RaycastHit = DCL.ECSComponents.RaycastHit;

// TODO: Which internal properties can be turned to READONLY ???
// TODO: Check every model constructor is being called correctly

namespace DCL.ECS7.InternalComponents
{
    public interface InternalComponent
    {
        bool dirty { get; set; }
    }

    public struct InternalTexturizable : InternalComponent
    {
        public bool dirty { get; set; }
        public IList<Renderer> renderers;

        public InternalTexturizable(List<Renderer> initialRenderers)
        {
            this.dirty = false;
            this.renderers = initialRenderers;
        }
    }

    public struct InternalMaterial : InternalComponent
    {
        public bool dirty { get; set; }
        public Material material;
        public bool castShadows;

        public InternalMaterial(Material material, bool castShadows)
        {
            this.dirty = false;
            this.material = material;
            this.castShadows = castShadows;
        }
    }

    public struct InternalVideoMaterial : InternalComponent
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

        public bool dirty { get; set; }
        public Material material;
        public IList<VideoTextureData> videoTextureDatas;

        public InternalVideoMaterial(Material material, IList<VideoTextureData> videoTextureDatas)
        {
            this.dirty = false;
            this.material = material;
            this.videoTextureDatas = videoTextureDatas;
        }
    }

    public struct InternalVideoPlayer : InternalComponent
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

        public bool dirty { get; set; }
        public bool removed;
        public WebVideoPlayer videoPlayer;
        public IList<MaterialAssigned> assignedMaterials;

        // public InternalVideoPlayer(WebVideoPlayer videoPlayer, IList<MaterialAssigned> assignedMaterials)
        // {
        //     this.dirty = false;
        //     this.removed = false;
        //     this.videoPlayer = videoPlayer;
        //     this.assignedMaterials = assignedMaterials;
        // }
    }

    public struct InternalColliders : InternalComponent
    {
        public bool dirty { get; set; }
        public KeyValueSet<Collider, uint> colliders;

        public InternalColliders(KeyValueSet<Collider, uint> colliders)
        {
            this.dirty = false;
            this.colliders = colliders;
        }
    }

    public struct InternalRenderers : InternalComponent
    {
        public bool dirty { get; set; }
        public IList<Renderer> renderers;

        public InternalRenderers(IList<Renderer> renderers)
        {
            this.dirty = false;
            this.renderers = renderers;
        }
    }

    public struct InternalAudioSource : InternalComponent
    {
        public bool dirty { get; set; }
        public AudioSource audioSource;

        // public InternalAudioSource(AudioSource audioSource = null)
        // {
        //     this.dirty = false;
        //     this.audioSource = audioSource;
        // }
    }

    public struct InternalSceneBoundsCheck : InternalComponent
    {
        public bool dirty { get; set; }
        public Vector3 entityPosition;
        public Bounds entityLocalMeshBounds;
        public bool meshesDirty;
        public IList<Renderer> renderers;
        public KeyValueSet<Collider, uint> physicsColliders;
        public KeyValueSet<Collider, uint> pointerColliders;
        public Action<bool> OnSceneBoundsStateChange;

        public InternalSceneBoundsCheck(Bounds entityLocalMeshBounds)
        {
            this.dirty = false;
            this.entityPosition = Vector3.zero;
            this.entityLocalMeshBounds = entityLocalMeshBounds;
            this.meshesDirty = false;
            this.renderers = null;
            this.physicsColliders = null;
            this.pointerColliders = null;
            this.OnSceneBoundsStateChange = null;
        }
    }

    public struct InternalVisibility : InternalComponent
    {
        public bool dirty { get; set; }
        public bool visible;

        public InternalVisibility(bool visible)
        {
            this.dirty = false;
            this.visible = visible;
        }
    }

    public struct InternalInputEventResults : InternalComponent
    {
        public struct EventData
        {
            public InputAction button;
            public RaycastHit hit;
            public PointerEventType type;
        }

        public bool dirty { get; set; }
        public readonly IList<EventData> events;

        public InternalInputEventResults(List<EventData> events)
        {
            this.dirty = false;
            this.events = events;
        }
    }

    public struct InternalUiContainer : InternalComponent
    {
        public bool dirty { get; set; }
        public readonly VisualElement rootElement;
        public readonly HashSet<int> components;
        public VisualElement parentElement;
        public long parentId;
        public long rightOf;
        public bool shouldSort;

        public InternalUiContainer(long entityId)
        {
            this.dirty = false;
            this.components = new HashSet<int>();
            this.parentElement = null;
            this.parentId = -1;
            this.rightOf = -1;
            this.shouldSort = false;

            this.rootElement = new VisualElement();
            rootElement.name += $"(Id: {entityId})";
        }
    }

    public struct InternalPointerEvents : InternalComponent
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

        public bool dirty { get; set; }
        public readonly List<Entry> PointerEvents;

        public InternalPointerEvents(List<Entry> pointerEvents)
        {
            this.dirty = false;
            this.PointerEvents = pointerEvents;
        }
    }

    public struct InternalRegisteredUiPointerEvents : InternalComponent
    {
        public bool dirty { get; set; }
        public EventCallback<PointerDownEvent> OnPointerDownCallback;
        public EventCallback<PointerUpEvent> OnPointerUpCallback;
        public EventCallback<PointerEnterEvent> OnPointerEnterCallback;
        public EventCallback<PointerLeaveEvent> OnPointerLeaveCallback;

        // public InternalRegisteredUiPointerEvents(
        //     EventCallback<PointerDownEvent> onPointerDownCallback = null,
        //     EventCallback<PointerUpEvent> OnPointerUpCallback = null,
        //     EventCallback<PointerEnterEvent> OnPointerEnterCallback = null,
        //     EventCallback<PointerLeaveEvent> OnPointerLeaveCallback = null)
        // {
        //     this.dirty = false;
        //     this.OnPointerDownCallback = onPointerDownCallback;
        //     this.OnPointerUpCallback = OnPointerUpCallback;
        //     this.OnPointerEnterCallback = OnPointerEnterCallback;
        //     this.OnPointerLeaveCallback = OnPointerLeaveCallback;
        // }
    }

    public struct InternalRaycast : InternalComponent
    {
        public bool dirty { get; set; }
        public PBRaycast raycastModel;

        // public InternalRaycast(PBRaycast raycastModel)
        // {
        //     this.dirty = false;
        //     this.raycastModel = raycastModel;
        // }
    }

    public struct InternalGltfContainerLoadingState : InternalComponent
    {
        public bool dirty { get; set; }
        public LoadingState LoadingState;
        public bool GltfContainerRemoved;

        // public InternalGltfContainerLoadingState(LoadingState loadingState = LoadingState.Unknown, bool gltfContainerRemoved = false)
        // {
        //     this.dirty = false;
        //     this.LoadingState = loadingState;
        //     this.GltfContainerRemoved = gltfContainerRemoved;
        // }
    }

    public struct InternalEngineInfo : InternalComponent
    {
        public bool dirty { get; set; }
        public uint SceneTick;
        public float SceneInitialRunTime;
        public float SceneInitialFrameCount;

        public InternalEngineInfo(uint sceneTick, float sceneInitialRunTime)
        {
            this.dirty = false;
            this.SceneTick = sceneTick;
            this.SceneInitialRunTime = sceneInitialRunTime;
            this.SceneInitialFrameCount = Time.frameCount;
        }
    }

    public struct InternalUIInputResults : InternalComponent
    {
        public readonly struct Result
        {
            public readonly IMessage Message;
            public readonly int ComponentId;

            public Result(IMessage message, int componentId)
            {
                this.Message = message;
                this.ComponentId = componentId;
            }
        }

        public bool dirty { get; set; }
        public readonly Queue<Result> Results;

        public InternalUIInputResults(Queue<Result> results)
        {
            this.dirty = false;
            this.Results = results;
        }
    }
}
