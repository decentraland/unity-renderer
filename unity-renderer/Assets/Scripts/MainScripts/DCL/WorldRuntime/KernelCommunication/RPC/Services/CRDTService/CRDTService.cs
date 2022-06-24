using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.CRDT;
using Google.Protobuf;
using KernelCommunication;
using rpc_csharp;
using UnityEngine;

namespace RPC.Services
{
    public static class CRDTServiceImpl
    {
        private static readonly UniTask<CRDTResponse> defaultResponse = UniTask.FromResult(new CRDTResponse());
        private static readonly CRDTManyMessages reusableCrdtMessage = new CRDTManyMessages();

        // TODO: remove binary reader and use this stream instead
        private static readonly CRDTStream crdtStream = new CRDTStream();

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            CRDTService<RPCContext>.RegisterService(
                port,
                sendCRDT: OnCRDTReceived,
                cRDTNotificationStream: CRDTNotificationStream
            );

            PingPongService<RPCContext>.RegisterService(port,
                ping: async (request, context) =>
                {
                    Debug.Log("Ping received!");
                    return new PongResponse();
                });
        }

        private static UniTask<CRDTResponse> OnCRDTReceived(CRDTManyMessages messages, RPCContext context)
        {
            messages.Payload.WriteTo(crdtStream);

            var sceneMessagesPool = context.crdtContext.messageQueueHanlder.sceneMessagesPool;

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

                    context.crdtContext.messageQueueHanlder.EnqueueSceneMessage(queuedMessage);
                }
            }

            return defaultResponse;
        }

        private static IEnumerator<UniTask<CRDTManyMessages>> CRDTNotificationStream(CRDTStreamRequest request, RPCContext context)
        {
            return new CRDTStreamEnumerator(context);
        }

        class CRDTStreamEnumerator : IEnumerator<UniTask<CRDTManyMessages>>
        {
            private readonly RPCContext context;
            private readonly CancellationTokenSource cancellationTokenSource;
            private UniTaskCompletionSource<CRDTManyMessages> messageFuture;
            private bool isDisposed = false;

            public CRDTStreamEnumerator(RPCContext context)
            {
                this.context = context;
                cancellationTokenSource = new CancellationTokenSource();
            }

            public bool MoveNext()
            {
                if (isDisposed)
                {
                    return false;
                }

                if (messageFuture != null && !messageFuture.Task.GetAwaiter().IsCompleted)
                {
                    return true;
                }

                messageFuture = new UniTaskCompletionSource<CRDTManyMessages>();

                UniTask.Void(async (ct) =>
                {
                    while (context.crdtContext.notifications.Count == 0 && !ct.IsCancellationRequested)
                    {
                        await UniTask.Yield();
                    }

                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    var (sceneId, payload) = context.crdtContext.notifications.Dequeue();
                    Resolve(sceneId, payload);
                }, cancellationTokenSource.Token);

                return true;
            }

            public void Reset() { }

            public UniTask<CRDTManyMessages> Current => messageFuture.Task;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                cancellationTokenSource.Cancel();
                isDisposed = true;
            }

            private void Resolve(string sceneId, byte[] payload)
            {
                reusableCrdtMessage.SceneId = sceneId;
                reusableCrdtMessage.Payload = ByteString.CopyFrom(payload);
                messageFuture.TrySetResult(reusableCrdtMessage);
            }
        }
    }
}