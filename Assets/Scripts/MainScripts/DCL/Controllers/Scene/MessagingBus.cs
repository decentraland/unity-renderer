using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{

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
        public long processedMessagesCount { get; private set; }
        public float lastTimeConsumed { get; private set; }

        public static bool renderingIsDisabled = false;
        private float timeBudgetValue;
        public float timeBudget
        {
            get => renderingIsDisabled ? float.MaxValue : timeBudgetValue;
            set => timeBudgetValue = value;
        }

        private Coroutine mainCoroutine;
        private CleanableYieldInstruction msgYieldInstruction;

        public MessagingSystem owner;

        public MessagingBus(IMessageHandler handler, MessagingSystem owner)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.owner = owner;
            this.handler = handler;
        }

        public bool isRunning { get { return mainCoroutine != null; } }

        public void Start()
        {
            if (mainCoroutine == null)
            {
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages(pendingMessages));
            }
        }

        public void Stop()
        {
            if (mainCoroutine != null)
            {
                SceneController.i.StopCoroutine(mainCoroutine);
                mainCoroutine = null;
            }

            if (msgYieldInstruction != null)
                msgYieldInstruction.Cleanup();
        }

        public void Dispose()
        {
            Stop();
        }

        IEnumerator ProcessMessages(LinkedList<QueuedSceneMessage> queue)
        {
            while (true)
            {
                float startTime = Time.realtimeSinceStartup;
                float timeBudget = this.timeBudget;

                if (VERBOSE && timeBudget == 0)
                {
                    string finalTag = SceneController.i.TryToGetSceneCoordsID(owner.debugTag);
                    Debug.Log($"#{processedMessagesCount} ... bus = {finalTag}, id = {owner.id}... timeBudget is zero!!!");
                }

                while (Time.realtimeSinceStartup - startTime < timeBudget && queue.Count > 0)
                {
                    QueuedSceneMessage m = queue.First.Value;

                    if (queue.First != null)
                        queue.RemoveFirst();

                    bool shouldLogMessage = VERBOSE;

                    switch (m.type)
                    {
                        case QueuedSceneMessage.Type.NONE:
                            break;
                        case QueuedSceneMessage.Type.SCENE_MESSAGE:

                            var messageObject = m as QueuedSceneMessage_Scene;

                            if (handler.ProcessMessage(messageObject.sceneId, messageObject.tag, messageObject.method, messageObject.payload, out msgYieldInstruction))
                            {
#if UNITY_EDITOR
                                if (SceneController.i.msgStepByStep)
                                {
                                    if (VERBOSE)
                                    {
                                        LogMessage(m, false);
                                        shouldLogMessage = false;
                                    }

                                    Debug.Break();

                                    yield return null;
                                }
#endif
                            }
                            else
                            {
                                shouldLogMessage = false;
                            }

                            if (msgYieldInstruction != null)
                            {
                                processedMessagesCount++;

                                lastTimeConsumed = Time.realtimeSinceStartup - startTime;
                                yield return msgYieldInstruction;

                                msgYieldInstruction = null;
                            }

                            SceneController.i.OnMessageWillDequeue?.Invoke(messageObject.method);
                            break;
                        case QueuedSceneMessage.Type.LOAD_PARCEL:
                            handler.LoadParcelScenesExecute(m.message);
                            SceneController.i.OnMessageWillDequeue?.Invoke("LoadScene");
                            break;
                        case QueuedSceneMessage.Type.UNLOAD_PARCEL:
                            handler.UnloadParcelSceneExecute(m.message);
                            SceneController.i.OnMessageWillDequeue?.Invoke("UnloadScene");
                            break;
                        case QueuedSceneMessage.Type.UPDATE_PARCEL:
                            handler.UpdateParcelScenesExecute(m.message);
                            SceneController.i.OnMessageWillDequeue?.Invoke("UpdateScene");
                            break;
                        case QueuedSceneMessage.Type.UNLOAD_SCENES:
                            handler.UnloadAllScenes();
                            SceneController.i.OnMessageWillDequeue?.Invoke("UnloadAllScenes");
                            break;
                    }

                    processedMessagesCount++;

#if UNITY_EDITOR
                    if (shouldLogMessage)
                    {
                        LogMessage(m);
                    }
#endif
                }

                lastTimeConsumed = Time.realtimeSinceStartup - startTime;
                yield return null;
            }
        }


        private void LogMessage(QueuedSceneMessage m, bool logType = true)
        {
            string finalTag = SceneController.i.TryToGetSceneCoordsID(owner.debugTag);

            if (logType)
            {
                Debug.Log($"#{processedMessagesCount} ... bus = {finalTag}, id = {owner.id}... processing msg... type = {m.type}... message = {m.message}");
            }
            else
            {
                Debug.Log($"#{processedMessagesCount} ... Bus = {finalTag}, id = {owner.id}... processing msg... {m.message}");
            }
        }
    }
}
