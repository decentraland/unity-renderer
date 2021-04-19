using System.Runtime.InteropServices;
using DCL.Interface;
using UnityEngine;

namespace DCL.Models
{
    public enum CLASS_ID_COMPONENT
    {
        NONE = 0,
        TRANSFORM = 1,
        UUID_CALLBACK = 8,
        UUID_ON_CLICK = 9,
        UUID_ON_DOWN = 10,
        UUID_ON_UP = 11,
        TEXT_SHAPE = 21,
        AVATAR_SHAPE = 56,
        ANIMATOR = 33,
        AUDIO_SOURCE = 201,
        GIZMOS = 203,
        BILLBOARD = 32,
        SMART_ITEM = 204,
        AUDIO_STREAM = 202,
        AVATAR_MODIFIER_AREA = 205,
        QUEST_TRACKING_INFORMATION = 1417815519 // This corresponds with dcl-ecs-quests
    }

    public enum CLASS_ID
    {
        BOX_SHAPE = 16,
        SPHERE_SHAPE = 17,
        PLANE_SHAPE = 18,
        CONE_SHAPE = 19,
        CYLINDER_SHAPE = 20,
        NFT_SHAPE = 22,

        UI_WORLD_SPACE_SHAPE = 23,
        UI_SCREEN_SPACE_SHAPE = 24,
        UI_CONTAINER_RECT = 25,
        UI_CONTAINER_STACK = 26,
        UI_TEXT_SHAPE = 27,
        UI_INPUT_TEXT_SHAPE = 28,
        UI_IMAGE_SHAPE = 29,
        UI_SLIDER_SHAPE = 30,

        CIRCLE_SHAPE = 31,

        FONT = 34,

        UI_FULLSCREEN_SHAPE = 40,
        UI_BUTTON_SHAPE = 41,

        GLTF_SHAPE = 54,
        OBJ_SHAPE = 55,
        BASIC_MATERIAL = 64,
        PBR_MATERIAL = 65,

        HIGHLIGHT_ENTITY = 66,

        TEXTURE = 68,

        VIDEO_CLIP = 70,
        VIDEO_TEXTURE = 71,

        AUDIO_CLIP = 200,

        NAME = 300,
        LOCKED_ON_EDIT = 301,
        VISIBLE_ON_EDIT = 302
    }

    public static class Protocol
    {
        [System.Serializable]
        public struct SceneReady { }

        [System.Serializable]
        public struct CreateEntity
        {
            public string entityId;

            public static CreateEntity FromPB(PB_CreateEntity pbPayload) { return new CreateEntity() { entityId = pbPayload.Id }; }
        }

        [System.Serializable]
        public struct RemoveEntity
        {
            public string entityId;

            public static RemoveEntity FromPB(PB_RemoveEntity pbPayload) { return new RemoveEntity() { entityId = pbPayload.Id }; }
        }

        [System.Serializable]
        public struct SetEntityParent
        {
            public string entityId;
            public string parentId;

            public static SetEntityParent FromPB(PB_SetEntityParent pbPayload) { return new SetEntityParent() { entityId = pbPayload.EntityId, parentId = pbPayload.ParentId }; }
        }

        [System.Serializable]
        public struct EntityComponentCreateOrUpdate
        {
            public string entityId;
            public int classId;
            public string json;

            public static EntityComponentCreateOrUpdate FromPB(PB_UpdateEntityComponent pbPayload) { return new EntityComponentCreateOrUpdate() { entityId = pbPayload.EntityId, classId = pbPayload.ClassId, json = pbPayload.Data }; }
        }

        [System.Serializable]
        public struct EntityComponentDestroy
        {
            public string entityId;
            public string name;

            public static EntityComponentDestroy FromPB(PB_ComponentRemoved pbPayload) { return new EntityComponentDestroy() { entityId = pbPayload.EntityId, name = pbPayload.Name }; }
        }

        [System.Serializable]
        public struct SharedComponentAttach
        {
            public string entityId;
            public string id;
            public string name;

            public static SharedComponentAttach FromPB(PB_AttachEntityComponent pbPayload) { return new SharedComponentAttach() { entityId = pbPayload.EntityId, id = pbPayload.Id, name = pbPayload.Name }; }
        }

        [System.Serializable]
        public struct SharedComponentCreate
        {
            public string id;
            public int classId;
            public string name;

            public static SharedComponentCreate FromPB(PB_ComponentCreated pbPayload) { return new SharedComponentCreate() { id = pbPayload.Id, classId = pbPayload.Classid, name = pbPayload.Name }; }
        }

        [System.Serializable]
        public struct SharedComponentDispose
        {
            public string id;

            public static SharedComponentDispose FromPB(PB_ComponentDisposed pbPayload) { return new SharedComponentDispose() { id = pbPayload.Id }; }
        }

        [System.Serializable]
        public struct SharedComponentUpdate
        {
            public string componentId;
            public string json;

            public static SharedComponentUpdate FromPB(PB_ComponentUpdated pbPayload) { return new SharedComponentUpdate() { componentId = pbPayload.Id, json = pbPayload.Json }; }
        }

        [System.Serializable]
        public struct ParcelSceneLoad { }

        [System.Serializable]
        public struct ParcelSceneUpdate { }

        [System.Serializable]
        public struct ParcelSceneUnload { }

        [System.Serializable]
        public struct OpenExternalUrl
        {
            public string url;

            public static OpenExternalUrl FromPB(PB_OpenExternalUrl pbPayload) { return new OpenExternalUrl() { url = pbPayload.Url }; }
        }

        [System.Serializable]
        public struct OpenNftDialog
        {
            public string contactAddress;
            public string comment;
            public string tokenId;

            public static OpenNftDialog FromPB(PB_OpenNFTDialog pbPayload) { return new OpenNftDialog() { contactAddress = pbPayload.AssetContractAddress, comment = pbPayload.Comment, tokenId = pbPayload.TokenId }; }
        }

        [System.Serializable]
        public struct QueryPayload
        {
            public int queryType;
            public RaycastQueryPayload raycastPayload;
        }

        [System.Serializable]
        public struct RaycastQueryPayload
        {
            public int id;
            public int raycastType;
            public Vector3 origin;
            public Vector3 direction;
            public float distance;
        }

        public static string RaycastTypeToLiteral(Models.RaycastType raycastType)
        {
            switch (raycastType)
            {
                case RaycastType.HIT_FIRST:
                    return "HitFirst";
                case RaycastType.HIT_ALL:
                    return "HitAll";
                case RaycastType.HIT_FIRST_AVATAR:
                    return "HitFirstAvatar";
                case RaycastType.HIT_ALL_AVATARS:
                    return "HitAllAvatars";
                default:
                    return "";
            }
        }

        public static RaycastType RaycastLiteralToType(string literal)
        {
            switch (literal)
            {
                case "HitFirst":
                    return RaycastType.HIT_FIRST;
                case "HitAll":
                    return RaycastType.HIT_ALL;
                case "HitFirstAvatar":
                    return RaycastType.HIT_FIRST_AVATAR;
                case "HitAllAvatars":
                    return RaycastType.HIT_ALL_AVATARS;
                default:
                    return RaycastType.NONE;
            }
        }
    }

    [System.Serializable]
    public class UUIDCallbackMessage
    {
        /// ID of the event to trigger
        public string uuid;

        /// type of the event
        public string type;

        public void FromJSON(string rawJson)
        {
            uuid = default(string);
            type = default(string);

            JsonUtility.FromJsonOverwrite(rawJson, this);
        }
    }

    //-----------------------------------------------------
    // Raycast
    [System.Serializable]
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;

        [System.NonSerialized] public Vector3 unityOrigin;
    }

    public enum RaycastType
    {
        NONE = 0,
        HIT_FIRST = 1,
        HIT_ALL = 2,
        HIT_FIRST_AVATAR = 3,
        HIT_ALL_AVATARS = 4
    }

    [System.Serializable]
    public class RaycastQuery
    {
        public string sceneId;
        public string id;
        public RaycastType raycastType;
        public Ray ray;
    }

    [System.Serializable]
    public class QueryMessage
    {
        public string queryType;
        public RaycastQuery payload;

        public void FromJSON(string rawJson)
        {
            queryType = default(string);
            JsonUtility.FromJsonOverwrite(rawJson, this);
        }
    }
}