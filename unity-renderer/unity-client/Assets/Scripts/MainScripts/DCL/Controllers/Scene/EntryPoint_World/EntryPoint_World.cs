using System;
using System.Runtime.InteropServices;
using DCL;
using DCL.Interface;
using DCL.Models;
using UnityEngine;
using Ray = DCL.Models.Ray;

public class EntryPoint_World
{
    private static string currentEntityId;
    private static string currentSceneId;
    private static string currentTag;

    private static IMessageQueueHandler queueHandler;

    delegate void JS_Delegate_VIS(int a, string b);

    delegate void JS_Delegate_VSS(string a, string b);

    delegate void JS_Delegate_VSSS(string a, string b, string c);

    delegate void JS_Delegate_VS(string a);

    delegate void JS_Delegate_Query(Protocol.QueryPayload a);

    delegate void JS_Delegate_V();

    public EntryPoint_World(IMessageQueueHandler queueHandler)
    {
        EntryPoint_World.queueHandler = queueHandler;
#if UNITY_WEBGL && !UNITY_EDITOR
        SetCallback_CreateEntity(CreateEntity);
        SetCallback_RemoveEntity(RemoveEntity);
        SetCallback_SceneReady(SceneReady);

        SetCallback_SetEntityId(SetEntityId);
        SetCallback_SetSceneId(SetSceneId);
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
#endif
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
                sceneId = currentSceneId
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
    internal static void SetEntityId(string id)
    {
        currentEntityId = id;
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetSceneId(string id)
    {
        currentSceneId = id;
    }

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetTag(string id)
    {
        currentTag = id;
    }

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
        QueuedSceneMessage_Scene message;

        var sceneMessagesPool = queueHandler.sceneMessagesPool;

        if (sceneMessagesPool.Count > 0)
            message = sceneMessagesPool.Dequeue();
        else
            message = new QueuedSceneMessage_Scene();

        message.sceneId = currentSceneId;
        message.tag = currentTag;
        message.type = QueuedSceneMessage.Type.SCENE_MESSAGE;

        return message;
    }


    [DllImport("__Internal")]
    private static extern void SetCallback_CreateEntity(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_RemoveEntity(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SceneReady(JS_Delegate_V callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetEntityId(JS_Delegate_VS callback);

    [DllImport("__Internal")]
    private static extern void SetCallback_SetSceneId(JS_Delegate_VS callback);

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
}