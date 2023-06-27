using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Helpers;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf;
using MainScripts.DCL.Components;
using rpc_csharp;
using RPC.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace RPC.Services
{
    public class SceneControllerServiceImpl : IRpcSceneControllerService<RPCContext>
    {
        // HACK: Until we fix the code generator, we must replace all 'Decentraland.Common.Entity' for 'DCL.ECSComponents.Entity' in RpcSceneController.gen.cs
        // to be able to access request.Entity properties.
        private static readonly UnloadSceneResult defaultUnloadSceneResult = new UnloadSceneResult();

        private static readonly SendBatchResponse defaultSendBatchResult = new SendBatchResponse();

        private const string REQUIRED_PORT_ID_START = "scene-";

        private int sceneNumber = -1;
        private RPCContext context;
        private RpcServerPort<RPCContext> port;
        private bool isFirstMessage = true;
        private int receivedSendCrdtCalls = 0;

        private readonly MemoryStream sendCrdtMemoryStream;
        private readonly BinaryWriter sendCrdtBinaryWriter;
        private readonly MemoryStream getStateMemoryStream;
        private readonly BinaryWriter getStateBinaryWriter;

        private readonly MemoryStream serializeComponentBuffer;
        private readonly CodedOutputStream serializeComponentStream;

        private readonly CRDTSceneMessage reusableCrdtMessageResult = new CRDTSceneMessage();

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            if (!port.portName.StartsWith(REQUIRED_PORT_ID_START)) return;

            RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl(port));
        }

        public SceneControllerServiceImpl(RpcServerPort<RPCContext> port)
        {
            port.OnClose += OnPortClose;
            this.port = port;

            sendCrdtMemoryStream = new MemoryStream();
            sendCrdtBinaryWriter = new BinaryWriter(sendCrdtMemoryStream);

            getStateMemoryStream = new MemoryStream();
            getStateBinaryWriter = new BinaryWriter(getStateMemoryStream);

            serializeComponentBuffer = new MemoryStream();
            serializeComponentStream = new CodedOutputStream(serializeComponentBuffer);
        }

        private void OnPortClose()
        {
            port.OnClose -= OnPortClose;

            if (context != null && context.crdt.WorldState.ContainsScene(sceneNumber))
                UnloadScene(null, context, new CancellationToken()).Forget();
        }

        public async UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            sceneNumber = request.SceneNumber;
            this.context = context;

            List<ContentServerUtils.MappingPair> parsedContent = new List<ContentServerUtils.MappingPair>();

            for (var i = 0; i < request.Entity.Content.Count; i++)
            {
                parsedContent.Add(new ContentServerUtils.MappingPair()
                {
                    file = request.Entity.Content[i].File,
                    hash = request.Entity.Content[i].Hash
                });
            }

            CatalystSceneEntityMetadata parsedMetadata = Utils.SafeFromJson<CatalystSceneEntityMetadata>(request.Entity.Metadata);
            Vector2Int[] parsedParcels = new Vector2Int[parsedMetadata.scene.parcels.Length];

            for (int i = 0; i < parsedMetadata.scene.parcels.Length; i++)
            {
                parsedParcels[i] = Utils.StringToVector2Int(parsedMetadata.scene.parcels[i]);
            }

            await UniTask.SwitchToMainThread(ct);

            if (request.IsGlobalScene)
            {
                CreateGlobalSceneMessage globalScene = new CreateGlobalSceneMessage()
                {
                    contents = parsedContent,
                    id = request.Entity.Id,
                    sdk7 = request.Sdk7,
                    name = request.SceneName,
                    baseUrl = request.BaseUrl,
                    sceneNumber = sceneNumber,
                    isPortableExperience = request.IsPortableExperience,
                    requiredPermissions = parsedMetadata.requiredPermissions,
                    allowedMediaHostnames = parsedMetadata.allowedMediaHostnames,
                    icon = string.Empty // TODO: add icon url!
                };

                try
                {
                    context.crdt.SceneController.CreateGlobalScene(globalScene);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                LoadParcelScenesMessage.UnityParcelScene unityParcelScene = new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = sceneNumber,
                    id = request.Entity.Id,
                    sdk7 = request.Sdk7,
                    baseUrl = request.BaseUrl,
                    baseUrlBundles = request.BaseUrlAssetBundles,
                    basePosition = Utils.StringToVector2Int(parsedMetadata.scene.@base),
                    parcels = parsedParcels,
                    contents = parsedContent,
                    requiredPermissions = parsedMetadata.requiredPermissions,
                    allowedMediaHostnames = parsedMetadata.allowedMediaHostnames
                };

                try
                {
                    context.crdt.SceneController.LoadUnityParcelScene(unityParcelScene);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            LoadSceneResult result = new LoadSceneResult() { Success = true };
            return result;
        }

        public async UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            context.crdt.SceneController.UnloadParcelSceneExecute(sceneNumber);

            return defaultUnloadSceneResult;
        }

        public async UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request, RPCContext context, CancellationToken ct)
        {
            IParcelScene scene = null;
            CRDTServiceContext crdtContext = context.crdt;

            await UniTask.SwitchToMainThread(ct);

            // This line is to avoid a race condition because a CRDT message could be sent before the scene was loaded
            // more info: https://github.com/decentraland/sdk/issues/480#issuecomment-1331309908
            await UniTask.WaitUntil(() => crdtContext.WorldState.TryGetScene(sceneNumber, out scene),
                cancellationToken: ct);

            await UniTask.WaitWhile(() => crdtContext.MessagingControllersManager.HasScenePendingMessages(sceneNumber),
                cancellationToken: ct);

            reusableCrdtMessageResult.Payload = ByteString.Empty;
            if (!scene.sceneData.sdk7) return reusableCrdtMessageResult;

            if (isFirstMessage && (!request.Payload.IsEmpty || receivedSendCrdtCalls > 0))
                isFirstMessage = false;

            receivedSendCrdtCalls++;

            try
            {
                if (!isFirstMessage)
                {
                    using (var iterator = CRDTDeserializer.DeserializeBatch(request.Payload.Memory))
                    {
                        while (iterator.MoveNext())
                        {
                            if (!(iterator.Current is CrdtMessage crdtMessage))
                                continue;

                            crdtContext.CrdtMessageReceived?.Invoke(sceneNumber, crdtMessage);
                        }
                    }

                    if (crdtContext.GetSceneTick(sceneNumber) == 0)
                    {
                        // pause scene update until GLTFs are loaded
                        await UniTask.WaitUntil(() => crdtContext.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber), cancellationToken: ct);

                        // When sdk7 scene receive it first crdt we set `InitMessagesDone` since
                        // kernel won't be sending that message for those scenes
                        if (scene.sceneData.sdk7 && !scene.IsInitMessageDone())
                        {
                            crdtContext.SceneController.EnqueueSceneMessage(new QueuedSceneMessage_Scene()
                            {
                                sceneNumber = sceneNumber,
                                tag = "scene",
                                payload = new Protocol.SceneReady(),
                                method = MessagingTypes.INIT_DONE,
                                type = QueuedSceneMessage.Type.SCENE_MESSAGE
                            });
                        }
                    }
                }

                sendCrdtMemoryStream.SetLength(0);

                SendSceneMessages(crdtContext, sceneNumber, sendCrdtBinaryWriter, serializeComponentBuffer, serializeComponentStream, true);

                reusableCrdtMessageResult.Payload = ByteString.CopyFrom(sendCrdtMemoryStream.ToArray());

                if (!isFirstMessage)
                    crdtContext.IncreaseSceneTick(sceneNumber);
            }
            catch (OperationCanceledException _)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return reusableCrdtMessageResult;
        }

        public async UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request, RPCContext context, CancellationToken ct)
        {
            CRDTProtocol sceneState = null;
            CRDTServiceContext crdtContext = context.crdt;

            await UniTask.SwitchToMainThread(ct);

            if (crdtContext.CrdtExecutors != null && crdtContext.CrdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor executor))
            {
                sceneState = executor.crdtProtocol;
            }

            CRDTSceneCurrentState result = new CRDTSceneCurrentState
            {
                HasOwnEntities = false
            };

            try
            {
                getStateMemoryStream.SetLength(0);

                SendSceneMessages(crdtContext, sceneNumber, sendCrdtBinaryWriter, serializeComponentBuffer, serializeComponentStream, false);

                // serialize scene state
                if (sceneState != null)
                {
                    var crdtMessages = sceneState.GetStateAsMessages();

                    for (int i = 0; i < crdtMessages.Count; i++)
                    {
                        CRDTSerializer.Serialize(getStateBinaryWriter, crdtMessages[i]);
                    }
                }

                result.Payload = ByteString.CopyFrom(getStateMemoryStream.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return result;
        }

        public async UniTask<SendBatchResponse> SendBatch(SendBatchRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);

            try
            {
                RendererManyEntityActions sceneRequest = RendererManyEntityActions.Parser.ParseFrom(request.Payload);

                for (var i = 0; i < sceneRequest.Actions.Count; i++)
                {
                    context.crdt.SceneController.EnqueueSceneMessage(
                        SDK6DataMapExtensions.SceneMessageFromSdk6Message(sceneRequest.Actions[i], sceneNumber)
                    );
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return defaultSendBatchResult;
        }

        private static void SendSceneMessages(CRDTServiceContext crdtContext,
            int sceneNumber,
            BinaryWriter sendCrdtBinaryWriter,
            MemoryStream serializeComponentBuffer,
            CodedOutputStream serializeComponentStream,
            bool clearMessages)
        {
            if (!crdtContext.CrdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor executor))
                return;

            if (crdtContext.ScenesOutgoingMsgs.TryGetValue(sceneNumber, out var msgs))
            {
                var pairs = msgs.Pairs;

                for (int i = 0; i < pairs.Count; i++)
                {
                    var msg = pairs[i].value;
                    int entityId = (int)pairs[i].key1;
                    int componentId = pairs[i].key2;

                    if (msg.MessageType != CrdtMessageType.APPEND_COMPONENT
                        && msg.MessageType != CrdtMessageType.PUT_COMPONENT
                        && msg.MessageType != CrdtMessageType.DELETE_COMPONENT)
                    {
                        if (clearMessages)
                            msg.Dispose();

                        continue;
                    }

                    CRDTProtocol crdtProtocol = executor.crdtProtocol;
                    CrdtMessage crdtMessage;

                    if (msg.MessageType == CrdtMessageType.APPEND_COMPONENT || msg.MessageType == CrdtMessageType.PUT_COMPONENT)
                        msg.PooledWrappedComponent.WrappedComponentBase.SerializeTo(serializeComponentBuffer, serializeComponentStream);

                    if (msg.MessageType == CrdtMessageType.APPEND_COMPONENT)
                    {
                        crdtMessage = crdtProtocol.CreateSetMessage(entityId, componentId, serializeComponentBuffer.ToArray());
                    }
                    else if (msg.MessageType == CrdtMessageType.PUT_COMPONENT)
                    {
                        crdtMessage = crdtProtocol.CreateLwwMessage(entityId, componentId, serializeComponentBuffer.ToArray());
                    }
                    else
                    {
                        crdtMessage = crdtProtocol.CreateLwwMessage(entityId, componentId, null);
                    }

                    CRDTProtocol.ProcessMessageResultType resultType = crdtProtocol.ProcessMessage(crdtMessage);

                    if (resultType == CRDTProtocol.ProcessMessageResultType.StateUpdatedData ||
                        resultType == CRDTProtocol.ProcessMessageResultType.StateUpdatedTimestamp ||
                        resultType == CRDTProtocol.ProcessMessageResultType.EntityWasDeleted ||
                        resultType == CRDTProtocol.ProcessMessageResultType.StateElementAddedToSet)
                    {
                        CRDTSerializer.Serialize(sendCrdtBinaryWriter, crdtMessage);
                    }

                    if (clearMessages)
                        msg.Dispose();
                }

                if (clearMessages)
                    msgs.Clear();
            }
        }
    }
}
