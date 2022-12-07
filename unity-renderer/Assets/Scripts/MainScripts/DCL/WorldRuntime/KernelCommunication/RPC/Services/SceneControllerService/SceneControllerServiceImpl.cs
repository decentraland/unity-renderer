using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.CRDT;
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

        private int sceneNumber = -1;

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl());
        }

        public async UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.LoadScene() - {request.Entity.Metadata}");

            sceneNumber = request.SceneNumber;

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

            LoadParcelScenesMessage.UnityParcelScene unityParcelScene = new LoadParcelScenesMessage.UnityParcelScene()
            {
                sceneNumber = this.sceneNumber,
                id = request.Entity.Id,
                sdk7 = request.Sdk7,
                baseUrl = request.BaseUrl,
                baseUrlBundles = request.BaseUrlAssetBundles,
                basePosition = Utils.StringToVector2Int(parsedMetadata.scene.@base),
                parcels = parsedParcels,
                contents = parsedContent
            };

            await UniTask.SwitchToMainThread(ct);
            context.crdt.SceneController.LoadUnityParcelScene(unityParcelScene);

            // TODO: bind this result to a real 'Success' value ?
            LoadSceneResult result = new LoadSceneResult() { Success = true };
            return result;
        }

        private static readonly UnloadSceneResult defaultUnloadSceneResult = new UnloadSceneResult();
        public async UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.UnloadScene() - scene number: {sceneNumber}");

            await UniTask.SwitchToMainThread(ct);
            context.crdt.SceneController.UnloadScene(sceneNumber);

            return defaultUnloadSceneResult;
        }

        public async UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.SendCrdt() - {request.Payload}");

            // var deserializedMessage = DCL.CRDT.CRDTDeserializer.DeserializeSingle(request.Payload.Memory)
            try
            {
                using (var iterator = CRDTDeserializer.DeserializeBatch(request.Payload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        if (!(iterator.Current is CRDTMessage crdtMessage))
                        {
                            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.SendCrdt() - Skipped NON crdtMessage...");
                            continue;
                        }

                        Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.SendCrdt() - PARSED crdtMessage... {crdtMessage.key1}");

                        // context.crdt.CrdtMessageReceived?.Invoke(request.SceneNumber, crdtMessage);
                    }
                }

                // if (context.crdt.WorldState.TryGetScene(request.SceneNumber, out IParcelScene scene))
                // {
                //     // When sdk7 scene receive it first crdt we set `InitMessagesDone` since
                //     // kernel won't be sending that message for those scenes
                //     if (scene.sceneData.sdk7 && !scene.IsInitMessageDone())
                //     {
                //         scene.MarkInitMessagesDone();
                //     }
                // }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            // This line is to avoid a race condition because a CRDT message could be sent before the scene was loaded
            // more info: https://github.com/decentraland/sdk/issues/480#issuecomment-1331309908
            /*await UniTask.WaitUntil(() => context.crdt.MessagingControllersManager.ContainsController(messages.SceneNumber),
                cancellationToken: ct);

            await UniTask.WaitWhile(() => context.crdt.MessagingControllersManager.HasScenePendingMessages(messages.SceneNumber),
                cancellationToken: ct);

            await UniTask.SwitchToMainThread(ct);

            try
            {
                using (var iterator = CRDTDeserializer.DeserializeBatch(messages.Payload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        if (!(iterator.Current is CRDTMessage crdtMessage))
                            continue;

                        context.crdt.CrdtMessageReceived?.Invoke(messages.SceneNumber, crdtMessage);
                    }
                }

                if (context.crdt.WorldState.TryGetScene(messages.SceneNumber, out IParcelScene scene))
                {
                    // When sdk7 scene receive it first crdt we set `InitMessagesDone` since
                    // kernel won't be sending that message for those scenes
                    if (scene.sceneData.sdk7 && !scene.IsInitMessageDone())
                    {
                        scene.MarkInitMessagesDone();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            CRDTSceneMessage defaultResponse = new CRDTSceneMessage();
            return defaultResponse;*/

            throw new NotImplementedException();
        }

        public async UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log($"{GetHashCode()} SceneControllerServiceImpl.GetCurrentState() - ");
            throw new NotImplementedException();
        }
    }
}
