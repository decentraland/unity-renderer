using DCL;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Unity.PerformanceTesting;
using UnityEngine;

namespace MessagingBusTest
{

    public class MainMessagingBusTest
    {
        private const string SEND_SCENE_MESSAGE = "SceneController.SendSceneMessage";
        private const int SEND_SCENE_UNUSED_CHARS = 3;
        protected string[] dataAsJson;
        protected LinkedList<MessagingBus.QueuedSceneMessage_Scene> queuedMessages = new LinkedList<MessagingBus.QueuedSceneMessage_Scene>();
        protected string dataSource = "../TestResources/SceneMessages/SceneMessagesDump.RealData.txt";
        protected IMessageHandler dummyHandler = new DummyMessageHandler();
        protected IEnumerator<MessagingBus.QueuedSceneMessage_Scene> nextQueueMessage;
        protected MessagingBus bus;

        public void SetupTests()
        {
            if (bus == null)
            {
                bus = new MessagingBus("bus", dummyHandler, null, 0.1f, 1f);
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
            bus.Start();
            Measure.Method(() =>
            {
                var processed = bus.processedMessagesCount;
                Assert.IsTrue(bus.pendingMessagesCount > 1000);
                while (bus.processedMessagesCount < processed + 1000)
                {
                    System.Collections.IEnumerator yieldReturn;
                    bus.ProcessQueue(0.1f, out yieldReturn);
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
            bus.Enqueue(queuedMessage);
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

        public MessagingBus.QueuedSceneMessage_Scene GetNextSceneMessage()
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

        public static MessagingBus.QueuedSceneMessage_Scene ParseRawIntoQueuedMessage(string raw)
        {
            if (!SceneMessageUtilities.DecodePayloadChunk(raw, out string sceneId, out string message, out string tag))
            {
                throw new InvalidDataException("Could not decode: " + raw);
            }

            return SceneMessageUtilities.DecodeSceneMessage(sceneId, message, tag);
        }

    }

    internal class DummyMessageHandler : IMessageHandler
    {
        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
        }

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            return true;
        }

        public void UnloadAllScenes()
        {
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
        }

        public void UpdateParcelScenesExecute(string sceneKey)
        {
        }
    }

}
