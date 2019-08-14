using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class CleanableYieldInstruction : CustomYieldInstruction, ICleanable
    {
        public virtual void Cleanup()
        {
        }

        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    public interface IMessageHandler
    {
        bool ProcessMessage(string sceneId, string id, string method, string payload, out CleanableYieldInstruction yieldInstruction);
        void LoadParcelScenesExecute(string decentralandSceneJSON);
        void UnloadParcelSceneExecute(string sceneKey);
        void UnloadAllScenes();
        void UpdateParcelScenesExecute(string sceneKey);
    }

    public class MessagingController : IDisposable
    {
        public enum QueueState
        {
            Init,
            Systems,
        }

        //TODO(Brian): Improve this. We should distribute the budget between the scenes.
        public const float UI_MSG_BUS_BUDGET_MAX = 0.01f;

        public const float INIT_MSG_BUS_BUDGET_MAX = 0.2f;
        public const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.01f;

        public const float MSG_BUS_BUDGET_MIN = 0.0001f;

        public Dictionary<string, MessagingSystem> messagingSystems = new Dictionary<string, MessagingSystem>();
        IMessageHandler messageHandler;
        public string debugTag;

        private QueueState currentQueueState
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

        public int pendingInitMessagesCount
        {
            get
            {
                int total = 0;

                using (var iterator = messagingSystems.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        if (iterator.Current.Value.id == MessagingBusId.INIT)
                        {
                            total += iterator.Current.Value.bus.pendingMessagesCount;
                        }
                    }
                }

                return total;
            }
        }

        public MessagingController(IMessageHandler messageHandler, string debugTag = null)
        {
            this.debugTag = debugTag;
            this.messageHandler = messageHandler;

            //TODO(Brian): This is too hacky, most of the controllers won't be using this system. Refactor this in the future.
            AddMessageSystem(MessagingBusId.UI, budgetMin: MSG_BUS_BUDGET_MIN, budgetMax: UI_MSG_BUS_BUDGET_MAX);

            AddMessageSystem(MessagingBusId.INIT, budgetMin: MSG_BUS_BUDGET_MIN, budgetMax: INIT_MSG_BUS_BUDGET_MAX, enableThrottler: true);

            AddMessageSystem(MessagingBusId.SYSTEM, budgetMin: MSG_BUS_BUDGET_MIN, budgetMax: SYSTEM_MSG_BUS_BUDGET_MAX);

            currentQueueState = QueueState.Init;

            StartBus(MessagingBusId.INIT);
            StartBus(MessagingBusId.UI);
        }

        private MessagingSystem AddMessageSystem(string id, float budgetMin, float budgetMax, bool enableThrottler = false)
        {
            var newMessagingSystem = new MessagingSystem(id, messageHandler, budgetMin, budgetMax);

            newMessagingSystem.debugTag = debugTag;

            if (enableThrottler)
                newMessagingSystem.throttler = new MessageThrottlingController();

            messagingSystems.Add(id, newMessagingSystem);
            return newMessagingSystem;
        }

        public void StartBus(string busId)
        {
            if (messagingSystems.ContainsKey(busId))
            {
                messagingSystems[busId].bus.Start();
            }
        }

        public void StopBus(string busId)
        {
            if (messagingSystems.ContainsKey(busId))
            {
                messagingSystems[busId].bus.Stop();
            }
        }

        public void Stop()
        {
            using (var iterator = messagingSystems.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.bus.Stop();
                }
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
            //TODO(Brian): This is too hacky, most of the controllers won't be using this system. Refactor this in the future.
            prevTimeBudget -= messagingSystems[MessagingBusId.UI].Update(prevTimeBudget);

            prevTimeBudget -= messagingSystems[MessagingBusId.INIT].Update(prevTimeBudget);
            prevTimeBudget -= messagingSystems[MessagingBusId.SYSTEM].Update(prevTimeBudget);

            return prevTimeBudget;
        }

        public void ForceEnqueue(string busId, MessagingBus.QueuedSceneMessage queuedMessage)
        {
            messagingSystems[busId].Enqueue(queuedMessage);
        }

        public string Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            string busId = "";

            QueueMode queueMode = QueueMode.Reliable;

            // If current scene is the Global Scene, the bus id should be UI
            if (scene && scene.sceneData.id == SceneController.i.GlobalSceneId)
            {
                busId = MessagingBusId.UI;
            }
            else if (currentQueueState == MessagingController.QueueState.Init)
            {
                busId = MessagingBusId.INIT;
            }
            else
            {
                busId = MessagingBusId.SYSTEM;
            }

            // Check if the message type is an UpdateEntityComponent 
            if (queuedMessage.method == MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE)
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
            else if (queuedMessage.method == MessagingTypes.SCENE_STARTED)
            {
                // When a SCENE STARTED message is enqueued, the next messages should be 
                // enqueued in SYSTEM message bus, but we don't process them until 
                // scene started has been processed
                currentQueueState = MessagingController.QueueState.Systems;
            }

            messagingSystems[busId].Enqueue(queuedMessage, queueMode);

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
