using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IMessageHandler
    {
        bool ProcessMessage(string sceneId, string id, string method, string payload, out Coroutine routine);
        void LoadParcelScenesExecute(string decentralandSceneJSON);
        void UnloadAllScenes();
        void UpdateParcelScenesExecute(string sceneKey);
    }

    public class MessagingController : IDisposable
    {
        public enum State
        {
            Initializing,
            FinishingInitializing,
            Running,
            Stopped
        }

        private const float UI_MSG_BUS_BUDGET_MAX = 0.0016f;
        private const float INIT_MSG_BUS_BUDGET_MAX = 0.003f;
        private const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.0016f;

        private const float MSG_BUS_BUDGET_MIN = 0.0001f;

        public Dictionary<string, MessagingSystem> messagingSystems = new Dictionary<string, MessagingSystem>();

        private State currentState
        {
            get;
            set;
        }

        public int pendingMessagesCount
        {
            get
            {
                int total = 0;
                using (var iterator = messagingSystems.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        total += iterator.Current.Value.bus.pendingMessagesCount;
                    }
                }
                return total;
            }
        }

        public MessagingController(IMessageHandler messageHandler)
        {
            messagingSystems.Add(MessagingBusId.UI, new MessagingSystem(messageHandler, MSG_BUS_BUDGET_MIN, UI_MSG_BUS_BUDGET_MAX, enableThrottler: false));
            messagingSystems.Add(MessagingBusId.INIT, new MessagingSystem(messageHandler, MSG_BUS_BUDGET_MIN, INIT_MSG_BUS_BUDGET_MAX, enableThrottler: true));
            messagingSystems.Add(MessagingBusId.SYSTEM, new MessagingSystem(messageHandler, MSG_BUS_BUDGET_MIN, SYSTEM_MSG_BUS_BUDGET_MAX, enableThrottler: false));

            currentState = State.Initializing;
        }

        public void Start()
        {
            if (currentState != MessagingController.State.Stopped)
            {
                currentState = MessagingController.State.Running;
            }
        }

        public void Dispose()
        {
            using (var iterator = messagingSystems.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.Dispose();
                }
            }
        }

        public float UpdateThrottling(float prevTimeBudget)
        {
            prevTimeBudget += messagingSystems[MessagingBusId.UI].Update(prevTimeBudget);
            prevTimeBudget += messagingSystems[MessagingBusId.INIT].Update(prevTimeBudget);

            if (currentState != State.Initializing && currentState != State.FinishingInitializing)
                prevTimeBudget += messagingSystems[MessagingBusId.SYSTEM].Update(prevTimeBudget);

            return prevTimeBudget;
        }

        public void ForceEnqueue(string busId, MessagingBus.QueuedSceneMessage queuedMessage)
        {
            messagingSystems[busId].Enqueue(queuedMessage);
        }

        public string Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            string busId = "";

            if (currentState != MessagingController.State.Stopped)
            {
                QueueMode queueMode = QueueMode.Reliable;

                // If current scene is the Global Scene, the bus id should be UI
                if (scene && scene.sceneData.id == SceneController.i.GlobalSceneId)
                {
                    busId = MessagingBusId.UI;
                }
                else if (currentState == MessagingController.State.Initializing)
                {
                    busId = MessagingBusId.INIT;
                }
                else
                {
                    busId = MessagingBusId.SYSTEM;
                }

                // Check if the message type is an UpdateEntityComponent 
                if (queuedMessage.method == MessagingTypes.ENTITY_COMPONENT_CREATE)
                {
                    // By default, the tag is the id of the entity/component
                    string entityId = queuedMessage.tag;

                    int classId = 0;

                    // We need to extract the entityId and the classId from the tag.
                    // The tag format is "entityId_classId", i.e: "E1_2". 
                    GetEntityIdAndClassIdFromTag(queuedMessage.tag, out entityId, out classId);

                    // If it is a transform update, the queue mode is Lossy
                    if (classId == (int)CLASS_ID_COMPONENT.TRANSFORM)
                    {
                        queueMode = QueueMode.Lossy;
                    }
                }
                // When a SCENE STARTED message is enqueued, the next messages should be 
                // enqueued in SYSTEM message bus, but we don't process them until 
                // scene started has been processed
                else if (queuedMessage.method == MessagingTypes.SCENE_STARTED)
                    currentState = MessagingController.State.FinishingInitializing;

                // When a SCENE DESTROY message is enqueued, we need to stop enqueuing
                // messages for this controller
                else if (queuedMessage.method == MessagingTypes.SCENE_DESTROY)
                    currentState = MessagingController.State.Stopped;

                messagingSystems[busId].Enqueue(queuedMessage, queueMode);
            }

            return busId;
        }

        private void GetEntityIdAndClassIdFromTag(string tag, out string entityId, out int classId)
        {
            int separator = tag.IndexOf('_');
            entityId = tag.Substring(0, separator);
            classId = System.Convert.ToInt32(tag.Substring(separator + 1));
        }

        private string FormatQueueId(string sceneId, string tag)
        {
            return sceneId + tag;
        }
    }
}
