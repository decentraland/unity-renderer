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
            context = new RPCContext();

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

            context.crdt.SceneController = Environment.i.world.sceneController;
            context.crdt.WorldState = Environment.i.world.state;
            context.crdt.MessagingControllersManager = Environment.i.messaging.manager;
            context.crdt.GetSceneTick = (int x) => 1;
            context.crdt.IncreaseSceneTick = (int x) => { };
            context.crdt.IsSceneGltfLoadingFinished = (int x) => true;
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

                ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
                ECSComponentsManager componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
                Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>();
                context.crdt.CrdtExecutors = crdtExecutors;

                CrdtExecutorsManager crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager,
                    context.crdt.SceneController, context.crdt);

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

                // Check scene was created correctly and it has only the 2 auto-generated entities
                Assert.IsTrue(context.crdt.WorldState.TryGetScene(TEST_SCENE_NUMBER, out IParcelScene testScene));
                Assert.IsTrue(testScene.entities.Count == 2); // scenes auto-generate entities for camera and player

                // Prepare entity creation CRDT message
                CrdtMessage crdtMessage = new CrdtMessage
                (
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: ENTITY_ID,
                    componentId: 0,
                    timestamp: 799,
                    data: new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                );

                bool messageReceived = false;

                void OnCrdtMessageReceived(int incomingSceneNumber, CrdtMessage incomingCrdtMessage)
                {
                    Assert.AreEqual(crdtMessage.EntityId, incomingCrdtMessage.EntityId);
                    Assert.AreEqual(crdtMessage.ComponentId, incomingCrdtMessage.ComponentId);
                    Assert.IsTrue(AreEqual((byte[])incomingCrdtMessage.Data, (byte[])crdtMessage.Data));
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
        public IEnumerator WaitForSceneInitialGltfLoadBeforeSceneReady()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 696;
                const int ENTITY_ID = 666;

                ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
                ECSComponentsManager componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
                Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>();
                context.crdt.CrdtExecutors = crdtExecutors;

                CrdtExecutorsManager crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager,
                    context.crdt.SceneController, context.crdt);

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

                // Check scene was created correctly
                Assert.IsTrue(context.crdt.WorldState.TryGetScene(TEST_SCENE_NUMBER, out IParcelScene testScene));

                // Setup context callbacks simulating a GLTF that takes 3 frames to load
                int framesToWait = 3;
                int framesCounter = 0;

                context.crdt.IsSceneGltfLoadingFinished = (int x) =>
                {
                    framesCounter++;

                    Assert.IsFalse(testScene.IsInitMessageDone());

                    return framesCounter == framesToWait;
                };

                context.crdt.GetSceneTick = (int x) => 0; // first tick

                // Prepare entity creation CRDT message
                CrdtMessage crdtMessage = new CrdtMessage
                (
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: ENTITY_ID,
                    componentId: 0,
                    timestamp: 799,
                    data: new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                );

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

                await UniTask.Yield();

                Assert.IsTrue(testScene.IsInitMessageDone());

                crdtExecutorsManager.Dispose();
            });
        }

        [UnityTest]
        public IEnumerator CallGetCurrentStateWithoutStoredState()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 666;

                ClientRpcSceneControllerService rpcClient = await CreateRpcClient(testClientTransport);
                await rpcClient.LoadScene(CreateLoadSceneMessage(TEST_SCENE_NUMBER));
                var crdtExecutors = new Dictionary<int, ICRDTExecutor>();
                crdtExecutors.Add(TEST_SCENE_NUMBER, new CRDTExecutor(Substitute.For<IParcelScene>(), new ECSComponentsManager(null)));
                context.crdt.CrdtExecutors = crdtExecutors;

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

                await new WaitUntil(() => getCurrentStateFinished, 3);

                Assert.IsTrue(getCurrentStateFinished);
                Assert.IsFalse(sceneHasStateStored);
                Assert.IsTrue(responsePayload.IsEmpty);
            });
        }

        [UnityTest]
        public IEnumerator CallGetCurrentStateWithStoredState()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int TEST_SCENE_NUMBER = 666;

                CrdtMessage[] crdts = new CrdtMessage[]
                {
                    // outgoing crdt
                    new CrdtMessage
                    (
                        type: CrdtMessageType.PUT_COMPONENT,
                        entityId: 1,
                        componentId: 1,
                        data: new byte[] { 0, 0, 0, 0 },
                        timestamp: 0
                    ),

                    // stored crdt
                    new CrdtMessage
                    (
                        type: CrdtMessageType.PUT_COMPONENT,
                        entityId: 1,
                        componentId: 2,
                        data: new byte[] { 1, 1, 1, 1, 1, 1, 1 },
                        timestamp: 0
                    )
                };

                var storedCrdtExecutor = new CRDTExecutor(Substitute.For<IParcelScene>(), new ECSComponentsManager(null));
                storedCrdtExecutor.crdtProtocol.ProcessMessage(crdts[0]);
                storedCrdtExecutor.crdtProtocol.ProcessMessage(crdts[1]);

                var crdtExecutors = new Dictionary<int, ICRDTExecutor>();
                crdtExecutors.Add(TEST_SCENE_NUMBER, storedCrdtExecutor);
                context.crdt.CrdtExecutors = crdtExecutors;

                ClientRpcSceneControllerService rpcClient = await CreateRpcClient(testClientTransport);
                await rpcClient.LoadScene(CreateLoadSceneMessage(TEST_SCENE_NUMBER));

                bool getCurrentStateFinished = false;
                ByteString responsePayload = null;

                rpcClient.GetCurrentState(new GetCurrentStateMessage() { })
                         .ContinueWith(crdtState =>
                          {
                              responsePayload = crdtState.Payload;
                              getCurrentStateFinished = true;
                          });

                await new WaitUntil(() => getCurrentStateFinished, 3);

                Assert.IsTrue(getCurrentStateFinished);
                Assert.IsFalse(responsePayload.IsEmpty);

                int index = 0;

                using (var iterator = CRDTDeserializer.DeserializeBatch(responsePayload.Memory))
                {
                    while (iterator.MoveNext())
                    {
                        var responseCrdt = (CrdtMessage)iterator.Current;
                        Assert.AreEqual(responseCrdt.EntityId, crdts[index].EntityId);
                        Assert.AreEqual(responseCrdt.ComponentId, crdts[index].ComponentId);
                        Assert.IsTrue(AreEqual((byte[])responseCrdt.Data, (byte[])crdts[index].Data));
                        index++;
                    }
                }

                Assert.AreEqual(crdts.Length, index);
            });
        }

        byte[] CreateCRDTMessage(CrdtMessage message)
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
                Sdk7 = true,
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
