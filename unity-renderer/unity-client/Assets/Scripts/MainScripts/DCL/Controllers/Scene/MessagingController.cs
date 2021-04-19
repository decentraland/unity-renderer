using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MessagingController : IDisposable
    {
        const char SEPARATOR = '_';

        public enum QueueState
        {
            Init,
            Systems,
        }

        public Dictionary<MessagingBusType, MessagingBus> messagingBuses = new Dictionary<MessagingBusType, MessagingBus>();
        public IMessageProcessHandler messageHandler;
        public string debugTag;
        public bool enabled = true;

        private QueueState currentQueueState;

        public readonly MessagingBus initBus;
        public readonly MessagingBus systemBus;
        public readonly MessagingBus uiBus;

        public MessagingController(IMessageProcessHandler messageHandler, string debugTag = null)
        {
            this.debugTag = debugTag;
            this.messageHandler = messageHandler;

            //TODO(Brian): This is too hacky, most of the controllers won't be using this system. Refactor this in the future.
            uiBus = AddMessageBus(MessagingBusType.UI);
            initBus = AddMessageBus(MessagingBusType.INIT);
            systemBus = AddMessageBus(MessagingBusType.SYSTEM);

            currentQueueState = QueueState.Init;

            StartBus(MessagingBusType.INIT);
            StartBus(MessagingBusType.UI);
        }

        private MessagingBus AddMessageBus(MessagingBusType type)
        {
            var newMessagingBus = new MessagingBus(type, messageHandler, this);
            newMessagingBus.debugTag = debugTag;

            messagingBuses.Add(type, newMessagingBus);
            return newMessagingBus;
        }

        public void StartBus(MessagingBusType busType)
        {
            if (messagingBuses.ContainsKey(busType))
            {
                messagingBuses[busType].Start();
            }
        }

        public void StopBus(MessagingBusType busType)
        {
            if (messagingBuses.ContainsKey(busType))
            {
                messagingBuses[busType].Stop();
            }
        }

        public void Stop()
        {
            using (var iterator = messagingBuses.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.Stop();
                }
            }
        }

        public void Dispose()
        {
            using (var iterator = messagingBuses.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.Dispose();
                }
            }
        }

        public void ForceEnqueue(MessagingBusType busType, QueuedSceneMessage queuedMessage)
        {
            messagingBuses[busType].Enqueue(queuedMessage);
        }

        public void Enqueue(bool isUiBus, QueuedSceneMessage_Scene queuedMessage, out MessagingBusType busType)
        {
            busType = MessagingBusType.NONE;

            QueueMode queueMode = QueueMode.Reliable;

            // If current scene is the Global Scene, the bus id should be UI
            if (isUiBus)
                busType = MessagingBusType.UI;
            else if (currentQueueState == QueueState.Init)
                busType = MessagingBusType.INIT;
            else
                busType = MessagingBusType.SYSTEM;

            // Check if the message type is an EntityComponentCreateOrUpdate 
            if (queuedMessage.payload is Protocol.EntityComponentCreateOrUpdate)
            {
                // We need to extract the entityId and the classId from the tag.
                // The tag format is "entityId_classId", i.e: "E1_2". 
                GetEntityIdAndClassIdFromTag(queuedMessage.tag, out int classId);

                // If it is a transform update, the queue mode is Lossy
                if (classId == (int) CLASS_ID_COMPONENT.TRANSFORM)
                    queueMode = QueueMode.Lossy;
            }
            else if (queuedMessage.payload is Protocol.QueryPayload)
            {
                busType = MessagingBusType.UI;
                queueMode = QueueMode.Lossy;
            }
            else if (queuedMessage.payload is Protocol.SceneReady)
            {
                // When a INIT DONE message is enqueued, the next messages should be 
                // enqueued in SYSTEM message bus, but we don't process them until 
                // scene started has been processed
                currentQueueState = QueueState.Systems;
            }

            switch (busType)
            {
                case MessagingBusType.INIT:
                    initBus.Enqueue(queuedMessage, queueMode);
                    break;
                case MessagingBusType.SYSTEM:
                    systemBus.Enqueue(queuedMessage, queueMode);
                    break;
                case MessagingBusType.UI:
                    uiBus.Enqueue(queuedMessage, queueMode);
                    break;
            }
        }

        private void GetEntityIdAndClassIdFromTag(string tag, out int classId)
        {
            int lastSeparator = tag.LastIndexOf(SEPARATOR);
            if (!int.TryParse(tag.Substring(lastSeparator + 1), out classId))
                Debug.LogError("Couldn't parse classId string to int");
        }
    }
}