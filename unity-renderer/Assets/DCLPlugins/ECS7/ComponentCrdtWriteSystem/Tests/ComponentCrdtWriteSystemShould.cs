using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using KernelCommunication;
using NSubstitute;
using NUnit.Framework;
using BinaryWriter = KernelCommunication.BinaryWriter;

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
            IWorldState worldState = Substitute.For<IWorldState>();
            IUpdateEventHandler updateHandler = Substitute.For<IUpdateEventHandler>();

            crdtExecutor = Substitute.For<ICRDTExecutor>();
            crdtExecutor.crdtProtocol.Returns(new CRDTProtocol());

            scene.crdtExecutor.Returns(crdtExecutor);
            worldState.loadedScenes.Returns(new Dictionary<string, IParcelScene>() { { SCENE_ID, scene } });

            crdtWriteSystem = new ComponentCrdtWriteSystem(updateHandler, worldState, DataStore.i.rpcContext.context);
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
                            Assert.AreEqual(CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID), crdtMessage.key);
                            Assert.AreEqual(timeStamp, crdtMessage.timestamp);
                            Assert.IsTrue(AreEqual(componentData, (byte[])crdtMessage.data));
                        });

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_LOCAL);
            crdtExecutor.Received(1).Execute(Arg.Any<CRDTMessage>());
        }

        [Test]
        public void SendBinaryMessage()
        {
            const int ENTITY_ID = 345345;
            const int COMPONENT_ID = 3452;
            byte[] componentData = new byte[] { 3, 67, 2, 3 };

            //testing CRDTMessage
            CRDTMessage message = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID),
                timestamp = 0,
                data = componentData
            };

            // write component
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_SCENE);

            // process message so it is serialized and sent to WebInterface
            crdtWriteSystem.ProcessMessages();

            // get the expected binary message and compare with the one that is being sent to WebInterface
            byte[] expectedBinaryMessage = SerializeCRDTMessage(message);
            var (sceneId, bytes) = DataStore.i.rpcContext.context.crdtContext.notifications.Dequeue();
            Assert.AreEqual(SCENE_ID, sceneId);
            Assert.IsTrue(AreEqual(expectedBinaryMessage, bytes));
        }

        [Test]
        public void JoinCrdtInSameBinaryMessage()
        {
            const int ENTITY_ID = 345345;
            const int COMPONENT_ID_0 = 3452;
            const int COMPONENT_ID_1 = 574593;

            byte[] componentData0 = new byte[] { 154, 234, 77, 122, 66 };
            byte[] componentData1 = new byte[] { 56, 6, 232, 43 };

            //testing crdt messages
            CRDTMessage message0 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_0),
                timestamp = 0,
                data = componentData0
            };
            CRDTMessage message1 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_1),
                timestamp = 0,
                data = componentData1
            };

            // write components
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_0, componentData0, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_1, componentData1, ECSComponentWriteType.SEND_TO_SCENE);
            Assert.AreEqual(2, crdtWriteSystem.queuedMessages[SCENE_ID].Count);

            // get expected messages
            var expectedSerialized = SerializeCRDTMessage(new List<CRDTMessage>() { message0, message1 });

            // process messages
            crdtWriteSystem.ProcessMessages();

            var (sceneId, bytes) = DataStore.i.rpcContext.context.crdtContext.notifications.Dequeue();
            Assert.AreEqual(SCENE_ID, sceneId);
            Assert.IsTrue(AreEqual(expectedSerialized, bytes));

            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);
        }

        [Test]
        public void SplitMessageIfTooBig()
        {
            const int ENTITY_ID = 345345;
            const int COMPONENT_ID_0 = 3452;
            const int COMPONENT_ID_1 = 574593;

            byte[] componentData0 = new byte[ComponentCrdtWriteSystem.BINARY_MSG_MAX_SIZE];
            for (int i = 0; i < ComponentCrdtWriteSystem.BINARY_MSG_MAX_SIZE; i++)
            {
                componentData0[i] = 1;
            }
            byte[] componentData1 = new byte[] { 56, 6, 232, 43 };

            //testing crdt messages
            CRDTMessage message0 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_0),
                timestamp = 0,
                data = componentData0
            };
            CRDTMessage message1 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_1),
                timestamp = 0,
                data = componentData1
            };

            // write components
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_0, componentData0, ECSComponentWriteType.SEND_TO_SCENE);
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_1, componentData1, ECSComponentWriteType.SEND_TO_SCENE);
            Assert.AreEqual(2, crdtWriteSystem.queuedMessages[SCENE_ID].Count);

            // get expected messages
            var expectedSerialized0 = SerializeCRDTMessage(message0);
            var expectedSerialized1 = SerializeCRDTMessage(message1);

            // process messages (it should take 2 iterations)
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(1, crdtWriteSystem.queuedMessages[SCENE_ID].Count);

            var (sceneId0, bytes0) = DataStore.i.rpcContext.context.crdtContext.notifications.Dequeue();
            Assert.AreEqual(SCENE_ID, sceneId0);
            Assert.IsTrue(AreEqual(expectedSerialized0, bytes0));

            // 2nd iteration
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);

            var (sceneId1, bytes1) = DataStore.i.rpcContext.context.crdtContext.notifications.Dequeue();
            Assert.AreEqual(SCENE_ID, sceneId1);
            Assert.IsTrue(AreEqual(expectedSerialized1, bytes1));
        }

        [Test]
        public void SendBigMessage()
        {
            const int ENTITY_ID = 345345;
            const int COMPONENT_ID = 3452;

            byte[] componentData = new byte[ComponentCrdtWriteSystem.BINARY_MSG_MAX_SIZE + 1];
            for (int i = 0; i < componentData.Length; i++)
            {
                componentData[i] = 1;
            }

            //testing CRDTMessage
            CRDTMessage message = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID),
                timestamp = 0,
                data = componentData
            };

            // write component
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData, ECSComponentWriteType.SEND_TO_SCENE);
            Assert.AreEqual(1, crdtWriteSystem.queuedMessages.Count);

            byte[] expectedBinaryMessage = SerializeCRDTMessage(message);

            // process message so it is serialized and sent to WebInterface
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);

            var (sceneId, bytes) = DataStore.i.rpcContext.context.crdtContext.notifications.Dequeue();
            Assert.AreEqual(SCENE_ID, sceneId);
            Assert.IsTrue(AreEqual(expectedBinaryMessage, bytes));
        }

        static byte[] SerializeCRDTMessage(CRDTMessage message)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            KernelBinaryMessageSerializer.Serialize(binaryWriter, message);
            byte[] result = stream.ToArray();
            binaryWriter.Dispose();
            stream.Dispose();
            return result;
        }

        static byte[] SerializeCRDTMessage(List<CRDTMessage> messages)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);

            foreach (var message in messages)
            {
                KernelBinaryMessageSerializer.Serialize(binaryWriter, message);
            }

            byte[] result = stream.ToArray();
            binaryWriter.Dispose();
            stream.Dispose();
            return result;
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