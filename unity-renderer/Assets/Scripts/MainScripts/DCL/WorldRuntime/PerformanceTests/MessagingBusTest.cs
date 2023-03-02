using System;
using System.Collections;
using DCL;
using NUnit.Framework;
using DCL.Models;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using QueueMode = DCL.QueueMode;

namespace MessagingBusTest
{
    public class MainMessagingBusTest
    {
        private string entityId;
        private IMessageProcessHandler messageProcessHandler;
        private MessagingBus bus;

        [SetUp]
        public void SetUp()
        {
            entityId = "entity";
            messageProcessHandler = Substitute.For<IMessageProcessHandler>();
            bus = new MessagingBus(MessagingBusType.SYSTEM, messageProcessHandler,
                new MessagingController(new MessagingControllersManager(messageProcessHandler), messageProcessHandler));
        }

        [UnityTest]
        public IEnumerator LossyMessageIsReplaced()
        {
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

            return null;
        }

        [UnityTest]
        public IEnumerator RemoveEntityShouldClearLossyMessages()
        {
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

            return null;
        }

        [Test]
        public void SceneMessageIsProcessedCorrectly()
        {
            //Given
            QueuedSceneMessage.Type messageType = QueuedSceneMessage.Type.SCENE_MESSAGE;

            QueuedSceneMessage_Scene queuedSceneMessageScene = new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = messageType.ToString(),
                tag = "entity_1",
                type = messageType
            };

            bus.Start();
            bus.Enqueue(queuedSceneMessageScene);
            //When
            bus.ProcessQueue(0.1f, out _);

            //Then
            messageProcessHandler.Received(1).ProcessMessage(Arg.Any<QueuedSceneMessage_Scene>(),
                out Arg.Any<CustomYieldInstruction>());
        }

        [Test]
        public void LoadParcelIsProcessedCorrectly()
        {
            //Given
            QueuedSceneMessage.Type messageType = QueuedSceneMessage.Type.LOAD_PARCEL;
            string messagePayload = messageType.ToString();

            QueuedSceneMessage_Scene queuedSceneMessageScene = new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = messagePayload,
                tag = "entity_1",
                type = messageType
            };

            bus.Start();
            bus.Enqueue(queuedSceneMessageScene);

            //When
            bus.ProcessQueue(0.1f, out _);

            //Then
            messageProcessHandler.Received(1).LoadParcelScenesExecute(messagePayload);
        }

        [Test]
        public void UnloadParcelIsProcessedCorrectly()
        {
            //Given
            QueuedSceneMessage.Type messageType = QueuedSceneMessage.Type.UNLOAD_PARCEL;
            string messagePayload = "111";

            QueuedSceneMessage_Scene queuedSceneMessageScene = new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = messagePayload,
                tag = "entity_1",
                type = messageType,
                sceneNumber = 111
            };

            bus.Start();
            bus.Enqueue(queuedSceneMessageScene);
            //When
            bus.ProcessQueue(0.1f, out _);

            //Then
            messageProcessHandler.Received(1).UnloadParcelSceneExecute(Int32.Parse(messagePayload));
        }

        [Test]
        public void UpdateParcelIsProcessedCorrectly()
        {
            //Given
            QueuedSceneMessage.Type messageType = QueuedSceneMessage.Type.UPDATE_PARCEL;
            string messagePayload = messageType.ToString();

            QueuedSceneMessage_Scene queuedSceneMessageScene = new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = messagePayload,
                tag = "entity_1",
                type = messageType
            };

            bus.Start();
            bus.Enqueue(queuedSceneMessageScene);
            //When
            bus.ProcessQueue(0.1f, out _);

            //Then
            messageProcessHandler.Received(1).UpdateParcelScenesExecute(messagePayload);
        }

        [Test]
        public void UnloadScenesIsProcessedCorrectly()
        {
            //Given
            QueuedSceneMessage.Type messageType = QueuedSceneMessage.Type.UNLOAD_SCENES;
            string messagePayload = messageType.ToString();

            QueuedSceneMessage_Scene queuedSceneMessageScene = new QueuedSceneMessage_Scene
            {
                payload = new Protocol.CreateEntity {entityId = entityId},
                message = messagePayload,
                tag = "entity_1",
                type = messageType
            };

            bus.Start();
            bus.Enqueue(queuedSceneMessageScene);
            //When
            bus.ProcessQueue(0.1f, out _);

            //Then
            messageProcessHandler.Received(1).UnloadAllScenes();
        }
    }
}