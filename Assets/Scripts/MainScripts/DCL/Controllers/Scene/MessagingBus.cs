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

    public class MessagingBus
    {
        public class QueuedSceneMessage
        {
            public enum Type
            {
                NONE,
                SCENE_MESSAGE,
                LOAD_PARCEL,
                TELEPORT,
                UNLOAD_SCENES,
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

        public float timeBudget;
        public float budgetMax;

        public MessagingBus(IMessageHandler handler, float budgetMax)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.handler = handler;
            this.budgetMax = budgetMax;
            SceneController.i.StartCoroutine(ProcessMessages(pendingMessages));
        }

        IEnumerator ProcessMessages(LinkedList<QueuedSceneMessage> queue)
        {
            while (true)
            {
                float startTime = Time.realtimeSinceStartup;
                float prevDeltaTime = Time.deltaTime;
                float timeBudget = this.timeBudget;

                while (Time.realtimeSinceStartup - startTime < timeBudget && queue.Count > 0)
                {
                    QueuedSceneMessage m = queue.First.Value;

                    switch (m.type)
                    {
                        case QueuedSceneMessage.Type.NONE:
                            break;
                        case QueuedSceneMessage.Type.SCENE_MESSAGE:

                            var messageObject = m as QueuedSceneMessage_Scene;
                            Coroutine routine = null;

                            if (handler.ProcessMessage(messageObject.sceneId, messageObject.tag, messageObject.method, messageObject.payload, out routine))
                            {
#if UNITY_EDITOR
                                if (SceneController.i.msgStepByStep)
                                {
                                    Debug.Log("message: " + m.message);
                                    Debug.Break();
                                    yield return null;
                                }
#endif
                            }

                            if (routine != null)
                            {
                                processedMessagesCount++;

                                yield return routine;
                            }

                            SceneController.i.OnMessageWillDequeue?.Invoke(messageObject.method);
                            break;
                        case QueuedSceneMessage.Type.LOAD_PARCEL:
                            handler.LoadParcelScenesExecute(m.message);
                            SceneController.i.OnMessageWillDequeue?.Invoke("LoadScene");
                            break;
                        case QueuedSceneMessage.Type.UNLOAD_SCENES:
                            handler.UnloadAllScenes();
                            SceneController.i.OnMessageWillDequeue?.Invoke("UnloadScene");
                            break;
                    }

                    processedMessagesCount++;

                    if (queue.First != null)
                        queue.RemoveFirst();
                }

                yield return null;
            }
        }
    }
}
