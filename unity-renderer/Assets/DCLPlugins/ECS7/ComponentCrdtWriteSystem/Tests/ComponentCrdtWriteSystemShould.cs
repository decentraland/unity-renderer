using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class ComponentCrdtWriteSystemShould
    {
        const int SCENE_NUMBER = 666;

        private ICRDTExecutor crdtExecutor;
        private ComponentCrdtWriteSystem crdtWriteSystem;

        [SetUp]
        public void SetUp()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            crdtExecutor = Substitute.For<ICRDTExecutor>();
            crdtExecutor.crdtProtocol.Returns(new CRDTProtocol());

            scene.GetParcels().Returns(new HashSet<Vector2Int>());
            scene.sceneData.sceneNumber = SCENE_NUMBER;

            crdtWriteSystem = new ComponentCrdtWriteSystem(new Dictionary<int, ICRDTExecutor>() { { SCENE_NUMBER, crdtExecutor } },
                Substitute.For<ISceneController>(), DataStore.i.rpc.context);
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
                             Assert.AreEqual(ENTITY_ID, crdtMessage.entityId);
                             Assert.AreEqual(COMPONENT_ID, crdtMessage.componentId);
                             Assert.AreEqual(timeStamp, crdtMessage.timestamp);
                             Assert.IsTrue(AreEqual(componentData, (byte[])crdtMessage.data));
                         });

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.SEND_TO_LOCAL);
            crdtWriteSystem.LateUpdate();
            crdtExecutor.Received(1).Execute(Arg.Any<CRDTMessage>());
        }

        [Test]
        public void NotSendCrdtToSceneExecutor()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { };

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.SEND_TO_SCENE);
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

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.LateUpdate();

            DataStore.i.rpc.context.crdt.scenesOutgoingCrdts.TryGetValue(SCENE_NUMBER, out DualKeyValueSet<int, long, CRDTMessage> protocol);
            // not null or empty
            Assert.NotNull(protocol);

            // The state doesn't sync with only SEND_TO_SCENE
            CRDTProtocol.EntityComponentData message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.IsNull(message);
        }

        [Test]
        public void NotSendCrdtToRpcService()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { };

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.SEND_TO_LOCAL);
            crdtWriteSystem.LateUpdate();

            DataStore.i.rpc.context.crdt.scenesOutgoingCrdts.TryGetValue(SCENE_NUMBER, out DualKeyValueSet<int, long, CRDTMessage> protocol);

            // null or empty
            Assert.IsNull(protocol);
        }

        [Test]
        public void ExecuteLocallyWithoutModifyingState()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.EXECUTE_LOCALLY);
            crdtWriteSystem.LateUpdate();

            crdtExecutor.Received(1).ExecuteWithoutStoringState(ENTITY_ID, COMPONENT_ID, Arg.Any<object>());
            crdtExecutor.DidNotReceive().Execute(Arg.Any<CRDTMessage>());

            CRDTProtocol.EntityComponentData  message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.IsNull(message);
        }

        [Test]
        public void StoreInStateWithoutExecuting()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.WRITE_STATE_LOCALLY);
            crdtWriteSystem.LateUpdate();

            crdtExecutor.DidNotReceive().ExecuteWithoutStoringState(ENTITY_ID, COMPONENT_ID, Arg.Any<object>());
            crdtExecutor.DidNotReceive().Execute(Arg.Any<CRDTMessage>());

            CRDTProtocol.EntityComponentData  message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.NotNull(message);
            Assert.AreEqual(0, message.timestamp);
            Assert.IsTrue(AreEqual(componentData, (byte[])message.data));
        }

        [Test]
        public void EnforceMinimunTimeStamp()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };
            const int minTimeStamp = 42;

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, minTimeStamp, ECSComponentWriteType.WRITE_STATE_LOCALLY);
            crdtWriteSystem.LateUpdate();

            CRDTProtocol.EntityComponentData  message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.AreEqual(minTimeStamp, message.timestamp);

            crdtWriteSystem.WriteMessage(SCENE_NUMBER, ENTITY_ID, COMPONENT_ID, componentData, -1, ECSComponentWriteType.WRITE_STATE_LOCALLY);
            crdtWriteSystem.LateUpdate();

            message = crdtExecutor.crdtProtocol.GetState(ENTITY_ID, COMPONENT_ID);
            Assert.AreEqual(minTimeStamp + 1, message.timestamp);
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
