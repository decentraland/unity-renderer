using System;
using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.Interface;
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

            scene.crdtExecutor.Returns(crdtExecutor);
            worldState.loadedScenes.Returns(new Dictionary<string, IParcelScene>() { { SCENE_ID, scene } });

            crdtWriteSystem = new ComponentCrdtWriteSystem(updateHandler, worldState);
        }

        [Test]
        public void SendCrdtToSceneExecutor()
        {
            const int ENTITY_ID = 42;
            const int COMPONENT_ID = 2134;

            byte[] componentData = new byte[] { 1, 0, 0, 1 };
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            crdtExecutor.WhenForAnyArgs(x => x.Execute(Arg.Any<CRDTMessage>()))
                        .Do(info =>
                        {
                            CRDTMessage crdtMessage = (CRDTMessage)info.Args()[0];
                            Assert.AreEqual(CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID), crdtMessage.key);
                            Assert.AreEqual(timeStamp, crdtMessage.timestamp);
                            Assert.IsTrue(AreEqual(componentData, (byte[])crdtMessage.data));
                        });

            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData);
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
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData
            };

            // write component
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData);

            var subscriber = Substitute.For<IDummyEventSubscriber<string, byte[]>>();
            WebInterface.OnBinaryMessageFromEngine += subscriber.React;

            // get the expected binary message and compare with the one that is being sent to WebInterface
            byte[] expectedBinaryMessage = SerializeCRDTMessage(message);
            subscriber.WhenForAnyArgs(x => x.React(Arg.Any<string>(), Arg.Any<byte[]>()))
                      .Do(info =>
                      {
                          byte[] bytes = (byte[])info.Args()[1];
                          Assert.IsTrue(AreEqual(expectedBinaryMessage, bytes));
                      });

            // process message so it is serialized and sent to WebInterface
            crdtWriteSystem.ProcessMessages();

            WebInterface.OnBinaryMessageFromEngine -= subscriber.React;
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
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData0
            };
            CRDTMessage message1 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_1),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData1
            };

            // write components
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_0, componentData0);
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_1, componentData1);
            Assert.AreEqual(2, crdtWriteSystem.queuedMessages[SCENE_ID].Count);

            // get expected messages
            var expectedSerialized = SerializeCRDTMessage(new List<CRDTMessage>() { message0, message1 });

            // subscriber
            var subscriber = Substitute.For<IDummyEventSubscriber<string, byte[]>>();
            subscriber.WhenForAnyArgs(x => x.React(Arg.Any<string>(), Arg.Any<byte[]>()))
                      .Do(info =>
                      {
                          byte[] bytes = (byte[])info.Args()[1];
                          Assert.IsTrue(AreEqual(expectedSerialized, bytes));
                      });

            // process messages (it should take 2 iterations)
            WebInterface.OnBinaryMessageFromEngine += subscriber.React;
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);
            WebInterface.OnBinaryMessageFromEngine -= subscriber.React;
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
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData0
            };
            CRDTMessage message1 = new CRDTMessage()
            {
                key = CRDTUtils.KeyFromIds(ENTITY_ID, COMPONENT_ID_1),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData1
            };

            // write components
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_0, componentData0);
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID_1, componentData1);
            Assert.AreEqual(2, crdtWriteSystem.queuedMessages[SCENE_ID].Count);

            // get expected messages
            var expectedSerialized0 = SerializeCRDTMessage(message0);
            var expectedSerialized1 = SerializeCRDTMessage(message1);

            // subscribers
            var subscriber0 = Substitute.For<IDummyEventSubscriber<string, byte[]>>();
            subscriber0.WhenForAnyArgs(x => x.React(Arg.Any<string>(), Arg.Any<byte[]>()))
                       .Do(info =>
                       {
                           byte[] bytes = (byte[])info.Args()[1];
                           Assert.IsTrue(AreEqual(expectedSerialized0, bytes));
                       });
            var subscriber1 = Substitute.For<IDummyEventSubscriber<string, byte[]>>();
            subscriber1.WhenForAnyArgs(x => x.React(Arg.Any<string>(), Arg.Any<byte[]>()))
                       .Do(info =>
                       {
                           byte[] bytes = (byte[])info.Args()[1];
                           Assert.IsTrue(AreEqual(expectedSerialized1, bytes));
                       });

            // process messages (it should take 2 iterations)
            WebInterface.OnBinaryMessageFromEngine += subscriber0.React;
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(1, crdtWriteSystem.queuedMessages[SCENE_ID].Count);
            WebInterface.OnBinaryMessageFromEngine -= subscriber0.React;

            WebInterface.OnBinaryMessageFromEngine += subscriber1.React;
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);
            WebInterface.OnBinaryMessageFromEngine -= subscriber1.React;
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
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                data = componentData
            };

            // write component
            crdtWriteSystem.WriteMessage(SCENE_ID, ENTITY_ID, COMPONENT_ID, componentData);
            Assert.AreEqual(1, crdtWriteSystem.queuedMessages.Count);

            var subscriber = Substitute.For<IDummyEventSubscriber<string, byte[]>>();
            WebInterface.OnBinaryMessageFromEngine += subscriber.React;

            // get the expected binary message and compare with the one that is being sent to WebInterface
            byte[] expectedBinaryMessage = SerializeCRDTMessage(message);
            subscriber.WhenForAnyArgs(x => x.React(Arg.Any<string>(), Arg.Any<byte[]>()))
                      .Do(info =>
                      {
                          byte[] bytes = (byte[])info.Args()[1];
                          Assert.IsTrue(AreEqual(expectedBinaryMessage, bytes));
                      });

            // process message so it is serialized and sent to WebInterface
            crdtWriteSystem.ProcessMessages();
            Assert.AreEqual(0, crdtWriteSystem.queuedMessages.Count);

            WebInterface.OnBinaryMessageFromEngine -= subscriber.React;
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