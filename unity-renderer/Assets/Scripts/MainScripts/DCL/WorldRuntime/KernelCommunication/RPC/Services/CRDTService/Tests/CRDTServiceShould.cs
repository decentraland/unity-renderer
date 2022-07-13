using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using DCL;
using DCL.CRDT;
using Google.Protobuf;
using KernelCommunication;
using NSubstitute;
using NUnit.Framework;
using Proto;
using RPC;
using rpc_csharp;
using rpc_csharp_test;
using rpc_csharp.transport;
using RPC.Services;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class CRDTServiceShould
    {
        private RPCContext context;
        private ITransport testClientTransport;
        private RpcServer<RPCContext> rpcServer;
        private CancellationTokenSource testCancellationSource;

        [SetUp]
        public void SetUp()
        {
            context = new RPCContext();

            var (clientTransport, serverTransport) = MemoryTransport.Create();
            rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                CRDTServiceImpl.RegisterService(port);
            });
            testClientTransport = clientTransport;
            testCancellationSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            rpcServer.Dispose();
            testCancellationSource.Cancel();
            testCancellationSource.Dispose();
        }

        [Test]
        public async void ProcessIncomingCRDT()
        {
            TestClient testClient = await TestClient.Create(testClientTransport, CRDTService<RPCContext>.ServiceName);

            var messageQueueHandler = Substitute.For<IMessageQueueHandler>();
            messageQueueHandler.sceneMessagesPool.Returns(new ConcurrentQueue<QueuedSceneMessage_Scene>());
            context.crdtContext.messageQueueHandler = messageQueueHandler;

            string sceneId = "temptation";
            CRDTMessage crdtMessage = new CRDTMessage()
            {
                key = 7693,
                timestamp = 799,
                data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
            };

            // Check if incoming CRDT is dispatched as scene message
            messageQueueHandler.WhenForAnyArgs(x => x.EnqueueSceneMessage(Arg.Any<QueuedSceneMessage_Scene>()))
                               .Do(info =>
                               {
                                   QueuedSceneMessage_Scene message = info.Arg<QueuedSceneMessage_Scene>();
                                   Assert.AreEqual(message.sceneId, sceneId);
                                   CRDTMessage received = (CRDTMessage)message.payload;
                                   Assert.AreEqual(crdtMessage.key, received.key);
                                   Assert.AreEqual(crdtMessage.timestamp, received.timestamp);
                                   Assert.IsTrue(AreEqual((byte[])received.data, (byte[])crdtMessage.data));
                               });

            // Simulate client sending `crdtMessage` CRDT
            try
            {
                await testClient.CallProcedure<CRDTResponse>("SendCrdt", new CRDTManyMessages()
                {
                    SceneId = sceneId,
                    Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtMessage))
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            messageQueueHandler.Received(1).EnqueueSceneMessage(Arg.Any<QueuedSceneMessage_Scene>());
        }

        // [Test]
        // public async void SendCRDTNotificationToClient()
        // {
        //     TestClient testClient = await TestClient.Create(testClientTransport, CRDTService<RPCContext>.ServiceName);
        //
        //     UniTaskCompletionSource<CRDTManyMessages> clientReceiveCRDT = null;
        //     bool isLastElementOfStream = false;
        //
        //     // this task simulate renderer generating CRDT notifications to send to client
        //     UniTask.RunOnThreadPool(async () =>
        //     {
        //         await UniTask.Yield();
        //         var crdt = new CRDTMessage()
        //         {
        //             key = 2,
        //             timestamp = 3945,
        //             data = new byte[] { 2, 4, 6, 8 }
        //         };
        //         var sceneId = "temptation";
        //         var crdtBytes = CreateCRDTMessage(crdt);
        //
        //         clientReceiveCRDT = new UniTaskCompletionSource<CRDTManyMessages>();
        //         context.crdtContext.notifications.Enqueue((sceneId, crdtBytes));
        //         var clientReceive = await clientReceiveCRDT.Task;
        //
        //         Assert.AreEqual(clientReceive.SceneId, sceneId);
        //         Assert.IsTrue(AreEqual(crdtBytes, clientReceive.Payload.ToByteArray()));
        //
        //         await UniTask.Yield();
        //         crdt = new CRDTMessage()
        //         {
        //             key = 1,
        //             timestamp = 344,
        //             data = new byte[] { 122, 32, 1 }
        //         };
        //         sceneId = "temptation2";
        //         crdtBytes = CreateCRDTMessage(crdt);
        //
        //         isLastElementOfStream = true;
        //
        //         clientReceiveCRDT = new UniTaskCompletionSource<CRDTManyMessages>();
        //         context.crdtContext.notifications.Enqueue((sceneId, crdtBytes));
        //         clientReceive = await clientReceiveCRDT.Task;
        //
        //         Assert.AreEqual(clientReceive.SceneId, sceneId);
        //         Assert.IsTrue(AreEqual(crdtBytes, clientReceive.Payload.ToByteArray()));
        //     }, true, testCancellationSource.Token);
        //
        //     bool testDone = false;
        //
        //     try
        //     {
        //         // client receives CRDT notifications
        //         await foreach (var element in testClient.CallStream<CRDTManyMessages>("CrdtNotificationStream", new CRDTStreamRequest()))
        //         {
        //             var receivedCrdt = await element;
        //             clientReceiveCRDT.TrySetResult(receivedCrdt);
        //             if (isLastElementOfStream)
        //             {
        //                 testDone = true;
        //                 break;
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError(e);
        //     }
        //     Assert.IsTrue(testDone);
        // }

        static byte[] CreateCRDTMessage(CRDTMessage message)
        {
            using (MemoryStream msgStream = new MemoryStream())
            {
                using (BinaryWriter msgWriter = new BinaryWriter(msgStream))
                {
                    KernelBinaryMessageSerializer.Serialize(msgWriter, message);
                    return msgStream.ToArray();
                }
            }
        }

        static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}