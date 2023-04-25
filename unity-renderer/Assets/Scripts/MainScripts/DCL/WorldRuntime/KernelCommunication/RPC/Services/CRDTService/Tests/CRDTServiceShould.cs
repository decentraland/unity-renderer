using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Models;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf;
using NSubstitute;
using NUnit.Framework;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Services;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
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
                CRDTServiceCodeGen.RegisterService(port, new CRDTServiceImpl());
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

        [UnityTest]
        public IEnumerator ProcessIncomingCRDT()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                ClientCRDTService clientCrdtService = await CreateClientCrdtService(testClientTransport);

                var messagingControllersManager = Substitute.For<IMessagingControllersManager>();
                messagingControllersManager.HasScenePendingMessages(Arg.Any<int>()).Returns(false);
                messagingControllersManager.ContainsController(Arg.Any<int>()).Returns(true);

                var worldState = Substitute.For<IWorldState>();
                worldState.TryGetScene(Arg.Any<int>(), out Arg.Any<IParcelScene>()).Returns(false);

                int sceneNumber = 666;

                CrdtMessage crdtMessage = new CrdtMessage
                (
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: 7693,
                    componentId: 0,
                    timestamp: 799,
                    data: new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                );

                bool messageReceived = false;

                // Check if incoming CRDT is dispatched as scene message
                void OnCrdtMessageReceived(int incommingSceneNumber, CrdtMessage incommingCrdtMessage)
                {
                    Assert.AreEqual(sceneNumber, incommingSceneNumber);
                    Assert.AreEqual(crdtMessage.EntityId, incommingCrdtMessage.EntityId);
                    Assert.AreEqual(crdtMessage.Timestamp, incommingCrdtMessage.Timestamp);
                    Assert.IsTrue(AreEqual((byte[])incommingCrdtMessage.Data, (byte[])crdtMessage.Data));
                    messageReceived = true;
                }

                context.crdt.CrdtMessageReceived += OnCrdtMessageReceived;
                context.crdt.MessagingControllersManager = messagingControllersManager;
                context.crdt.WorldState = worldState;

                // Simulate client sending `crdtMessage` CRDT
                try
                {
                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = sceneNumber,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtMessage))
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    context.crdt.CrdtMessageReceived -= OnCrdtMessageReceived;
                }

                Assert.IsTrue(messageReceived);
            });
        }

        [UnityTest]
        public IEnumerator SendCRDTtoScene()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                int scene1 = 666;
                int scene2 = 777;

                CRDTProtocol sceneState1 = new CRDTProtocol();
                CRDTProtocol sceneState2 = new CRDTProtocol();

                CrdtMessage messageToScene1 = sceneState1.CreateLwwMessage(1, 34, new byte[] { 1, 0, 2, 45, 67 });
                CrdtMessage messageToScene2 = sceneState2.CreateLwwMessage(45, 9, new byte[] { 42, 42, 42, 41 });

                sceneState1.ProcessMessage(messageToScene1);
                sceneState2.ProcessMessage(messageToScene2);

                context.crdt.scenesOutgoingCrdts.Add(scene1, new DualKeyValueSet<int, long, CrdtMessage> { });
                context.crdt.scenesOutgoingCrdts.Add(scene2, new DualKeyValueSet<int, long, CrdtMessage> { });

                context.crdt.scenesOutgoingCrdts[scene1].Add(messageToScene1.ComponentId, messageToScene1.EntityId, messageToScene1);
                context.crdt.scenesOutgoingCrdts[scene2].Add(messageToScene2.ComponentId, messageToScene2.EntityId, messageToScene2);

                // Simulate client requesting scene's crdt
                try
                {
                    ClientCRDTService clientCrdtService = await CreateClientCrdtService(testClientTransport);

                    // request for `scene1`
                    CRDTManyMessages response1 = await clientCrdtService.PullCrdt(new PullCRDTRequest() { SceneNumber = scene1 });

                    var deserializer = CRDTDeserializer.DeserializeBatch(response1.Payload.Memory);
                    deserializer.MoveNext();
                    CrdtMessage message = (CrdtMessage)deserializer.Current;

                    Assert.AreEqual(messageToScene1.EntityId, message.EntityId);
                    Assert.AreEqual(messageToScene1.Timestamp, message.Timestamp);
                    Assert.IsTrue(AreEqual((byte[])messageToScene1.Data, (byte[])message.Data));
                    Assert.IsFalse(context.crdt.scenesOutgoingCrdts.ContainsKey(scene1));

                    // request for `scene2`
                    CRDTManyMessages response2 = await clientCrdtService.PullCrdt(new PullCRDTRequest() { SceneNumber = scene2 });

                    deserializer = CRDTDeserializer.DeserializeBatch(response2.Payload.Memory);
                    deserializer.MoveNext();
                    message = (CrdtMessage)deserializer.Current;

                    Assert.AreEqual(messageToScene2.EntityId, message.EntityId);
                    Assert.AreEqual(messageToScene2.Timestamp, message.Timestamp);
                    Assert.IsTrue(AreEqual((byte[])messageToScene2.Data, (byte[])message.Data));
                    Assert.IsFalse(context.crdt.scenesOutgoingCrdts.ContainsKey(scene2));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }

        [UnityTest]
        public IEnumerator SetSceneAsInitDoneOnFirstCrdt()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                ClientCRDTService clientCrdtService = await CreateClientCrdtService(testClientTransport);

                var messagingControllersManager = Substitute.For<IMessagingControllersManager>();
                messagingControllersManager.HasScenePendingMessages(Arg.Any<int>()).Returns(false);
                messagingControllersManager.ContainsController(Arg.Any<int>()).Returns(true);

                int sceneNumber = 666;
                IParcelScene scene = Substitute.For<IParcelScene>();
                bool isInitDone = false;

                scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = sceneNumber,
                    sdk7 = true
                });

                scene.IsInitMessageDone().Returns(_ => isInitDone);

                IWorldState worldState = Substitute.For<IWorldState>();

                worldState.TryGetScene(Arg.Any<int>(), out Arg.Any<IParcelScene>())
                          .Returns(x =>
                           {
                               x[1] = scene;
                               return true;
                           });

                CrdtMessage crdtMessage = new CrdtMessage
                (
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: 7693,
                    componentId: 0,
                    timestamp: 799,
                    data: new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                );

                context.crdt.MessagingControllersManager = messagingControllersManager;
                context.crdt.WorldState = worldState;
                context.crdt.SceneController = Substitute.For<ISceneController>();

                // Simulate client sending `crdtMessage` CRDT
                try
                {
                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = sceneNumber,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtMessage))
                    });

                    context.crdt.SceneController.Received(1)
                           .EnqueueSceneMessage(Arg.Is<QueuedSceneMessage_Scene>(q =>
                                q.method == MessagingTypes.INIT_DONE
                                && q.sceneNumber == sceneNumber
                                && q.tag == "scene"
                                && q.type == QueuedSceneMessage.Type.SCENE_MESSAGE));

                    context.crdt.SceneController.ClearReceivedCalls();
                    isInitDone = true;

                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = sceneNumber,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtMessage))
                    });

                    context.crdt.SceneController.DidNotReceive().EnqueueSceneMessage(Arg.Any<QueuedSceneMessage_Scene>());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }

        static byte[] CreateCRDTMessage(CrdtMessage message)
        {
            using (MemoryStream msgStream = new MemoryStream())
            {
                using (BinaryWriter msgWriter = new BinaryWriter(msgStream))
                {
                    CRDTSerializer.Serialize(msgWriter, message);
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

        static async UniTask<ClientCRDTService> CreateClientCrdtService(ITransport transport)
        {
            RpcClient client = new RpcClient(transport);
            RpcClientPort port = await client.CreatePort("test-port");
            RpcClientModule module = await port.LoadModule(CRDTServiceCodeGen.ServiceName);
            ClientCRDTService crdtService = new ClientCRDTService(module);
            return crdtService;
        }
    }
}
