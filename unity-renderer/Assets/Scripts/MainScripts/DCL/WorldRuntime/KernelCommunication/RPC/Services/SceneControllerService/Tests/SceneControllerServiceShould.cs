using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using Decentraland.Renderer.RendererServices;
using Decentraland.Common;
using DCL.ECSRuntime;
using Google.Protobuf;
using KernelCommunication;
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

namespace Tests
{
    public class SceneControllerServiceShould
    {
        private RPCContext context;
        private ITransport testClientTransport;
        private RpcServer<RPCContext> rpcServer;
        private CancellationTokenSource testCancellationSource;
        private RpcServerPort<RPCContext> currentServerPort;

        [SetUp]
        public void SetUp()
        {
            context = DataStore.i.rpc.context;

            var (clientTransport, serverTransport) = MemoryTransport.Create();

            rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                currentServerPort = port;
                RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl(port));
            });

            testClientTransport = clientTransport;
            testCancellationSource = new CancellationTokenSource();

            var serviceLocator = ServiceLocatorFactory.CreateDefault();
            DCL.Environment.Setup(serviceLocator);
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
                RpcServerPort<RPCContext> testPort2 = new RpcServerPort<RPCContext>(111, validScenePortName, new CancellationToken());
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

                // Close port
                currentServerPort.Close();

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
                CrdtExecutorsManager crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController,
                    Environment.i.world.state, DataStore.i.rpc.context.crdt);


                // Prepare entity creation CRDT message
                CRDTMessage crdtMessage = new CRDTMessage()
                {
                    key1 = ENTITY_ID,
                    key2 = 0,
                    data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                };

                bool messageReceived = false;
                void OnCrdtMessageReceived(int incomingSceneNumber, CRDTMessage incomingCrdtMessage)
                {
                    Assert.AreEqual(crdtMessage.key1, incomingCrdtMessage.key1);
                    Assert.AreEqual(crdtMessage.key2, incomingCrdtMessage.key2);
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

        byte[] CreateCRDTMessage(CRDTMessage message)
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
