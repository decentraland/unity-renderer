using System;
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
        private static readonly CRDTResponse defaultResponse = new CRDTResponse();
        private static readonly UniTask<CRDTManyMessages> emptyResponse = UniTask.FromResult(new CRDTManyMessages() { SceneId = "", Payload = ByteString.Empty });

        private static readonly CRDTManyMessages reusableCrdtMessage = new CRDTManyMessages();

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            CRDTStream crdtStream = new CRDTStream();
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

            CRDTService<RPCContext>.RegisterService(
                port,
                sendCrdt: (messages, context, ct) => OnCRDTReceived(messages, context, ct, crdtStream),
                pullCrdt: (request, context, ct) => SendCRDT(request, context, ct, memoryStream, binaryWriter),
                crdtNotificationStream: CrdtNotificationStream
            );
        }

        private static async UniTask<CRDTResponse> OnCRDTReceived(CRDTManyMessages messages, RPCContext context,
            CancellationToken ct, CRDTStream crdtStream)
        {
            await UniTask.SwitchToMainThread(ct);
            
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

                        context.crdtContext.CrdtMessageReceived?.Invoke(messages.SceneId, crdtMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return defaultResponse;
        }

        private static UniTask<CRDTManyMessages> SendCRDT(PullCRDTRequest request, RPCContext context,
            CancellationToken ct, MemoryStream memoryStream, BinaryWriter binaryWriter)
        {
            string sceneId = request.SceneId;

            try
            {
                if (!context.crdtContext.scenesOutgoingCrdts.TryGetValue(sceneId, out CRDTProtocol sceneCrdtState))
                {
                    return emptyResponse;
                }

                memoryStream.SetLength(0);

                context.crdtContext.scenesOutgoingCrdts.Remove(sceneId);

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

        [Obsolete("deprecated")]
        private static IEnumerator<CRDTManyMessages> CrdtNotificationStream(CRDTStreamRequest request, RPCContext context)
        {
            yield break;
        }
    }
}