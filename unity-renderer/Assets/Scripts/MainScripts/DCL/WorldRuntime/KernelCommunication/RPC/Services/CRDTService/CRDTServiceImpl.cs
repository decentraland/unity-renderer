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
            await UniTask.WaitWhile(() => context.crdtContext.MessagingControllersManager.HasScenePendingMessages(messages.SceneNumber),
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

                        context.crdtContext.CrdtMessageReceived?.Invoke(messages.SceneNumber, crdtMessage);
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
                if (!context.crdtContext.scenesOutgoingCrdts.TryGetValue(request.SceneNumber, out CRDTProtocol sceneCrdtState))
                {
                    return emptyResponse;
                }

                memoryStream.SetLength(0);

                context.crdtContext.scenesOutgoingCrdts.Remove(request.SceneNumber);

                KernelBinaryMessageSerializer.Serialize(binaryWriter, sceneCrdtState);
                sceneCrdtState.ClearOnUpdated();
                
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