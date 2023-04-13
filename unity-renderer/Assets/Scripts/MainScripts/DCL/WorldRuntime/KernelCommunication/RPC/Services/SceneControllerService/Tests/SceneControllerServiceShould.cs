using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCLServices.MapRendererV2;
using Decentraland.Common;
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using BinaryWriter = KernelCommunication.BinaryWriter;
using Environment = DCL.Environment;
using WaitUntil = DCL.WaitUntil;

namespace Tests
{
    public class SceneControllerServiceShould
    {
        private RPCContext context;
        private ITransport testClientTransport;
        private RpcServer<RPCContext> rpcServer;
        private CancellationTokenSource testCancellationSource;

        [SetUp]
        public void SetUp()
        {
            context = DataStore.i.rpc.context;

            var (clientTransport, serverTransport) = MemoryTransport.Create();

            rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl(port));
            });

            testClientTransport = clientTransport;
            testCancellationSource = new CancellationTokenSource();

            var serviceLocator = ServiceLocatorFactory.CreateDefault();
            serviceLocator.Register<IMapRenderer>(() => Substitute.For<IMapRenderer>());
            Environment.Setup(serviceLocator);
        }

        [TearDown]
        public void TearDown()
        {
            rpcServer.Dispose();
            testCancellationSource.Cancel();
            testCancellationSource.Dispose();
            DataStore.Clear();
        }

        [UnityTest]
        public IEnumerator OnlyRegisterServiceForScenePorts()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                string invalidScenePortName = "test-...";
                UInt32 portId = 111;
                RpcServerPort<RPCContext> testPort1 = new RpcServerPort<RPCContext>(portId, invalidScenePortName, new CancellationToken());
                SceneControllerServiceImpl.RegisterService(testPort1);
                Assert.Throws(Is.TypeOf<Exception>().And.Message.EqualTo($"Module ${RpcSceneControllerServiceCodeGen.ServiceName} is not available for port {invalidScenePortName} ({portId}))"), () => testPort1.LoadModule(RpcSceneControllerServiceCodeGen.ServiceName));
                testPort1.Close();

                string validScenePortName = "scene-12324";
                RpcServerPort<RPCContext> testPort2 = new RpcServerPort<RPCContext>(portId, validScenePortName, new CancellationToken());
                SceneControllerServiceImpl.RegisterService(testPort2);
                Assert.DoesNotThrow(() => testPort2.LoadModule(RpcSceneControllerServiceCodeGen.ServiceName));
                testPort2.Close();
            });
        }

        [UnityTest]
        public IEnumerator LoadAndUnloadScenesCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var rpcClient = await CreateRpcClient(testClientTransport);
                int testSceneNumber = 987;

                // Check scene is not already loaded
                Assert.IsFalse(context.crdt.WorldState.ContainsScene(testSceneNumber));

                // Simulate client requesting `LoadScene()`...
                try
                {
                    await rpcClient.LoadScene(CreateLoadSceneMessage(testSceneNumber));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                // Check scene loaded
                Assert.IsTrue(context.crdt.WorldState.ContainsScene(testSceneNumber));

                // Simulate client requesting `UnloadScene()`...
                try
                {
                    await rpcClient.UnloadScene(new UnloadSceneMessage());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                // Check scene unloaded
                Assert.IsFalse(context.crdt.WorldState.ContainsScene(testSceneNumber));
            });
        }

        [UnityTest]
        public IEnumerator UnloadSceneIfPortIsClosed()
        {
            var (clientTransport, serverTransport) = MemoryTransport.Create();

            var testRpcServer = new RpcServer<RPCContext>();
            testRpcServer.AttachTransport(serverTransport, context);

            RpcServerPort<RPCContext> currentServerPort = null;

            testRpcServer.SetHandler((port, t, c) =>
            {
                currentServerPort = port;
                RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl(port));
            });

            yield return UniTask.ToCoroutine(async () =>
            {
                var rpcClient = await CreateRpcClient(clientTransport);

                int testSceneNumber = 987;

                // Check scene is not already loaded
                Assert.IsFalse(context.crdt.WorldState.ContainsScene(testSceneNumber));

                // Simulate client requesting `LoadScene()`...
                try
                {
                    await rpcClient.LoadScene(CreateLoadSceneMessage(testSceneNumber));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                // Check scene loaded
                Assert.IsTrue(context.crdt.WorldState.ContainsScene(testSceneNumber));

                // Close port
                testRpcServer.Dispose();

                // Check scene unloaded
                Assert.IsFalse(context.crdt.WorldState.ContainsScene(testSceneNumber));
            });
        }

        [UnityTest]
        public IEnumerator ProcessSceneCRDTMessagesCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 696;
                const int ENTITY_ID = 666;
                ClientRpcSceneControllerService rpcClient = await CreateRpcClient(testClientTransport);

                // client requests `LoadScene()` to have the port open with a scene ready to receive crdt messages
                try
                {
                    await rpcClient.LoadScene(CreateLoadSceneMessage(TEST_SCENE_NUMBER));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                // Check scene was created correctly and it has no entities
                Assert.IsTrue(context.crdt.WorldState.TryGetScene(TEST_SCENE_NUMBER, out IParcelScene testScene));
                Assert.IsTrue(testScene.entities.Count == 0);

                ISceneController sceneController = Environment.i.world.sceneController;
                ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
                ECSComponentsManager componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
                Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(1);
                crdtExecutors.Add(TEST_SCENE_NUMBER, new CRDTExecutor(testScene, componentsManager));

                CrdtExecutorsManager crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager,
                    sceneController, DataStore.i.rpc.context.crdt);

                // Prepare entity creation CRDT message
                CRDTMessage crdtMessage = new CRDTMessage()
                {
                    type = CrdtMessageType.PUT_COMPONENT,
                    entityId = ENTITY_ID,
                    componentId = 0,
                    data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                };

                bool messageReceived = false;

                void OnCrdtMessageReceived(int incomingSceneNumber, CRDTMessage incomingCrdtMessage)
                {
                    Assert.AreEqual(crdtMessage.entityId, incomingCrdtMessage.entityId);
                    Assert.AreEqual(crdtMessage.componentId, incomingCrdtMessage.componentId);
                    Assert.IsTrue(AreEqual((byte[])incomingCrdtMessage.data, (byte[])crdtMessage.data));
                    messageReceived = true;
                }

                context.crdt.CrdtMessageReceived += OnCrdtMessageReceived;

                // Send entity creation CRDT message
                try
                {
                    await rpcClient.SendCrdt(new CRDTSceneMessage()
                    {
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

                // Check message received correctly, and entity created correctly
                Assert.IsTrue(messageReceived);
                Assert.IsTrue(testScene.entities.ContainsKey(ENTITY_ID));

                crdtExecutorsManager.Dispose();
            });
        }

        [UnityTest]
        [NUnit.Framework.Explicit("Flakytest, issue to fix it: https://github.com/decentraland/unity-renderer/issues/4222")]
        [Category("Explicit")]
        public IEnumerator CallGetCurrentStateWithoutStoredState()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 666;
                const int ENTITY_ID = 1;
                const int COMPONENT_ID = 1;
                byte[] outgoingCrdtBytes = new byte[] { 0, 0, 0, 0 };

                ClientRpcSceneControllerService rpcClient = await CreateRpcClient(testClientTransport);
                await rpcClient.LoadScene(CreateLoadSceneMessage(TEST_SCENE_NUMBER));

                bool sceneHasStateStored = true;
                bool getCurrentStateFinished = false;
                ByteString responsePayload = null;

                rpcClient.GetCurrentState(new GetCurrentStateMessage() { })
                         .ContinueWith(crdtState =>
                          {
                              sceneHasStateStored = crdtState.HasOwnEntities;
                              responsePayload = crdtState.Payload;
                              getCurrentStateFinished = true;
                          });

                var protocol = new CRDTProtocol() { };
                var msg = protocol.CreateLwwMessage(ENTITY_ID, COMPONENT_ID, outgoingCrdtBytes);
                context.crdt.scenesOutgoingCrdts.Add(TEST_SCENE_NUMBER, new DualKeyValueSet<int, long, CRDTMessage>());
                context.crdt.scenesOutgoingCrdts[TEST_SCENE_NUMBER].Add(msg.componentId, msg.entityId, msg);
                await new WaitUntil(() => getCurrentStateFinished, 1);

                Assert.IsTrue(getCurrentStateFinished);
                Assert.IsFalse(sceneHasStateStored);
                Assert.IsFalse(responsePayload.IsEmpty);

                using (var iterator = CRDTDeserializer.DeserializeBatch(responsePayload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        var responseCrdt = (CRDTMessage)iterator.Current;
                        Assert.AreEqual(responseCrdt.entityId, ENTITY_ID);
                        Assert.AreEqual(responseCrdt.componentId, COMPONENT_ID);
                        Assert.IsTrue(AreEqual(outgoingCrdtBytes, (byte[])responseCrdt.data));
                    }
                }
            });
        }

        [UnityTest]
        [NUnit.Framework.Explicit("Flakytest, issue to fix it: https://github.com/decentraland/unity-renderer/issues/4222")]
        [Category("Explicit")]
        public IEnumerator CallGetCurrentStateWithStoredState()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 666;
                CRDTMessage[] crdts = new CRDTMessage[]
                {
                    // outgoing crdt
                    new CRDTMessage()
                    {
                        type = CrdtMessageType.PUT_COMPONENT,
                        entityId = 1,
                        componentId = 1,
                        data = new byte[] { 0, 0, 0, 0 },
                    },

                    // stored crdt
                    new CRDTMessage()
                    {
                        type = CrdtMessageType.PUT_COMPONENT,
                        entityId = 1,
                        componentId = 2,
                        data = new byte[] { 1, 1, 1, 1, 1, 1, 1 }
                    }
                };

                var storedCrdtExecutor = new CRDTExecutor(Substitute.For<IParcelScene>(), new ECSComponentsManager(null));
                storedCrdtExecutor.crdtProtocol.ProcessMessage(crdts[1]);
                context.crdt.CrdtExecutors = new Dictionary<int, ICRDTExecutor>();
                context.crdt.CrdtExecutors.Add(TEST_SCENE_NUMBER, storedCrdtExecutor);

                ClientRpcSceneControllerService rpcClient = await CreateRpcClient(testClientTransport);
                await rpcClient.LoadScene(CreateLoadSceneMessage(TEST_SCENE_NUMBER));

                bool sceneHasStateStored = false;
                bool getCurrentStateFinished = false;
                ByteString responsePayload = null;

                rpcClient.GetCurrentState(new GetCurrentStateMessage() { })
                         .ContinueWith(crdtState =>
                          {
                              sceneHasStateStored = crdtState.HasOwnEntities;
                              responsePayload = crdtState.Payload;
                              getCurrentStateFinished = true;
                          });

                var outgoingCrdtProtocol = new CRDTProtocol() { };
                outgoingCrdtProtocol.ProcessMessage(crdts[0]);
                context.crdt.scenesOutgoingCrdts.Add(TEST_SCENE_NUMBER, new DualKeyValueSet<int, long, CRDTMessage>());
                context.crdt.scenesOutgoingCrdts[TEST_SCENE_NUMBER].Add(crdts[0].componentId, crdts[0].entityId, crdts[0]);
                await new WaitUntil(() => getCurrentStateFinished, 1);

                Assert.IsTrue(getCurrentStateFinished);
                Assert.IsTrue(sceneHasStateStored);
                Assert.IsFalse(responsePayload.IsEmpty);

                int index = 0;

                using (var iterator = CRDTDeserializer.DeserializeBatch(responsePayload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        var responseCrdt = (CRDTMessage)iterator.Current;
                        Assert.AreEqual(responseCrdt.entityId, crdts[index].entityId);
                        Assert.AreEqual(responseCrdt.componentId, crdts[index].componentId);
                        Assert.IsTrue(AreEqual((byte[])responseCrdt.data, (byte[])crdts[index].data));
                        index++;
                    }
                }

                Assert.AreEqual(crdts.Length, index);
            });
        }

        byte[] CreateCRDTMessage(CRDTMessage message)
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

        LoadSceneMessage CreateLoadSceneMessage(int sceneNumber)
        {
            return new LoadSceneMessage()
            {
                SceneNumber = sceneNumber,
                Sdk7 = false,
                IsPortableExperience = false,
                IsGlobalScene = false,
                BaseUrl = "testUrl",
                BaseUrlAssetBundles = "testUrl",
                Entity = new Entity()
                {
                    Id = "temptation",
                    Metadata = JsonUtility.ToJson(new CatalystSceneEntityMetadata()
                    {
                        scene = new CatalystSceneEntityMetadata.Scene()
                        {
                            @base = "0,0",
                            parcels = new string[] { "0,0" }
                        }
                    })
                }
            };
        }

        bool AreEqual(byte[] a, byte[] b)
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

        async UniTask<ClientRpcSceneControllerService> CreateRpcClient(ITransport transport)
        {
            RpcClient client = new RpcClient(transport);
            RpcClientPort port = await client.CreatePort("scene-666999");
            RpcClientModule module = await port.LoadModule(RpcSceneControllerServiceCodeGen.ServiceName);
            ClientRpcSceneControllerService clientRpcSceneControllerService = new ClientRpcSceneControllerService(module);
            return clientRpcSceneControllerService;
        }
    }
}
