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
        bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction);
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

        public Dictionary<string, MessagingBus> messagingBuses = new Dictionary<string, MessagingBus>();
        public IMessageHandler messageHandler;
        public string debugTag;

        private QueueState currentQueueState
        {
            get;
            set;
        }

        public MessagingController(IMessageHandler messageHandler, string debugTag = null)
        {
            this.debugTag = debugTag;
            this.messageHandler = messageHandler;

            //TODO(Brian): This is too hacky, most of the controllers won't be using this system. Refactor this in the future.
            AddMessageBus(MessagingBusId.UI, budgetMin: MessagingControllersManager.MSG_BUS_BUDGET_MIN, budgetMax: MessagingControllersManager.UI_MSG_BUS_BUDGET_MAX);
            AddMessageBus(MessagingBusId.INIT, budgetMin: MessagingControllersManager.MSG_BUS_BUDGET_MIN, budgetMax: MessagingControllersManager.INIT_MSG_BUS_BUDGET_MAX);
            AddMessageBus(MessagingBusId.SYSTEM, budgetMin: MessagingControllersManager.MSG_BUS_BUDGET_MIN, budgetMax: MessagingControllersManager.SYSTEM_MSG_BUS_BUDGET_MAX);

            currentQueueState = QueueState.Init;

            StartBus(MessagingBusId.INIT);
            StartBus(MessagingBusId.UI);
        }

        private MessagingBus AddMessageBus(string id, float budgetMin, float budgetMax)
        {
            var newMessagingBus = new MessagingBus(id, messageHandler, budgetMin, budgetMax);
            newMessagingBus.debugTag = debugTag;

            messagingBuses.Add(id, newMessagingBus);
            return newMessagingBus;
        }

        public void StartBus(string busId)
        {
            if (messagingBuses.ContainsKey(busId))
            {
                messagingBuses[busId].Start();
            }
        }

        public void StopBus(string busId)
        {
            if (messagingBuses.ContainsKey(busId))
            {
                messagingBuses[busId].Stop();
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

        public void ForceEnqueue(string busId, MessagingBus.QueuedSceneMessage queuedMessage)
        {
            messagingBuses[busId].Enqueue(queuedMessage);
        }

        public void Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage, out string busId)
        {
            busId = "";

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
            else if (queuedMessage.method == MessagingTypes.INIT_DONE)
            {
                // When a INIT DONE message is enqueued, the next messages should be 
                // enqueued in SYSTEM message bus, but we don't process them until 
                // scene started has been processed
                currentQueueState = MessagingController.QueueState.Systems;
            }

            messagingBuses[busId].Enqueue(queuedMessage, queueMode);
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
