using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DCL.Interface;
using DCL.Models;
using UnityEngine.SceneManagement;

namespace DCL
{
    public enum QueueMode
    {
        Reliable,
        Lossy,
    }

    public class MessagingBus : IDisposable
    {
        public static bool VERBOSE = false;

        public IMessageProcessHandler handler;

        public LinkedList<QueuedSceneMessage> pendingMessages = new LinkedList<QueuedSceneMessage>();
        public bool hasPendingMessages => pendingMessagesCount > 0;

        //NOTE(Brian): This is handled manually. We aren't using pendingMessages.Count because is slow. Used heavily on critical ProcessMessages() loop.
        public int pendingMessagesCount;
        public long processedMessagesCount { get; set; }

        private static bool renderingIsDisabled => !CommonScriptableObjects.rendererState.Get();
        private float timeBudgetValue;

        public CustomYieldInstruction msgYieldInstruction;

        public MessagingBusType type;
        public string debugTag;

        public MessagingController owner;
        private IMessagingControllersManager manager;

        Dictionary<string, LinkedListNode<QueuedSceneMessage>> unreliableMessages = new Dictionary<string, LinkedListNode<QueuedSceneMessage>>();
        public int unreliableMessagesReplaced = 0;

        public bool enabled;

        public float timeBudget { get => renderingIsDisabled ? float.MaxValue : timeBudgetValue; set => timeBudgetValue = value; }

        public MessagingBus(MessagingBusType type, IMessageProcessHandler handler, MessagingController owner)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.handler = handler;
            this.enabled = false;
            this.type = type;
            this.owner = owner;
            this.pendingMessagesCount = 0;
            manager = owner.messagingManager;
        }

        public void Start() { enabled = true; }

        public void Stop()
        {
            enabled = false;

            if ( msgYieldInstruction is CleanableYieldInstruction cleanableYieldInstruction )
                cleanableYieldInstruction.Cleanup();

            pendingMessagesCount = 0;
        }

        public void Dispose() { Stop(); }

        public void Enqueue(QueuedSceneMessage message, QueueMode queueMode = QueueMode.Reliable)
        {
            bool enqueued = true;

            // When removing an entity we have to ensure that the enqueued lossy messages after it are processed and not replaced
            if (message is QueuedSceneMessage_Scene queuedSceneMessage && queuedSceneMessage.payload is Protocol.RemoveEntity removeEntityPayload)
            {
                List<string> unreliableMessagesToRemove = new List<string>();
                foreach (string key in unreliableMessages.Keys)
                {
                    if (key.Contains(removeEntityPayload.entityId)) //Key of unreliableMessages is a mixture of entityId
                    {
                        unreliableMessagesToRemove.Add(key);
                    }
                }

                for (int index = 0; index < unreliableMessagesToRemove.Count; index++)
                {
                    string key = unreliableMessagesToRemove[index];
                    if (unreliableMessages.ContainsKey(key))
                    {
                        unreliableMessages.Remove(key);
                    }
                }
            }

            if (queueMode == QueueMode.Reliable)
            {
                message.isUnreliable = false;
                AddReliableMessage(message);
            }
            else
            {
                message.isUnreliable = true;

                LinkedListNode<QueuedSceneMessage> node = null;

                message.unreliableMessageKey = message.tag;

                if (unreliableMessages.ContainsKey(message.unreliableMessageKey))
                {
                    node = unreliableMessages[message.unreliableMessageKey];

                    if (node.List != null)
                    {
                        node.Value = message;
                        enqueued = false;
                        unreliableMessagesReplaced++;
                    }
                }

                if (enqueued)
                {
                    node = AddReliableMessage(message);
                    unreliableMessages[message.unreliableMessageKey] = node;
                }
            }

            if (enqueued)
            {
                if (message.type == QueuedSceneMessage.Type.SCENE_MESSAGE)
                {
                    QueuedSceneMessage_Scene sm = message as QueuedSceneMessage_Scene;
                    ProfilingEvents.OnMessageWillQueue?.Invoke(sm.method);
                }

                if (type == MessagingBusType.INIT)
                {
                    manager.pendingInitMessagesCount++;
                }

                if (owner != null)
                {
                    owner.enabled = true;
                    manager.MarkBusesDirty();
                }
            }
        }

        public bool ProcessQueue(float timeBudget, out IEnumerator yieldReturn)
        {
            yieldReturn = null;

            // Note (Zak): This check is to avoid calling Time.realtimeSinceStartup
            // unnecessarily because it's pretty slow in JS
            if (timeBudget <= 0 || !enabled || pendingMessagesCount == 0)
                return false;

            float startTime = Time.realtimeSinceStartup;

            while (enabled && pendingMessagesCount > 0 && Time.realtimeSinceStartup - startTime < timeBudget)
            {
                QueuedSceneMessage m = pendingMessages.First.Value;

                RemoveFirstReliableMessage();

                if (m.isUnreliable)
                    RemoveUnreliableMessage(m);

                bool shouldLogMessage = VERBOSE;

                switch (m.type)
                {
                    case QueuedSceneMessage.Type.NONE:
                        break;
                    case QueuedSceneMessage.Type.SCENE_MESSAGE:

                        if (!(m is QueuedSceneMessage_Scene sceneMessage))
                            continue;

                        if (handler.ProcessMessage(sceneMessage, out msgYieldInstruction))
                        {
#if UNITY_EDITOR
                            if (DataStore.i.debugConfig.msgStepByStep)
                            {
                                if (VERBOSE)
                                {
                                    LogMessage(m, this, false);
                                    shouldLogMessage = false;
                                }

                                return true;
                            }
#endif
                        }
                        else
                        {
                            shouldLogMessage = false;
                        }

                        OnMessageProcessed();
                        ProfilingEvents.OnMessageWillDequeue?.Invoke(sceneMessage.method);

                        if (msgYieldInstruction != null)
                        {
                            processedMessagesCount++;

                            msgYieldInstruction = null;
                        }

                        break;
                    case QueuedSceneMessage.Type.LOAD_PARCEL:
                        handler.LoadParcelScenesExecute(m.message);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("LoadScene");
                        break;
                    case QueuedSceneMessage.Type.UNLOAD_PARCEL:
                        handler.UnloadParcelSceneExecute(m.message);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadScene");
                        break;
                    case QueuedSceneMessage.Type.UPDATE_PARCEL:
                        handler.UpdateParcelScenesExecute(m.message);
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UpdateScene");
                        break;
                    case QueuedSceneMessage.Type.UNLOAD_SCENES:
                        handler.UnloadAllScenes();
                        ProfilingEvents.OnMessageWillDequeue?.Invoke("UnloadAllScenes");
                        break;
                }

                OnMessageProcessed();
#if UNITY_EDITOR
                if (shouldLogMessage)
                {
                    LogMessage(m, this);
                }
#endif
            }

            return false;
        }

        public void OnMessageProcessed()
        {
            processedMessagesCount++;

            if (type == MessagingBusType.INIT)
            {
                manager.pendingInitMessagesCount--;
                manager.processedInitMessagesCount++;
            }
        }

        private LinkedListNode<QueuedSceneMessage> AddReliableMessage(QueuedSceneMessage message)
        {
            manager.pendingMessagesCount++;
            pendingMessagesCount++;
            return pendingMessages.AddLast(message);
        }

        private void RemoveFirstReliableMessage()
        {
            if (pendingMessages.First != null)
            {
                pendingMessages.RemoveFirst();
                pendingMessagesCount--;
                manager.pendingMessagesCount--;
            }
        }

        private void RemoveUnreliableMessage(QueuedSceneMessage message)
        {
            if (unreliableMessages.ContainsKey(message.unreliableMessageKey))
                unreliableMessages.Remove(message.unreliableMessageKey);
        }

        private void LogMessage(QueuedSceneMessage m, MessagingBus bus, bool logType = true)
        {
            string finalTag = WorldStateUtils.TryToGetSceneCoordsID(bus.debugTag);

            if (logType)
            {
                Debug.Log($"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.type}... processing msg... type = {m.type}... message = {m.message}");
            }
            else
            {
                Debug.Log($"#{bus.processedMessagesCount} ... Bus = {finalTag}, id = {bus.type}... processing msg... {m.message}");
            }
        }
    }
}