using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class ComponentCrdtWriteSystemShould
    {
        const string SCENE_ID = "temptation";

        private ICRDTExecutor crdtExecutor;
        private ComponentCrdtWriteSystem crdtWriteSystem;

        [SetUp]
        public void SetUp()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();
            WorldState worldState = new WorldState();

            crdtExecutor = Substitute.For<ICRDTExecutor>();
            crdtExecutor.crdtProtocol.Returns(new CRDTProtocol());

            scene.crdtExecutor.Returns(crdtExecutor);
            worldState.loadedScenes.Add(SCENE_ID, scene);

            crdtWriteSystem = new ComponentCrdtWriteSystem(worldState,
                Substitute.For<ISceneController>(), DataStore.i.rpcContext.context);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void SendCrdtToSceneExecutor()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };
            long timeStamp = 0;

            crdtExecutor.WhenForAnyArgs(x => x.Execute(Arg.Any<CRDTMessage>()))
                        .Do(info =>
                        {
                            CRDTMessage crdtMessage = (CRDTMessage)info.Args()[0];
                            Assert.AreEqual(ENTITY_ID, crdtMessage.key1);
                            Assert.AreEqual(COMPONENT_ID, crdtMessage.key2);
                            Assert.AreEqual(timeStamp, crdtMessage.timestamp);
                            Assert.IsTrue(AreEqual(componentData, (byte[])crdtMessage.data));
                        });

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_LOCAL);
            crdtWriteSystem.LateUpdate();
            crdtExecutor.Received(1).Execute(Arg.Any<CRDTMessage>());
        }

        [Test]
        public void NotSendCrdtToSceneExecutor()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { };

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.LateUpdate();
            crdtExecutor.DidNotReceive().Execute(Arg.Any<CRDTMessage>());
        }

        [Test]
        public void SendCrdtToRpcService()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };
            long timeStamp = 0;

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.LateUpdate();

            DataStore.i.rpcContext.context.crdtContext.scenesOutgoingCrdts.TryGetValue(SCENE_ID, out CRDTProtocol protocol);
            Assert.NotNull(protocol);

            CRDTMessage message = protocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.NotNull(message);
            Assert.AreEqual(timeStamp, message.timestamp);
            Assert.IsTrue(AreEqual(componentData, (byte[])message.data));
        }

        [Test]
        public void NotSendCrdtToRpcService()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { };

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_LOCAL);
            crdtWriteSystem.LateUpdate();

            DataStore.i.rpcContext.context.crdtContext.scenesOutgoingCrdts.TryGetValue(SCENE_ID, out CRDTProtocol protocol);
            Assert.IsNull(protocol);
        }

        [Test]
        public void ProcessWriteComponent()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };
            long timeStamp = 0;

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.LateUpdate();

            CRDTMessage message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.NotNull(message);
            Assert.AreEqual(timeStamp, message.timestamp);
            Assert.IsTrue(AreEqual(componentData, (byte[])message.data));
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