using System;
using System.Runtime.InteropServices;
using DCL;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;
using System.IO;
using UnityEngine;
using Ray = DCL.Models.Ray;

public class NativeBridgeCommunication : IKernelCommunication
{
    private static string currentEntityId;
    private static int currentSceneNumber;
    private static string currentTag;
    private static byte[] preallocatedReaderBuffer = new byte[88388608];

    private static IMessageQueueHandler queueHandler;

    delegate void JS_Delegate_VI(int a);

    delegate void JS_Delegate_VIS(int a, string b);

    delegate void JS_Delegate_VSS(string a, string b);

    delegate void JS_Delegate_VSSS(string a, string b, string c);

    delegate void JS_Delegate_VS(string a);

    delegate void JS_Delegate_Query(Protocol.QueryPayload a);

    delegate void JS_Delegate_V();

    public NativeBridgeCommunication(IMessageQueueHandler queueHandler)
    {
        NativeBridgeCommunication.queueHandler = queueHandler;
#if UNITY_WEBGL && !UNITY_EDITOR
        SetCallback_CreateEntity(CreateEntity);
        SetCallback_RemoveEntity(RemoveEntity);
        SetCallback_SceneReady(SceneReady);

        SetCallback_SetEntityId(SetEntityId);
        // @deprecated use SetSceneNumber
        SetCallback_SetSceneId(SetSceneId);
        SetCallback_SetSceneNumber(SetSceneNumber);
        SetCallback_SetTag(SetTag);

        SetCallback_SetEntityParent(SetEntityParent);

        SetCallback_EntityComponentCreateOrUpdate(EntityComponentCreateOrUpdate);
        SetCallback_EntityComponentDestroy(EntityComponentDestroy);

        SetCallback_SharedComponentCreate(SharedComponentCreate);
        SetCallback_SharedComponentAttach(SharedComponentAttach);
        SetCallback_SharedComponentUpdate(SharedComponentUpdate);
        SetCallback_SharedComponentDispose(SharedComponentDispose);

        SetCallback_OpenExternalUrl(OpenExternalUrl);
        SetCallback_OpenNftDialog(OpenNftDialog);

        SetCallback_Query(Query);
        SetCallback_Sdk6BinaryMessage(Sdk6BinaryMessage);
#endif
    }
    public void Dispose()
    {

    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VSSS))]
    internal static void OpenNftDialog(string contactAddress, string comment, string tokenId)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.OpenNftDialog payload = new Protocol.OpenNftDialog
        {
            contactAddress = contactAddress,
            comment = comment,
            tokenId = tokenId
        };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.OPEN_NFT_DIALOG;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void OpenExternalUrl(string url)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.OpenExternalUrl payload = new Protocol.OpenExternalUrl
        {
            url = url
        };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.OPEN_EXTERNAL_URL;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void EntityComponentDestroy(string name)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.EntityComponentDestroy payload = new Protocol.EntityComponentDestroy
        {
            entityId = currentEntityId,
            name = name
        };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.ENTITY_COMPONENT_DESTROY;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VSS))]
    internal static void SharedComponentAttach(string id, string name)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.SharedComponentAttach payload = new Protocol.SharedComponentAttach
        {
            entityId = currentEntityId,
            id = id,
            name = name
        };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.SHARED_COMPONENT_ATTACH;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_Query))]
    internal static void Query(Protocol.QueryPayload payload)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        string queryId = Convert.ToString(payload.raycastPayload.id);

        RaycastType raycastType = (RaycastType) payload.raycastPayload.raycastType;

        Ray ray = new Ray()
        {
            origin = payload.raycastPayload.origin,
            direction = payload.raycastPayload.direction,
            distance = payload.raycastPayload.distance
        };

        queuedMessage.method = MessagingTypes.QUERY;
        queuedMessage.payload = new QueryMessage()
        {
            payload = new RaycastQuery()
            {
                id = queryId,
                raycastType = raycastType,
                ray = ray,
                sceneNumber = currentSceneNumber
            }
        };

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VSS))]
    internal static void SharedComponentUpdate(string id, string json)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.SharedComponentUpdate payload =
            new Protocol.SharedComponentUpdate
            {
                componentId = id,
                json = json
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.SHARED_COMPONENT_UPDATE;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SharedComponentDispose(string id)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.SharedComponentDispose payload =
            new Protocol.SharedComponentDispose
            {
                id = id
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.SHARED_COMPONENT_DISPOSE;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VIS))]
    internal static void SharedComponentCreate(int classId, string id)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.SharedComponentCreate payload =
            new Protocol.SharedComponentCreate
            {
                id = id,
                classId = classId
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.SHARED_COMPONENT_CREATE;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VIS))]
    internal static void EntityComponentCreateOrUpdate(int classId, string json)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.EntityComponentCreateOrUpdate payload =
            new Protocol.EntityComponentCreateOrUpdate
            {
                entityId = currentEntityId,
                classId = classId,
                json = json
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetEntityParent(string parentId)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.SetEntityParent payload =
            new Protocol.SetEntityParent
            {
                entityId = currentEntityId,
                parentId = parentId
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.ENTITY_REPARENT;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetEntityId(string id) { currentEntityId = id; }

    // @deprecated use SetSceneNumber
    [MonoPInvokeCallback(typeof(JS_Delegate_VI))]
    internal static void SetSceneId(string _) { }

    [MonoPInvokeCallback(typeof(JS_Delegate_VI))]
    internal static void SetSceneNumber(int sceneNumber) { currentSceneNumber = sceneNumber; }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetTag(string id) { currentTag = id; }

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void CreateEntity()
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();

        Protocol.CreateEntity payload =
            new Protocol.CreateEntity
            {
                entityId = currentEntityId
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.ENTITY_CREATE;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void RemoveEntity()
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        Protocol.RemoveEntity payload =
            new Protocol.RemoveEntity()
            {
                entityId = currentEntityId
            };

        queuedMessage.payload = payload;
        queuedMessage.method = MessagingTypes.ENTITY_DESTROY;

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void SceneReady()
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        queuedMessage.method = MessagingTypes.INIT_DONE;
        queuedMessage.payload = new Protocol.SceneReady();

        queueHandler.EnqueueSceneMessage(queuedMessage);
    }

    internal static QueuedSceneMessage_Scene GetSceneMessageInstance()
    {
        var sceneMessagesPool = queueHandler.sceneMessagesPool;

        if (!sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene message))
        {
            message = new QueuedSceneMessage_Scene();
        }

        message.sceneNumber = currentSceneNumber;
        message.tag = currentTag;
        message.type = QueuedSceneMessage.Type.SCENE_MESSAGE;

        return message;
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VII))]
    internal static unsafe void Sdk6BinaryMessage(int intPtr, int length)
    {
        IntPtr ptr = new IntPtr(intPtr);
        
        var readonlySpan = new ReadOnlySpan<byte>(ptr.ToPointer(), length);
        readonlySpan.CopyTo(preallocatedReaderBuffer);

        try
        {
            RendererManyEntityActions sceneRequest = RendererManyEntityActions.Parser.ParseFrom(preallocatedReaderBuffer, 0, length);
            foreach(var action in sceneRequest.Actions)
                queueHandler.EnqueueSceneMessage(new QueuedSceneMessage_Scene
                {
                    type = QueuedSceneMessage.Type.SCENE_MESSAGE,
                    method = MapMessagingMethodType(action),
                    sceneNumber = currentSceneNumber,
                    payload = ExtractPayload(from: action),
                    tag = action.Tag,
                });
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }


    private static object ExtractPayload(EntityAction from)
    {
        return from.Payload.PayloadCase switch
               {
                   EntityActionPayload.PayloadOneofCase.InitMessagesFinished => new Protocol.SceneReady(),
                   EntityActionPayload.PayloadOneofCase.OpenExternalUrl => new Protocol.OpenExternalUrl { url = from.Payload.OpenExternalUrl.Url },
                   EntityActionPayload.PayloadOneofCase.OpenNftDialog => new Protocol.OpenNftDialog
                   {
                       contactAddress = from.Payload.OpenNftDialog.AssetContractAddress,
                       comment = from.Payload.OpenNftDialog.Comment,
                       tokenId = from.Payload.OpenNftDialog.TokenId
                   },
                   EntityActionPayload.PayloadOneofCase.CreateEntity => new Protocol.CreateEntity { entityId = from.Payload.CreateEntity.Id },
                   EntityActionPayload.PayloadOneofCase.RemoveEntity => new Protocol.RemoveEntity { entityId = from.Payload.RemoveEntity.Id },
                   EntityActionPayload.PayloadOneofCase.AttachEntityComponent => new Protocol.SharedComponentAttach
                   {
                       entityId = from.Payload.AttachEntityComponent.EntityId,
                       id = from.Payload.AttachEntityComponent.Id,
                       name = from.Payload.AttachEntityComponent.Name
                   },
                   EntityActionPayload.PayloadOneofCase.ComponentRemoved => new Protocol.EntityComponentDestroy()
                   {
                       entityId = from.Payload.ComponentRemoved.EntityId,
                       name = from.Payload.ComponentRemoved.Name
                   },
                   EntityActionPayload.PayloadOneofCase.SetEntityParent => new Protocol.SetEntityParent()
                   {
                       entityId = from.Payload.SetEntityParent.EntityId,
                       parentId = from.Payload.SetEntityParent.ParentId
                   },
                   EntityActionPayload.PayloadOneofCase.Query => new QueryMessage { payload = CreateRaycastPayload(from) },
                   EntityActionPayload.PayloadOneofCase.ComponentCreated => new Protocol.SharedComponentCreate
                   {
                       id = from.Payload.ComponentCreated.Id,
                       classId = from.Payload.ComponentCreated.ClassId,
                       name = from.Payload.ComponentCreated.Name,
                   },
                   EntityActionPayload.PayloadOneofCase.ComponentDisposed => new Protocol.SharedComponentDispose { id = from.Payload.ComponentDisposed.Id },

                   //--- NEW FLOW!
                   EntityActionPayload.PayloadOneofCase.ComponentUpdated => from.Payload.ComponentUpdated,
                   EntityActionPayload.PayloadOneofCase.UpdateEntityComponent => from.Payload.UpdateEntityComponent,

                   EntityActionPayload.PayloadOneofCase.None => null,
                   _ => throw new ArgumentOutOfRangeException(),
               };
    }

    private static RaycastQuery CreateRaycastPayload(EntityAction action)
    {
        var raycastType = action.Payload.Query.Payload.QueryType switch
            {
                "HitFirst" => RaycastType.HIT_FIRST,
                "HitAll" => RaycastType.HIT_ALL,
                "HitFirstAvatar" => RaycastType.HIT_FIRST_AVATAR,
                "HitAllAvatars" => RaycastType.HIT_ALL_AVATARS,
                _ => RaycastType.NONE,
            };

        var ray = new Ray
        {
            origin = action.Payload.Query.Payload.Ray.Origin.AsUnityVector3(),
            direction =  action.Payload.Query.Payload.Ray.Direction.AsUnityVector3(),
            distance = action.Payload.Query.Payload.Ray.Distance
        };

        return new RaycastQuery
        {
            id = action.Payload.Query.Payload.QueryId,
            raycastType = raycastType,
            ray = ray,
            sceneNumber = currentSceneNumber,
        };
    }

    private static string MapMessagingMethodType(EntityAction action) =>
        action.Payload.PayloadCase switch
        {
            EntityActionPayload.PayloadOneofCase.InitMessagesFinished => MessagingTypes.INIT_DONE,
            EntityActionPayload.PayloadOneofCase.OpenExternalUrl => MessagingTypes.OPEN_EXTERNAL_URL,
            EntityActionPayload.PayloadOneofCase.OpenNftDialog => MessagingTypes.OPEN_NFT_DIALOG,
            EntityActionPayload.PayloadOneofCase.CreateEntity => MessagingTypes.ENTITY_CREATE,
            EntityActionPayload.PayloadOneofCase.RemoveEntity => MessagingTypes.ENTITY_DESTROY,
            EntityActionPayload.PayloadOneofCase.AttachEntityComponent => MessagingTypes.SHARED_COMPONENT_ATTACH,
            EntityActionPayload.PayloadOneofCase.ComponentRemoved => MessagingTypes.ENTITY_COMPONENT_DESTROY,
            EntityActionPayload.PayloadOneofCase.SetEntityParent => MessagingTypes.ENTITY_REPARENT,
            EntityActionPayload.PayloadOneofCase.Query => MessagingTypes.QUERY,
            EntityActionPayload.PayloadOneofCase.ComponentCreated => MessagingTypes.SHARED_COMPONENT_CREATE,
            EntityActionPayload.PayloadOneofCase.ComponentDisposed => MessagingTypes.SHARED_COMPONENT_DISPOSE,
            EntityActionPayload.PayloadOneofCase.UpdateEntityComponent => MessagingTypes.PB_ENTITY_COMPONENT_CREATE_OR_UPDATE,  //--- NEW FLOW!
            EntityActionPayload.PayloadOneofCase.ComponentUpdated => MessagingTypes.PB_SHARED_COMPONENT_UPDATE,
            EntityActionPayload.PayloadOneofCase.None => null,
            _ => throw new ArgumentOutOfRangeException(),
        };



    [DllImport("__Internal")]
    private static extern void SetCallback_CreateEntity(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_RemoveEntity(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SceneReady(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetEntityId(JS_Delegate_VS callback);

    // @deprecated use SetSceneNumber
    [DllImport("__Internal")]
    private static extern void SetCallback_SetSceneId(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetSceneNumber(JS_Delegate_VI callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetEntityParent(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_EntityComponentCreateOrUpdate(JS_Delegate_VIS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SharedComponentAttach(JS_Delegate_VSS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_EntityComponentDestroy(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_OpenExternalUrl(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_OpenNftDialog(JS_Delegate_VSSS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SharedComponentUpdate(JS_Delegate_VSS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SharedComponentDispose(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SharedComponentCreate(JS_Delegate_VIS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetTag(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_Query(JS_Delegate_Query callback);

    // TODO: this is repeatead
    delegate void JS_Delegate_VII(int a, int b);

    [DllImport("__Internal")]
    private static extern void SetCallback_Sdk6BinaryMessage(JS_Delegate_VII callback);
}
