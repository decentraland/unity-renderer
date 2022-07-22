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
                key1 = 7693,
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
                                   Assert.AreEqual(crdtMessage.key1, received.key1);
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

        [Test]
        public async void SendCRDTtoScene()
        {
            string scene1 = "temptation1";
            string scene2 = "temptation2";

            CRDTProtocol sceneState1 = new CRDTProtocol();
            CRDTProtocol sceneState2 = new CRDTProtocol();

            CRDTMessage messageToScene1 = sceneState1.Create(1, 34, new byte[] { 1, 0, 2, 45, 67 });
            CRDTMessage messageToScene2 = sceneState2.Create(45, 9, new byte[] { 42, 42, 42, 41 });

            sceneState1.ProcessMessage(messageToScene1);
            sceneState2.ProcessMessage(messageToScene2);

            context.crdtContext.scenesOutgoingCrdts.Add(scene1, sceneState1);
            context.crdtContext.scenesOutgoingCrdts.Add(scene2, sceneState2);

            // Simulate client requesting scene's crdt
            try
            {
                TestClient testClient = await TestClient.Create(testClientTransport, CRDTService<RPCContext>.ServiceName);

                // request for `scene1`
                CRDTManyMessages response1 = await testClient.CallProcedure<CRDTManyMessages>("PullCrdt",
                    new PullCRDTRequest() { SceneId = scene1 });

                var deserializer = KernelBinaryMessageDeserializer.Deserialize(response1.Payload.ToByteArray());
                deserializer.MoveNext();
                CRDTMessage message = (CRDTMessage)deserializer.Current;

                Assert.AreEqual(messageToScene1.key1, message.key1);
                Assert.AreEqual(messageToScene1.timestamp, message.timestamp);
                Assert.IsTrue(AreEqual((byte[])messageToScene1.data, (byte[])message.data));
                Assert.IsFalse(context.crdtContext.scenesOutgoingCrdts.ContainsKey(scene1));

                // request for `scene2`
                CRDTManyMessages response2 = await testClient.CallProcedure<CRDTManyMessages>("PullCrdt",
                    new PullCRDTRequest() { SceneId = scene2 });

                deserializer = KernelBinaryMessageDeserializer.Deserialize(response2.Payload.ToByteArray());
                deserializer.MoveNext();
                message = (CRDTMessage)deserializer.Current;

                Assert.AreEqual(messageToScene2.key1, message.key1);
                Assert.AreEqual(messageToScene2.timestamp, message.timestamp);
                Assert.IsTrue(AreEqual((byte[])messageToScene2.data, (byte[])message.data));
                Assert.IsFalse(context.crdtContext.scenesOutgoingCrdts.ContainsKey(scene2));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

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