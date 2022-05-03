using System;
using UnityEngine;

namespace DCL.CRDT
{
    public class CRDTBridgeWS : MonoBehaviour
    {
        [Serializable]
        public class Message
        {
            public string sceneId;
            public string data;
        }

        public void CRDTMessage(string message)
        {
            Message msg = JsonUtility.FromJson<Message>(message);

            byte[] bytes = Convert.FromBase64String(msg.data);
            string sceneId = msg.sceneId;

            var queueHandler = Environment.i.world.sceneController;
            var sceneMessagesPool = queueHandler.sceneMessagesPool;

            using (var messageIterator = CRDTDeserializer.Deserialize(bytes))
            {
                while (messageIterator.MoveNext())
                {
                    if (!sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene queuedMessage))
                    {
                        queuedMessage = new QueuedSceneMessage_Scene();
                    }

                    queuedMessage.method = MessagingTypes.CRDT_MESSAGE;
                    queuedMessage.type = QueuedSceneMessage.Type.SCENE_MESSAGE;
                    queuedMessage.sceneId = sceneId;
                    queuedMessage.payload = messageIterator.Current;

                    queueHandler.EnqueueSceneMessage(queuedMessage);
                }
            }
        }
    }
}