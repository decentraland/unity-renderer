using System.Collections;
using DCL;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

namespace MessagingBusPerformanceTest
{
    public class MessagingBusPerformanceTest
    {
        private const string SEND_SCENE_MESSAGE = "SceneController.SendSceneMessage";
        private const int SEND_SCENE_UNUSED_CHARS = 3;
        private string[] dataAsJson;
        private LinkedList<QueuedSceneMessage_Scene> queuedMessages = new LinkedList<QueuedSceneMessage_Scene>();
        private string dataSource = "../TestResources/SceneMessages/SceneMessagesDump.RealData.txt";
        private IMessageProcessHandler dummyHandler = new DummyMessageHandler();
        private IEnumerator<QueuedSceneMessage_Scene> nextQueueMessage;
        private MessagingController controller;
        private MessagingControllersManager manager;

        public void SetupTests()
        {
            if (manager == null)
                manager = new MessagingControllersManager(dummyHandler);

            if (controller == null)
                controller = new MessagingController(manager, dummyHandler);

            if (nextQueueMessage == null)
            {
                SetupDataFile();
                nextQueueMessage = queuedMessages.GetEnumerator();
            }
        }

        [UnityTest, Performance]
        [Category("Explicit")]
        [Explicit]
        public IEnumerator MeasureTimeToEnqueueThousandMessages()
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

            yield return null;
        }

        [UnityTest, Performance]
        [Category("Explicit")]
        [Explicit]
        public IEnumerator MeasureTimeToProcessThousandMessages()
        {
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
                    controller.StartBus(MessagingBusType.INIT);
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

            yield return null;
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
                    raw = message.Substring(separator + 2,
                        message.Length - SEND_SCENE_MESSAGE.Length - SEND_SCENE_UNUSED_CHARS);
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
            if (!SceneMessageUtilities.DecodePayloadChunk(raw, out int sceneNumber, out string message, out string tag))
            {
                throw new InvalidDataException("Could not decode: " + raw);
            }

            return SceneMessageUtilities.DecodeSceneMessage(sceneNumber, message, tag);
        }
    }
}