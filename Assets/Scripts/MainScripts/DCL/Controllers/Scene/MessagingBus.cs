using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class MessagingTypes
    {
        public const string ENTITY_COMPONENT_CREATE_OR_UPDATE = "UpdateEntityComponent";
        public const string ENTITY_CREATE = "CreateEntity";
        public const string ENTITY_REPARENT = "SetEntityParent";
        public const string ENTITY_COMPONENT_DESTROY = "ComponentRemoved";
        public const string SHARED_COMPONENT_ATTACH = "AttachEntityComponent";
        public const string SHARED_COMPONENT_CREATE = "ComponentCreated";
        public const string SHARED_COMPONENT_DISPOSE = "ComponentDisposed";
        public const string SHARED_COMPONENT_UPDATE = "ComponentUpdated";
        public const string ENTITY_DESTROY = "RemoveEntity";
        public const string SCENE_LOAD = "LoadScene";
        public const string SCENE_UPDATE = "UpdateScene";
        public const string SCENE_DESTROY = "UnloadScene";
        public const string INIT_DONE = "InitMessagesFinished";
        public const string QUERY = "Query";
    }

    public class MessagingBusId
    {
        public const string UI = "UI";
        public const string INIT = "INIT";
        public const string SYSTEM = "SYSTEM";
    }


    public enum QueueMode
    {
        Reliable,
        Lossy,
    }

    public struct PendingMessage
    {
        public MessagingBus.QueuedSceneMessage_Scene message;
        public string busId;
        public QueueMode queueMode;

        public PendingMessage(string busId, MessagingBus.QueuedSceneMessage_Scene message, QueueMode queueMode)
        {
            this.busId = busId;
            this.message = message;
            this.queueMode = queueMode;
        }
    }

    public class MessagingBus : IDisposable
    {
        public static bool VERBOSE = false;

        public class QueuedSceneMessage
        {
            public enum Type
            {
                NONE,
                SCENE_MESSAGE,
                LOAD_PARCEL,
                UPDATE_PARCEL,
                TELEPORT,
                UNLOAD_SCENES,
                UNLOAD_PARCEL,
                SCENE_STARTED
            }

            public string tag;
            public Type type;
            public string sceneId;
            public string message;
        }
        public class QueuedSceneMessage_Scene : QueuedSceneMessage
        {
            public string method;
            public string payload;
        }

        public IMessageHandler handler;

        public LinkedList<QueuedSceneMessage> pendingMessages = new LinkedList<QueuedSceneMessage>();
        public bool hasPendingMessages => pendingMessages != null && pendingMessages.Count > 0;
        public int pendingMessagesCount => pendingMessages != null ? pendingMessages.Count : 0;
        public long processedMessagesCount { get; set; }

        public static bool renderingIsDisabled = false;
        private float timeBudgetValue;

        public CleanableYieldInstruction msgYieldInstruction;

        public string id;
        public string debugTag;

        public float budgetMin = MessagingController.MSG_BUS_BUDGET_MIN;
        public float budgetMax = MessagingController.INIT_MSG_BUS_BUDGET_MAX;

        Dictionary<string, LinkedListNode<MessagingBus.QueuedSceneMessage>> unreliableMessages = new Dictionary<string, LinkedListNode<MessagingBus.QueuedSceneMessage>>();
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        public int unreliableMessagesReplaced = 0;

        public bool isRunning
        {
            get; private set;
        }

        public float timeBudget
        {
            get => renderingIsDisabled ? float.MaxValue : timeBudgetValue;
            set => timeBudgetValue = value;
        }

        public MessagingBus(string id, IMessageHandler handler, float budgetMin, float budgetMax)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.handler = handler;
            this.isRunning = false;
            this.id = id;
            this.budgetMin = budgetMin;
            this.budgetMax = budgetMax;
        }

        public void Start()
        {
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;

            if (msgYieldInstruction != null)
                msgYieldInstruction.Cleanup();
        }
        public void Dispose()
        {
            Stop();
        }

        public float UpdateTimeBudget(float timeBudget, float prevTimeBudget)
        {
            if (!isRunning)
                return prevTimeBudget;

            timeBudget = Mathf.Clamp(timeBudget, budgetMin, budgetMax);

            if (VERBOSE)
                Debug.Log($"tag = {debugTag} id = {id} timeBudget == {timeBudget}");

            return timeBudget;
        }

        public void Enqueue(MessagingBus.QueuedSceneMessage message, QueueMode queueMode = QueueMode.Reliable)
        {
            bool enqueued = true;

            if (queueMode == QueueMode.Reliable)
            {
                pendingMessages.AddLast(message);
            }
            else
            {
                stringBuilder.Clear();

                LinkedListNode<MessagingBus.QueuedSceneMessage> node = null;

                stringBuilder.Append(message.tag);
                stringBuilder.Append(message.sceneId);

                string tag = stringBuilder.ToString();

                if (unreliableMessages.ContainsKey(tag))
                {
                    node = unreliableMessages[tag];

                    if (node.List != null)
                    {
                        node.Value = message;
                        enqueued = false;
                        unreliableMessagesReplaced++;
                    }
                }

                if (enqueued)
                {
                    node = pendingMessages.AddLast(message);
                    unreliableMessages[tag] = node;
                }
            }

            if (enqueued)
            {
                if (message.type == MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE)
                {
                    MessagingBus.QueuedSceneMessage_Scene sm = message as MessagingBus.QueuedSceneMessage_Scene;
                    SceneController.i.OnMessageWillQueue?.Invoke(sm.method);
                }
            }
        }

        public bool ProcessQueue(float timeBudget, out IEnumerator yieldReturn)
        {
            LinkedList<MessagingBus.QueuedSceneMessage> queue = pendingMessages;
            yieldReturn = null;

            // Note (Zak): This check is to avoid calling DCLDCLTime.realtimeSinceStartup
            // unnecessarily because it's pretty slow in JS
            if (timeBudget == 0 || !isRunning || queue.Count == 0)
                return false;

            float startTime = DCLTime.realtimeSinceStartup;

            while (timeBudget != 0 && isRunning && queue.Count > 0 && DCLTime.realtimeSinceStartup - startTime < timeBudget)
            {
                MessagingBus.QueuedSceneMessage m = queue.First.Value;

                if (queue.First != null)
                    queue.RemoveFirst();

                bool shouldLogMessage = VERBOSE;

                switch (m.type)
                {
                    case MessagingBus.QueuedSceneMessage.Type.NONE:
                        break;
                    case MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE:

                        var messageObject = m as MessagingBus.QueuedSceneMessage_Scene;

                        if (handler.ProcessMessage(messageObject.sceneId, messageObject.tag, messageObject.method, messageObject.payload, out msgYieldInstruction))
                        {
#if UNITY_EDITOR
                            if (SceneController.i.msgStepByStep)
                            {
                                if (VERBOSE)
                                {
                                    LogMessage(m, this, false);
                                    shouldLogMessage = false;
                                }

                                Debug.Break();
                                return true;
                            }
#endif
                        }
                        else
                        {
                            shouldLogMessage = false;
                        }

                        SceneController.i.OnMessageWillDequeue?.Invoke(messageObject.method);

                        if (msgYieldInstruction != null)
                        {
                            processedMessagesCount++;

                            yieldReturn = msgYieldInstruction;

                            msgYieldInstruction = null;

                            return true;
                        }

                        break;
                    case MessagingBus.QueuedSceneMessage.Type.LOAD_PARCEL:
                        handler.LoadParcelScenesExecute(m.message);
                        SceneController.i.OnMessageWillDequeue?.Invoke("LoadScene");
                        break;
                    case MessagingBus.QueuedSceneMessage.Type.UNLOAD_PARCEL:
                        handler.UnloadParcelSceneExecute(m.message);
                        SceneController.i.OnMessageWillDequeue?.Invoke("UnloadScene");
                        break;
                    case MessagingBus.QueuedSceneMessage.Type.UPDATE_PARCEL:
                        handler.UpdateParcelScenesExecute(m.message);
                        SceneController.i.OnMessageWillDequeue?.Invoke("UpdateScene");
                        break;
                    case MessagingBus.QueuedSceneMessage.Type.UNLOAD_SCENES:
                        handler.UnloadAllScenes();
                        SceneController.i.OnMessageWillDequeue?.Invoke("UnloadAllScenes");
                        break;
                }

                processedMessagesCount++;

#if UNITY_EDITOR
                if (shouldLogMessage)
                {
                    LogMessage(m, this);
                }
#endif
            }

            return false;
        }

        private void LogMessage(MessagingBus.QueuedSceneMessage m, MessagingBus bus, bool logType = true)
        {
            string finalTag = SceneController.i.TryToGetSceneCoordsID(bus.debugTag);

            if (logType)
            {
                Debug.Log($"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.id}... processing msg... type = {m.type}... message = {m.message}");
            }
            else
            {
                Debug.Log($"#{bus.processedMessagesCount} ... Bus = {finalTag}, id = {bus.id}... processing msg... {m.message}");
            }
        }

    }
}
