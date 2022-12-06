using Cysharp.Threading.Tasks;
using DCL;
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

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl());
        }

        public async UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log("SceneControllerServiceImpl.LoadScene() - " + request.Entity.Metadata);

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
                // Debug.Log("original parcel: " + parsedMetadata.scene.parcels[i] + "; parsed: " + parsedParcels[i]);
            }
            // Debug.Log("original base: " + parsedMetadata.scene.@base + "; parsed: " + Utils.StringToVector2Int(parsedMetadata.scene.@base));

            LoadParcelScenesMessage.UnityParcelScene unityParcelScene = new LoadParcelScenesMessage.UnityParcelScene()
            {
                sceneNumber = request.SceneNumber,
                id = request.Entity.Id,
                sdk7 = request.Sdk7,
                baseUrl = request.BaseUrl,
                baseUrlBundles = request.BaseUrlAssetBundles,
                basePosition = Utils.StringToVector2Int(parsedMetadata.scene.@base),
                parcels = parsedParcels,
                contents = parsedContent
            };

            context.crdt.SceneController.LoadUnityParcelScene(unityParcelScene);

            // TODO: bind this result to a real 'Success' value
            LoadSceneResult result = new LoadSceneResult() { Success = true };

            return result;
        }

        public async UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log("SceneControllerServiceImpl.UnloadScene()");
            throw new NotImplementedException();
        }

        public async UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log("SceneControllerServiceImpl.SendCrdt()");
            throw new NotImplementedException();
        }

        public async UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request, RPCContext context, CancellationToken ct)
        {
            Debug.Log("SceneControllerServiceImpl.GetCurrentState()");
            throw new NotImplementedException();
        }
    }
}
