using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf;
using rpc_csharp;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace RPC.Services
{
    public class CRDTServiceImpl : ICRDTService<RPCContext>
    {
        private static readonly CRDTResponse defaultResponse = new CRDTResponse();
        private static readonly UniTask<CRDTManyMessages> emptyResponse = UniTask.FromResult(new CRDTManyMessages() { SceneId = "", Payload = ByteString.Empty });

        private static readonly CRDTManyMessages reusableCrdtMessage = new CRDTManyMessages();

        private static readonly MemoryStream memoryStream = new MemoryStream();
        private static readonly BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            CRDTServiceCodeGen.RegisterService(port, new CRDTServiceImpl());
        }

        public async UniTask<CRDTResponse> SendCrdt(CRDTManyMessages messages, RPCContext context, CancellationToken ct)
        {
            // This line is to avoid a race condition because a CRDT message could be sent before the scene was loaded
            // more info: https://github.com/decentraland/sdk/issues/480#issuecomment-1331309908
            await UniTask.WaitUntil(() => context.crdt.MessagingControllersManager.ContainsController(messages.SceneNumber),
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
                        if (!(iterator.Current is CrdtMessage crdtMessage))
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
                        context.crdt.SceneController.EnqueueSceneMessage(new QueuedSceneMessage_Scene()
                        {
                            sceneNumber = messages.SceneNumber,
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

            return defaultResponse;
        }

        public UniTask<CRDTManyMessages> PullCrdt(PullCRDTRequest request, RPCContext context, CancellationToken ct)
        {
            try
            {
                if (!context.crdt.scenesOutgoingCrdts.TryGetValue(request.SceneNumber, out DualKeyValueSet<int, long, CrdtMessage> sceneCrdtOutgoingMessage))
                {
                    return emptyResponse;
                }

                memoryStream.SetLength(0);

                context.crdt.scenesOutgoingCrdts.Remove(request.SceneNumber);

                foreach (var msg in sceneCrdtOutgoingMessage)
                {
                    CRDTSerializer.Serialize(binaryWriter, msg.value);
                }
                sceneCrdtOutgoingMessage.Clear();

                reusableCrdtMessage.SceneId = request.SceneId;
                reusableCrdtMessage.SceneNumber = request.SceneNumber;
                reusableCrdtMessage.Payload = ByteString.CopyFrom(memoryStream.ToArray());

                return UniTask.FromResult(reusableCrdtMessage);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return emptyResponse;
            }
        }
    }
}
