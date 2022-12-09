using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
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
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using BinaryWriter = KernelCommunication.BinaryWriter;

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
                RpcSceneControllerServiceCodeGen.RegisterService(port, new SceneControllerServiceImpl());
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
        public IEnumerator OnlyRegisterServiceForScenePorts()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const string SERVICE_NAME = "RpcSceneControllerService";

                string invalidScenePortName = "test-...";
                UInt32 portId = 111;
                RpcServerPort<RPCContext> testPort1 = new RpcServerPort<RPCContext>(portId, invalidScenePortName, new CancellationToken());
                SceneControllerServiceImpl.RegisterService(testPort1);
                Assert.Throws(Is.TypeOf<Exception>().And.Message.EqualTo($"Module ${SERVICE_NAME} is not available for port {invalidScenePortName} ({portId}))"), () => testPort1.LoadModule(SERVICE_NAME));
                testPort1.Close();

                string validScenePortName = "scene-12324";
                RpcServerPort<RPCContext> testPort2 = new RpcServerPort<RPCContext>(111, validScenePortName, new CancellationToken());
                SceneControllerServiceImpl.RegisterService(testPort2);
                Assert.DoesNotThrow(() => testPort2.LoadModule(SERVICE_NAME));
                testPort2.Close();
            });
        }

        // ProcessLoadSceneRPCCorrectly()
        // ProcessUnloadSceneRPCCorrectly()
        // ProcessSendCrdtCorrectly()

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

                CRDTSceneMessage crdtSceneMessage = new CRDTSceneMessage()
                {
                    key1 = 7693,
                    timestamp = 799,
                    data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                };

                bool messageReceived = false;

                // Check if incoming CRDT is dispatched as scene message
                void OnCrdtMessageReceived(int incommingSceneNumber, CRDTSceneMessage incommingCrdtMessage)
                {
                    Assert.AreEqual(sceneNumber, incommingSceneNumber);
                    Assert.AreEqual(crdtSceneMessage.key1, incommingCrdtMessage.key1);
                    Assert.AreEqual(crdtSceneMessage.timestamp, incommingCrdtMessage.timestamp);
                    Assert.IsTrue(AreEqual((byte[])incommingCrdtMessage.data, (byte[])crdtSceneMessage.data));
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

        /*static async UniTask<ClientRpcSceneControllerService> CreateClientRpcSceneControllerService(ITransport transport)
        {
            RpcClient client = new RpcClient(transport);
            RpcClientPort port = await client.CreatePort("test-port");
            RpcClientModule module = await port.LoadModule(RpcSceneControllerServiceCodeGen.ServiceName);
            ClientRpcSceneControllerService clientRpcSceneControllerService = new ClientRpcSceneControllerService(module);
            return clientRpcSceneControllerService;
        }*/
    }
}
