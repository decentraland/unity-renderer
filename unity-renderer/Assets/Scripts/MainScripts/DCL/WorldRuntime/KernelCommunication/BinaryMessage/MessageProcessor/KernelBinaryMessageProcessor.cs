using System;
using System.Collections.Generic;
using DCL;
using DCL.CRDT;

namespace KernelCommunication
{
    public class KernelBinaryMessageProcessor
    {
        private readonly IMessageQueueHandler messageQueueHanlder;

        public KernelBinaryMessageProcessor(IMessageQueueHandler messageQueueHanlder)
        {
            this.messageQueueHanlder = messageQueueHanlder;
        }

        public void Process(string sceneId, IntPtr messageIntPtr, int messageLength)
        {
            Process(sceneId, KernelBinaryMessageDeserializer.Deserialize(messageIntPtr, messageLength));
        }

        public void Process(string sceneId, byte[] message)
        {
            Process(sceneId, KernelBinaryMessageDeserializer.Deserialize(message));
        }

        private void Process(string sceneId, IEnumerator<object> deserializer)
        {
            using (var iterator = deserializer)
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current is CRDTMessage crdtMessage)
                    {
                        OnCRDTMessage(sceneId, crdtMessage);
                    }
                }
            }
        }

        private void OnCRDTMessage(string sceneId, CRDTMessage message)
        {
            var sceneMessagesPool = messageQueueHanlder.sceneMessagesPool;
            if (!sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene queuedMessage))
            {
                queuedMessage = new QueuedSceneMessage_Scene();
            }
            queuedMessage.method = MessagingTypes.CRDT_MESSAGE;
            queuedMessage.type = QueuedSceneMessage.Type.SCENE_MESSAGE;
            queuedMessage.sceneId = sceneId;
            queuedMessage.payload = message;

            messageQueueHanlder.EnqueueSceneMessage(queuedMessage);
        }
    }
}