using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Helpers;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf;
using rpc_csharp;
using RPC.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;
using Ray = DCL.Models.Ray;

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

        private readonly MemoryStream sendCrdtMemoryStream;
        private readonly BinaryWriter sendCrdtBinaryWriter;
        private readonly MemoryStream getStateMemoryStream;
        private readonly BinaryWriter getStateBinaryWriter;

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

            try
            {
                int incomingCrdtCount = 0;
                reusableCrdtMessageResult.Payload = ByteString.Empty;

                using (var iterator = CRDTDeserializer.DeserializeBatch(request.Payload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        if (!(iterator.Current is CrdtMessage crdtMessage))
                            continue;

                        crdtContext.CrdtMessageReceived?.Invoke(sceneNumber, crdtMessage);
                        incomingCrdtCount++;
                    }
                }

                if (incomingCrdtCount > 0)
                {
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

                if (crdtContext.scenesOutgoingCrdts.TryGetValue(sceneNumber, out DualKeyValueSet<int, long, CrdtMessage> sceneCrdtOutgoing))
                {
                    sendCrdtMemoryStream.SetLength(0);
                    crdtContext.scenesOutgoingCrdts.Remove(sceneNumber);

                    for (int i = 0; i < sceneCrdtOutgoing.Count; i++)
                    {
                        CRDTSerializer.Serialize(sendCrdtBinaryWriter, sceneCrdtOutgoing.Pairs[i].value);
                    }

                    sceneCrdtOutgoing.Clear();
                    reusableCrdtMessageResult.Payload = ByteString.CopyFrom(sendCrdtMemoryStream.ToArray());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return reusableCrdtMessageResult;
        }

        public async UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request, RPCContext context, CancellationToken ct)
        {
            DualKeyValueSet<int, long, CrdtMessage> outgoingMessages = null;
            CRDTProtocol sceneState = null;
            CRDTServiceContext crdtContext = context.crdt;

            // we wait until messages for scene are set
            await UniTask.WaitUntil(() => crdtContext.scenesOutgoingCrdts.TryGetValue(sceneNumber, out outgoingMessages),
                cancellationToken: ct);

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

                // serialize outgoing messages
                crdtContext.scenesOutgoingCrdts.Remove(sceneNumber);

                foreach (var msg in outgoingMessages)
                {
                    CRDTSerializer.Serialize(getStateBinaryWriter, msg.value);
                }

                outgoingMessages.Clear();

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
    }
}
