using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
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
        public Queue<QueuedSceneMessage> pendingMessages = new Queue<QueuedSceneMessage>();
        public bool hasPendingMessages => pendingMessages != null && pendingMessages.Count > 0;
        public int pendingMessagesCount => pendingMessages != null ? pendingMessages.Count : 0;
        public long processedMessagesCount { get; private set; }

        public float timeBudget;

        public MessagingBus(IMessageHandler handler)
        {
            Assert.IsNotNull(handler, "IMessageHandler can't be null!");
            this.handler = handler;
            SceneController.i.StartCoroutine(ProcessMessages(pendingMessages));
        }

        IEnumerator ProcessMessages(Queue<QueuedSceneMessage> queue)
        {
            while (true)
            {
                float startTime = Time.realtimeSinceStartup;
                float prevDeltaTime = Time.deltaTime;
                float timeBudget = this.timeBudget;

                while (Time.realtimeSinceStartup - startTime < timeBudget && queue.Count > 0)
                {
                    QueuedSceneMessage m = queue.Peek();

                    switch (m.type)
                    {
                        case QueuedSceneMessage.Type.NONE:
                            break;
                        case QueuedSceneMessage.Type.SCENE_MESSAGE:
                            Coroutine routine = null;

                            var messageObject = m as QueuedSceneMessage_Scene;

#if UNITY_EDITOR
                            if (handler.ProcessMessage(messageObject.sceneId, messageObject.method, messageObject.payload, out routine))
                            {
                                if (SceneController.i.msgStepByStep)
                                {
                                    Debug.Log("message: " + m.message);
                                    Debug.Break();
                                    yield return null;
                                }
                            }
#else
                            handler.ProcessMessage(messageObject.sceneId, messageObject.method, messageObject.payload, out routine);
#endif

                            if (routine != null)
                            {
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
                    queue.Dequeue();
                }

                yield return null;
            }
        }
    }
}
