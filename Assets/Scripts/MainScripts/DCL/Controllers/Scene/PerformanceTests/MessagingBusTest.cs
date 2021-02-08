using DCL;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using DCL.Models;
using Unity.PerformanceTesting;
using UnityEngine;
using QueueMode = DCL.QueueMode;

namespace MessagingBusTest
{
    public class MainMessagingBusTest
    {
        private const string SEND_SCENE_MESSAGE = "SceneController.SendSceneMessage";
        private const int SEND_SCENE_UNUSED_CHARS = 3;
        protected string[] dataAsJson;
        protected LinkedList<QueuedSceneMessage_Scene> queuedMessages = new LinkedList<QueuedSceneMessage_Scene>();
        protected string dataSource = "../TestResources/SceneMessages/SceneMessagesDump.RealData.txt";
        protected IMessageProcessHandler dummyHandler = new DummyMessageHandler();
        protected IEnumerator<QueuedSceneMessage_Scene> nextQueueMessage;

        // protected MessagingBus bus;
        protected MessagingController controller;

        public void SetupTests()
        {
            if (controller == null)
            {
                controller = new MessagingController(dummyHandler);
            }

            if (nextQueueMessage == null)
            {
                SetupDataFile();
                nextQueueMessage = queuedMessages.GetEnumerator();
            }
        }

        [Test, Performance]
        public void MeasureTimeToEnqueueThousandMessages()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        EnqueueNextMessage();
                    }
                })
                .SetUp(() => SetupTests())
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void MeasureTimeToProcessThousandMessages()
        {
            controller.StartBus(MessagingBusType.INIT);

            Measure.Method(() =>
                {
                    var processed = controller.initBus.processedMessagesCount;
                    Assert.IsTrue(controller.initBus.pendingMessagesCount > 1000);
                    while (controller.initBus.processedMessagesCount < processed + 1000)
                    {
                        controller.initBus.ProcessQueue(0.1f, out _);
                    }
                })
                .SetUp(() =>
                {
                    SetupTests();
                    for (var i = 0; i < 1001; i++)
                    {
                        EnqueueNextMessage();
                    }
                })
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        private void EnqueueNextMessage()
        {
            var queuedMessage = GetNextSceneMessage();
            controller.Enqueue(false, queuedMessage, out _);
        }

        private string SceneMessagesPath()
        {
            return Application.dataPath + "/" + dataSource;
        }

        private void SetupDataFile()
        {
            if (!File.Exists(SceneMessagesPath()))
            {
                throw new InvalidDataException("The file " + SceneMessagesPath() + " doesn't exist!");
            }

            var source = new StreamReader(SceneMessagesPath());
            var fileContents = source.ReadToEnd();
            source.Close();
            dataAsJson = fileContents.Split('\n');

            ParseMessagesFromDataFile();
        }

        private void ParseMessagesFromDataFile()
        {
            for (var i = 0; i < dataAsJson.Length; i++)
            {
                string message, locator, raw;
                int separator;

                message = dataAsJson[i];
                separator = message.IndexOf(' ');
                locator = "";

                if (separator != -1)
                    locator = message.Substring(0, separator);

                if (locator == SEND_SCENE_MESSAGE)
                {
                    raw = message.Substring(separator + 2, message.Length - SEND_SCENE_MESSAGE.Length - SEND_SCENE_UNUSED_CHARS);
                    queuedMessages.AddLast(ParseRawIntoQueuedMessage(raw));
                }
            }
        }

        public QueuedSceneMessage_Scene GetNextSceneMessage()
        {
            var currentMessage = nextQueueMessage.Current;
            while (currentMessage == null)
            {
                if (!nextQueueMessage.MoveNext())
                {
                    nextQueueMessage = queuedMessages.GetEnumerator();
                }

                currentMessage = nextQueueMessage.Current;
            }

            return currentMessage;
        }

        public static QueuedSceneMessage_Scene ParseRawIntoQueuedMessage(string raw)
        {
            if (!SceneMessageUtilities.DecodePayloadChunk(raw, out string sceneId, out string message, out string tag))
            {
                throw new InvalidDataException("Could not decode: " + raw);
            }

            return SceneMessageUtilities.DecodeSceneMessage(sceneId, message, tag);
        }

        [Test]
        public void LossyMessageIsReplaced()
        {
            string entityId = "entity";
            MessagingBus bus = new MessagingBus(MessagingBusType.SYSTEM, new DummyMessageHandler(), null);

            bus.Enqueue(new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = QueuedSceneMessage.Type.SCENE_MESSAGE.ToString(),
                tag = "entity_1"
            }, QueueMode.Lossy);
            bus.Enqueue(new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = QueuedSceneMessage.Type.SCENE_MESSAGE.ToString(),
                tag = "entity_1"
            }, QueueMode.Lossy);

            Assert.AreEqual(1, bus.unreliableMessagesReplaced);
            Assert.AreEqual(1, bus.pendingMessagesCount);
        }

        [Test]
        public void RemoveEntityShouldClearLossyMessages()
        {
            string entityId = "entity";
            MessagingBus bus = new MessagingBus(MessagingBusType.SYSTEM, new DummyMessageHandler(), null);

            bus.Enqueue(new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = QueuedSceneMessage.Type.SCENE_MESSAGE.ToString(),
                tag = "entity_1"
            }, QueueMode.Lossy);
            bus.Enqueue(new QueuedSceneMessage_Scene
            {
                payload = new Protocol.RemoveEntity() {entityId = entityId},
                type = QueuedSceneMessage.Type.SCENE_MESSAGE,
                method = MessagingTypes.ENTITY_DESTROY,
                message = QueuedSceneMessage.Type.SCENE_MESSAGE.ToString(),
            });
            bus.Enqueue(new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = QueuedSceneMessage.Type.SCENE_MESSAGE.ToString(),
                tag = "entity_1"
            }, QueueMode.Lossy);

            Assert.AreEqual(0, bus.unreliableMessagesReplaced);
            Assert.AreEqual(3, bus.pendingMessagesCount);
        }
    }
}