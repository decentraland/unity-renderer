using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.CRDT;
using Google.Protobuf;
using KernelCommunication;
using rpc_csharp;
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
            await UniTask.WaitWhile(() => context.crdtContext.MessagingControllersManager.HasScenePendingMessages(messages.SceneId),
                cancellationToken: ct);

            var sceneMessagesPool = context.crdt.messageQueueHandler.sceneMessagesPool;

            try
            {
                using (var iterator = CRDTDeserializer.DeserializeBatch(messages.Payload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        if (!(iterator.Current is CRDTMessage crdtMessage))
                            continue;

                        if (!sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene queuedMessage))
                        {
                            queuedMessage = new QueuedSceneMessage_Scene();
                        }
                        queuedMessage.method = MessagingTypes.CRDT_MESSAGE;
                        queuedMessage.type = QueuedSceneMessage.Type.SCENE_MESSAGE;
                        queuedMessage.sceneId = messages.SceneId;
                        queuedMessage.payload = crdtMessage;

                        context.crdt.messageQueueHandler.EnqueueSceneMessage(queuedMessage);
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
            string sceneId = request.SceneId;

            try
            {
                if (!context.crdt.scenesOutgoingCrdts.TryGetValue(sceneId, out CRDTProtocol sceneCrdtState))
                {
                    return emptyResponse;
                }

                memoryStream.SetLength(0);

                context.crdt.scenesOutgoingCrdts.Remove(sceneId);

                KernelBinaryMessageSerializer.Serialize(binaryWriter, sceneCrdtState);
                sceneCrdtState.ClearOnUpdated();

                reusableCrdtMessage.SceneId = sceneId;
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
