using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Google.Protobuf;
using KernelCommunication;
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
                RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl());
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
                    await rpcClient.LoadScene(new LoadSceneMessage()
                    {
                        SceneNumber = testSceneNumber,
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
                    });
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

        /*[UnityTest]
        public IEnumerator ProcessIncomingSceneCRDTMessage()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                ClientRpcSceneControllerService clientRpcSceneControllerService = await CreateClientRpcSceneControllerService(testClientTransport);

                var messagingControllersManager = Substitute.For<IMessagingControllersManager>();
                messagingControllersManager.HasScenePendingMessages(Arg.Any<int>()).Returns(false);
                messagingControllersManager.ContainsController(Arg.Any<int>()).Returns(true);

                var worldState = Substitute.For<IWorldState>();
                worldState.TryGetScene(Arg.Any<int>(), out Arg.Any<IParcelScene>()).Returns(false);

                int sceneNumber = 666;

                CRDTMessage crdtMessage = new CRDTMessage()
                {
                    key1 = 7693,
                    timestamp = 799,
                    data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                };

                bool messageReceived = false;

                // Check if incoming CRDT is dispatched as scene message
                void OnCrdtMessageReceived(int incomingSceneNumber, CRDTMessage incomingCrdtMessage)
                {
                    Assert.AreEqual(sceneNumber, incomingSceneNumber);
                    Assert.AreEqual(crdtMessage.key1, incomingCrdtMessage.key1);
                    Assert.AreEqual(crdtMessage.timestamp, incomingCrdtMessage.timestamp);
                    Assert.IsTrue(AreEqual((byte[])incomingCrdtMessage.data, (byte[])crdtMessage.data));
                    messageReceived = true;
                }

                context.crdt.CrdtMessageReceived += OnCrdtMessageReceived;
                context.crdt.MessagingControllersManager = messagingControllersManager;
                context.crdt.WorldState = worldState;

                // Simulate client sending `CRDTSceneMessage`...
                try
                {
                    await clientRpcSceneControllerService.SendCrdt(new CRDTSceneMessage()
                    {
                        SceneNumber = sceneNumber,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtSceneMessage))
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
        }*/

        /*static CRDTSceneMessage CreateCRDTSceneMessage(CRDTMessage message)
        {
            using (MemoryStream msgStream = new MemoryStream())
            {
                using (BinaryWriter msgWriter = new BinaryWriter(msgStream))
                {
                    KernelBinaryMessageSerializer.Serialize(msgWriter, message);
                    return new CRDTSceneMessage() { Payload = msgStream.ToString() };
                }
            }
        }*/

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

        static async UniTask<ClientRpcSceneControllerService> CreateRpcClient(ITransport transport)
        {
            RpcClient client = new RpcClient(transport);
            RpcClientPort port = await client.CreatePort("scene-666999");
            RpcClientModule module = await port.LoadModule(RpcSceneControllerServiceCodeGen.ServiceName);
            ClientRpcSceneControllerService clientRpcSceneControllerService = new ClientRpcSceneControllerService(module);
            return clientRpcSceneControllerService;
        }
    }
}
