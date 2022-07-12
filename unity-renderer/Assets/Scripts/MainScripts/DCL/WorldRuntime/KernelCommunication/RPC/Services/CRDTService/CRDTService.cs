using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.CRDT;
using Google.Protobuf;
using KernelCommunication;
using rpc_csharp;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace RPC.Services
{
    public static class CRDTServiceImpl
    {
        private static readonly UniTask<CRDTResponse> defaultResponse = UniTask.FromResult(new CRDTResponse());
        private static readonly CRDTManyMessages reusableCrdtMessage = new CRDTManyMessages();

        private static readonly CRDTStream crdtStream = new CRDTStream();

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            CRDTService<RPCContext>.RegisterService(
                port,
                sendCrdt: OnCRDTReceived,
                crdtNotificationStream: CRDTNotificationStream
            );
        }

        private static UniTask<CRDTResponse> OnCRDTReceived(CRDTManyMessages messages, RPCContext context, CancellationToken ct)
        {
            messages.Payload.WriteTo(crdtStream);

            var sceneMessagesPool = context.crdtContext.messageQueueHandler.sceneMessagesPool;

            try
            {
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

                        context.crdtContext.messageQueueHandler.EnqueueSceneMessage(queuedMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return defaultResponse;
        }

        [Obsolete("To be removed soon")]
        private static IEnumerator<CRDTManyMessages> CRDTNotificationStream(CRDTStreamRequest request, RPCContext context)
        {
            using var sceneIdIterator = context.crdtContext.scenesOutgoingCrdts.Keys.GetEnumerator();
            using var memoryStream = new MemoryStream(5242880);
            using var binaryWriter = new BinaryWriter(memoryStream);

            while (true)
            {
                if (sceneIdIterator.MoveNext())
                {
                    try
                    {
                        memoryStream.SetLength(0);

                        string sceneId = sceneIdIterator.Current;
                        CRDTProtocol sceneCrdtState = context.crdtContext.scenesOutgoingCrdts[sceneId];
                        
                        ((IEnumerator)sceneIdIterator).Reset();
                        context.crdtContext.scenesOutgoingCrdts.Remove(sceneId);

                        KernelBinaryMessageSerializer.Serialize(binaryWriter, sceneCrdtState);
                        reusableCrdtMessage.SceneId = sceneId;
                        reusableCrdtMessage.Payload = ByteString.CopyFrom(memoryStream.GetBuffer());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        continue;
                    }

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