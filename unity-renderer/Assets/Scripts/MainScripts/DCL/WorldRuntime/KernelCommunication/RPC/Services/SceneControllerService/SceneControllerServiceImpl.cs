using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Helpers;
using DCL.Models;
using rpc_csharp;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RPC.Services
{
    public class SceneControllerServiceImpl : IRpcSceneControllerService<RPCContext>
    {
        // HACK: Until we fix the code generator, we must replace all 'Decentraland.Common.Entity' for 'DCL.ECSComponents.Entity' in RpcSceneController.gen.cs
        // to be able to access request.Entity properties.

        private const string REQUIRED_PORT_ID_START = "scene-";
        private int sceneNumber = -1;
        private RPCContext context;
        private RpcServerPort<RPCContext> port;

        private static readonly UnloadSceneResult defaultUnloadSceneResult = new UnloadSceneResult();

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            if (!port.portName.StartsWith(REQUIRED_PORT_ID_START)) return;

            RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl(port));
        }

        public SceneControllerServiceImpl(RpcServerPort<RPCContext> port)
        {
            port.OnClose += OnPortClose;
            this.port = port;
        }

        private void OnPortClose()
        {
            port.OnClose -= OnPortClose;

            if (context != null && context.crdt.WorldState.ContainsScene(sceneNumber))
                UnloadScene(null, context, new CancellationToken());
        }

        public async UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            // Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.LoadScene() - scene number: {request.SceneNumber}; metadata: {request.Entity.Metadata}");

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
                    name = request.SceneName,
                    baseUrl = request.BaseUrl,
                    sceneNumber = sceneNumber,
                    isPortableExperience = request.IsPortableExperience,
                    icon = string.Empty // TODO: add icon url!
                };

                context.crdt.SceneController.CreateGlobalScene(globalScene);
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
                    contents = parsedContent
                };

                context.crdt.SceneController.LoadUnityParcelScene(unityParcelScene);
            }

            LoadSceneResult result = new LoadSceneResult() { Success = true };
            return result;
        }

        public async UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            // Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.UnloadScene() - scene number: {sceneNumber}");

            await UniTask.SwitchToMainThread(ct);
            context.crdt.SceneController.UnloadParcelSceneExecute(sceneNumber);

            return defaultUnloadSceneResult;
        }

        public async UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request, RPCContext context, CancellationToken ct)
        {
            IParcelScene scene = null;

            // This line is to avoid a race condition because a CRDT message could be sent before the scene was loaded
            // more info: https://github.com/decentraland/sdk/issues/480#issuecomment-1331309908
            await UniTask.WaitUntil(() => context.crdt.WorldState.TryGetScene(sceneNumber, out scene),
                cancellationToken: ct);

            await UniTask.WaitWhile(() => context.crdt.MessagingControllersManager.HasScenePendingMessages(sceneNumber),
                cancellationToken: ct);

            await UniTask.SwitchToMainThread(ct);

            try
            {
                int counter = 0;

                using (var iterator = CRDTDeserializer.DeserializeBatch(request.Payload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        if (!(iterator.Current is CRDTMessage crdtMessage))
                            continue;

                        context.crdt.CrdtMessageReceived?.Invoke(sceneNumber, crdtMessage);
                        counter++;
                    }
                }

                if (counter > 0)
                {
                    // When sdk7 scene receive it first crdt we set `InitMessagesDone` since
                    // kernel won't be sending that message for those scenes
                    if (scene.sceneData.sdk7 && !scene.IsInitMessageDone())
                    {
                        context.crdt.SceneController.EnqueueSceneMessage(new QueuedSceneMessage_Scene()
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
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return new CRDTSceneMessage() { Payload = request.Payload };
        }

        public async UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.GetCurrentState()...");
            throw new NotImplementedException();
        }
    }
}
