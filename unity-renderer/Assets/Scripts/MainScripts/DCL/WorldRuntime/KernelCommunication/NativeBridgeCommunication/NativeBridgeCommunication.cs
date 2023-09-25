using DCL;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using MainScripts.DCL.Components;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using Ray = DCL.Models.Ray;

public class NativeBridgeCommunication : IKernelCommunication
{
    private delegate void JS_Delegate_V();
    private delegate void JS_Delegate_Query(Protocol.QueryPayload a);
    private delegate void JS_Delegate_VS(string a);
    private delegate void JS_Delegate_VI(int a);
    private delegate void JS_Delegate_VIS(int a, string b);
    private delegate void JS_Delegate_VSS(string a, string b);
    private delegate void JS_Delegate_VSSS(string a, string b, string c);

    // TODO: this is repeated
    private delegate void JS_Delegate_VII(int a, int b);

    private static readonly byte[] PREALLOCATED_READER_BUFFER = new byte[88388608];

    private static string currentEntityId;
    private static int currentSceneNumber;
    private static string currentTag;

    private static IMessageQueueHandler queueHandler;

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

    public void Dispose() { }

    [DllImport("__Internal")] private static extern void SetCallback_Sdk6BinaryMessage(JS_Delegate_VII callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VII))]
    internal static unsafe void Sdk6BinaryMessage(int intPtr, int length)
    {
        var ptr = new IntPtr(intPtr);

        var readonlySpan = new ReadOnlySpan<byte>(ptr.ToPointer(), length);
        readonlySpan.CopyTo(PREALLOCATED_READER_BUFFER);

        try
        {
            RendererManyEntityActions sceneRequest = RendererManyEntityActions.Parser.ParseFrom(PREALLOCATED_READER_BUFFER, 0, length);

            for (var i = 0; i < sceneRequest.Actions.Count; i++)
                queueHandler.EnqueueSceneMessage(
                    SDK6DataMapExtensions.SceneMessageFromSdk6Message(sceneRequest.Actions[i], currentSceneNumber)
                );
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    [DllImport("__Internal")] private static extern void SetCallback_SetTag(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetTag(string id) =>
        currentTag = id;

    [DllImport("__Internal")] private static extern void SetCallback_SetSceneNumber(JS_Delegate_VI callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VI))]
    internal static void SetSceneNumber(int sceneNumber) =>
        currentSceneNumber = sceneNumber;

    [DllImport("__Internal")] private static extern void SetCallback_SceneReady(JS_Delegate_V callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void SceneReady() =>
        EnqueueSceneMessage(MessagingTypes.INIT_DONE, new Protocol.SceneReady());

    [DllImport("__Internal")] private static extern void SetCallback_CreateEntity(JS_Delegate_V callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void CreateEntity() =>
        EnqueueSceneMessage(MessagingTypes.ENTITY_CREATE, new Protocol.CreateEntity { entityId = currentEntityId });

    [DllImport("__Internal")] private static extern void SetCallback_RemoveEntity(JS_Delegate_V callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_V))]
    internal static void RemoveEntity() =>
        EnqueueSceneMessage(MessagingTypes.ENTITY_DESTROY, new Protocol.RemoveEntity { entityId = currentEntityId });

    [DllImport("__Internal")] private static extern void SetCallback_SetEntityId(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetEntityId(string id) =>
        currentEntityId = id;

    [DllImport("__Internal")] private static extern void SetCallback_SetEntityParent(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SetEntityParent(string parentId) =>
        EnqueueSceneMessage(MessagingTypes.ENTITY_REPARENT, new Protocol.SetEntityParent { entityId = currentEntityId, parentId = parentId });

    [DllImport("__Internal")] private static extern void SetCallback_EntityComponentCreateOrUpdate(JS_Delegate_VIS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VIS))]
    internal static void EntityComponentCreateOrUpdate(int classId, string json) =>
        EnqueueSceneMessage(MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE, new Protocol.EntityComponentCreateOrUpdate { entityId = currentEntityId, classId = classId, json = json });

    [DllImport("__Internal")] private static extern void SetCallback_EntityComponentDestroy(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void EntityComponentDestroy(string name) =>
        EnqueueSceneMessage(MessagingTypes.ENTITY_COMPONENT_DESTROY, new Protocol.EntityComponentDestroy { entityId = currentEntityId, name = name });

    [DllImport("__Internal")] private static extern void SetCallback_SharedComponentCreate(JS_Delegate_VIS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VIS))]
    internal static void SharedComponentCreate(int classId, string id) =>
        EnqueueSceneMessage(MessagingTypes.SHARED_COMPONENT_CREATE, new Protocol.SharedComponentCreate { id = id, classId = classId });

    [DllImport("__Internal")] private static extern void SetCallback_SharedComponentDispose(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void SharedComponentDispose(string id) =>
        EnqueueSceneMessage(MessagingTypes.SHARED_COMPONENT_DISPOSE, new Protocol.SharedComponentDispose { id = id });

    [DllImport("__Internal")] private static extern void SetCallback_SharedComponentAttach(JS_Delegate_VSS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VSS))]
    internal static void SharedComponentAttach(string id, string name) =>
        EnqueueSceneMessage(MessagingTypes.SHARED_COMPONENT_ATTACH, new Protocol.SharedComponentAttach { entityId = currentEntityId, id = id, name = name });

    [DllImport("__Internal")] private static extern void SetCallback_SharedComponentUpdate(JS_Delegate_VSS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VSS))]
    internal static void SharedComponentUpdate(string id, string json) =>
        EnqueueSceneMessage(MessagingTypes.SHARED_COMPONENT_UPDATE, new Protocol.SharedComponentUpdate { componentId = id, json = json });

    [DllImport("__Internal")] private static extern void SetCallback_OpenExternalUrl(JS_Delegate_VS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    internal static void OpenExternalUrl(string url) =>
        EnqueueSceneMessage(MessagingTypes.OPEN_EXTERNAL_URL, new Protocol.OpenExternalUrl { url = url });

    [DllImport("__Internal")] private static extern void SetCallback_OpenNftDialog(JS_Delegate_VSSS callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_VSSS))]
    internal static void OpenNftDialog(string contactAddress, string comment, string tokenId) =>
        EnqueueSceneMessage(MessagingTypes.OPEN_NFT_DIALOG, new Protocol.OpenNftDialog { contactAddress = contactAddress, comment = comment, tokenId = tokenId });

    [DllImport("__Internal")] private static extern void SetCallback_Query(JS_Delegate_Query callback);

    [MonoPInvokeCallback(typeof(JS_Delegate_Query))]
    internal static void Query(Protocol.QueryPayload payload)
    {
        var ray = new Ray
        {
            origin = payload.raycastPayload.origin,
            direction = payload.raycastPayload.direction,
            distance = payload.raycastPayload.distance,
        };

        var resultPayload = new QueryMessage
        {
            payload = new RaycastQuery
            {
                id = Convert.ToString(payload.raycastPayload.id),
                raycastType = (RaycastType)payload.raycastPayload.raycastType,
                ray = ray,
                sceneNumber = currentSceneNumber,
            },
        };

        EnqueueSceneMessage(MessagingTypes.QUERY, resultPayload);
    }

    private static void EnqueueSceneMessage(string messageType, object payload)
    {
        QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        queuedMessage.method = messageType;
        queuedMessage.payload = payload;

        queueHandler.EnqueueSceneMessage(queuedMessage);

        QueuedSceneMessage_Scene GetSceneMessageInstance()
        {
            var message = new QueuedSceneMessage_Scene();
            message.sceneNumber = currentSceneNumber;
            message.tag = currentTag;
            message.type = QueuedSceneMessage.Type.SCENE_MESSAGE;

            return message;
        }
    }

#region DEPRECATED
    // @deprecated use SetSceneNumber
    [DllImport("__Internal")] private static extern void SetCallback_SetSceneId(JS_Delegate_VS callback);

    // @deprecated use SetSceneNumber
    [MonoPInvokeCallback(typeof(JS_Delegate_VI))] internal static void SetSceneId(string _) { }
#endregion
}
