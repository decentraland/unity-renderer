using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.Models;
using DCLServices.MapRendererV2;
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
using UnityEngine;
using UnityEngine.TestTools;
using BinaryWriter = KernelCommunication.BinaryWriter;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECS7HotReload
    {
        private ECS7Plugin ecs7Plugin;
        private GameObject testGameObjectForReferences;

        [SetUp]
        public void SetUp()
        {
            LoadEnvironment();
            ecs7Plugin = new ECS7Plugin();
            testGameObjectForReferences = new GameObject("TEMP_FOR_TESTING");

            DataStore.i.camera.transform.Set(testGameObjectForReferences.transform);
            DataStore.i.world.avatarTransform.Set(testGameObjectForReferences.transform);
            CommonScriptableObjects.rendererState.Set(true);
        }

        [TearDown]
        public void TearDown()
        {
            ecs7Plugin.Dispose();
            DataStore.Clear();
            CommonScriptableObjects.UnloadAll();
            Object.DestroyImmediate(testGameObjectForReferences);
        }

        [UnityTest]
        public IEnumerator HotReloadSceneCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int SCENE_NUMBER = 666;
                const int ENTITY_ID = 500;
                const int COMPONENT_ID = ComponentID.MESH_RENDERER;

                IMessage COMPONENT_DATA = new PBMeshRenderer()
                {
                    Box = new PBMeshRenderer.Types.BoxMesh()
                    {
                        Uvs = { }
                    }
                };

                RPCContext context = DataStore.i.rpc.context;

                var (clientTransport, serverTransport) = MemoryTransport.Create();

                RpcServer<RPCContext> rpcServer = new RpcServer<RPCContext>();
                rpcServer.AttachTransport(serverTransport, context);

                rpcServer.SetHandler((port, t, c) =>
                {
                    CRDTServiceCodeGen.RegisterService(port, new CRDTServiceImpl());
                });

                try
                {
                    context.crdt.MessagingControllersManager = Environment.i.messaging.manager;
                    context.crdt.WorldState = Substitute.For<IWorldState>();

                    ClientCRDTService clientCrdtService = await CreateClientCrdtService(clientTransport);
                    await LoadScene(SCENE_NUMBER);

                    IParcelScene scene = Environment.i.world.state.GetScene(SCENE_NUMBER);
                    Assert.NotNull(scene);

                    CrdtMessage crdtCreateBoxMessage = new CrdtMessage
                    (
                        type: CrdtMessageType.PUT_COMPONENT,
                        entityId: ENTITY_ID,
                        componentId: COMPONENT_ID,
                        timestamp: 1,
                        data: ProtoSerialization.Serialize(COMPONENT_DATA)
                    );

                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = SCENE_NUMBER,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtCreateBoxMessage))
                    });

                    IDCLEntity entity = scene.GetEntityById(ENTITY_ID);
                    Assert.NotNull(entity);

                    var component = ecs7Plugin.componentsManager.GetComponent(COMPONENT_ID);
                    Assert.IsTrue(component.HasComponent(scene, entity));

                    await UniTask.Yield();

                    // Do hot reload
                    await UnloadScene(SCENE_NUMBER);
                    await LoadScene(SCENE_NUMBER);

                    scene = Environment.i.world.state.GetScene(SCENE_NUMBER);
                    Assert.NotNull(scene);

                    crdtCreateBoxMessage = new CrdtMessage
                    (
                        type: CrdtMessageType.PUT_COMPONENT,
                        entityId: ENTITY_ID,
                        componentId: COMPONENT_ID,
                        timestamp: 1,
                        data: ProtoSerialization.Serialize(COMPONENT_DATA)
                    );

                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = SCENE_NUMBER,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtCreateBoxMessage))
                    });

                    entity = scene.GetEntityById(ENTITY_ID);
                    Assert.NotNull(entity);

                    component = ecs7Plugin.componentsManager.GetComponent(COMPONENT_ID);
                    Assert.IsTrue(component.HasComponent(scene, entity));

                    await UniTask.Yield();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    rpcServer.Dispose();
                }
            });
        }

        private static void LoadEnvironment()
        {
            ServiceLocator serviceLocator = ServiceLocatorFactory.CreateDefault();
            serviceLocator.Register<IMapRenderer>(() => Substitute.For<IMapRenderer>());
            Environment.Setup(serviceLocator);
        }

        private static async UniTask LoadScene(int sceneNumber)
        {
            LoadParcelScenesMessage.UnityParcelScene scene = new LoadParcelScenesMessage.UnityParcelScene()
            {
                basePosition = new Vector2Int(0, 0),
                parcels = new Vector2Int[] { new Vector2Int(0, 0) },
                sceneNumber = sceneNumber,
                sdk7 = true
            };

            Environment.i.world.sceneController.LoadParcelScenes(JsonUtility.ToJson(scene));

            var message = new QueuedSceneMessage_Scene
            {
                sceneNumber = scene.sceneNumber,
                tag = "",
                type = QueuedSceneMessage.Type.SCENE_MESSAGE,
                method = MessagingTypes.INIT_DONE,
                payload = new Protocol.SceneReady()
            };

            Environment.i.world.sceneController.EnqueueSceneMessage(message);

            await UniTask.WaitWhile(() => Environment.i.messaging.manager.HasScenePendingMessages(sceneNumber));
        }

        private static async UniTask UnloadScene(int sceneNumber)
        {
            Environment.i.world.sceneController.UnloadScene(sceneNumber);
            await UniTask.WaitWhile(() => Environment.i.messaging.manager.hasPendingMessages);
        }

        private static byte[] CreateCRDTMessage(CrdtMessage message)
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
