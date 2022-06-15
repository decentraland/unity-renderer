using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.CRDT;
using Google.Protobuf;
using KernelCommunication;
using Proto;
using rpc_csharp.server;

namespace RPC.Services
{
    public static class CRDTServiceImpl
    {
        private static readonly UniTask<CRDTResponse> defaultResponse = UniTask.FromResult(new CRDTResponse());
        private static readonly CRDTManyMessages reusableCrdtMessage = new CRDTManyMessages();

        // TODO: remove binary reader and use this stream instead
        private static readonly CRDTStream crdtStream = new CRDTStream();

        public static void RegisterService(RpcServerPort<CRDTServiceContext> port)
        {
            CRDTService<CRDTServiceContext>.RegisterService(
                port,
                sendCRDT: OnCRDTReceived,
                cRDTNotificationStream: CRDTNotificationStream
            );
        }

        private static UniTask<CRDTResponse> OnCRDTReceived(CRDTManyMessages messages, CRDTServiceContext context)
        {
            messages.Payload.WriteTo(crdtStream);

            var sceneMessagesPool = context.messageQueueHanlder.sceneMessagesPool;

            // TODO: delete KernelBinaryMessageDeserializer and move deserialization here
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(crdtStream))
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

                    context.messageQueueHanlder.EnqueueSceneMessage(queuedMessage);
                }
            }

            return defaultResponse;
        }

        private static IEnumerator<CRDTManyMessages> CRDTNotificationStream(CRDTStreamRequest request, CRDTServiceContext context)
        {
            while (true)
            {
                if (context.notifications.Count > 0)
                {
                    var (sceneId, payload) = context.notifications.Dequeue();
                    reusableCrdtMessage.SceneId = sceneId;
                    reusableCrdtMessage.Payload = ByteString.CopyFrom(payload);
                    yield return reusableCrdtMessage;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}