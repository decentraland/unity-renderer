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

        public void Process(int sceneNumber, IntPtr messageIntPtr, int messageLength)
        {
            Process(sceneNumber, KernelBinaryMessageDeserializer.Deserialize(messageIntPtr, messageLength));
        }

        public void Process(int sceneNumber, byte[] message)
        {
            Process(sceneNumber, KernelBinaryMessageDeserializer.Deserialize(message));
        }

        private void Process(int sceneNumber, IEnumerator<object> deserializer)
        {
            using (var iterator = deserializer)
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current is CRDTMessage crdtMessage)
                    {
                        OnCRDTMessage(sceneNumber, crdtMessage);
                    }
                }
            }
        }

        private void OnCRDTMessage(int sceneNumber, CRDTMessage message)
        {
            var sceneMessagesPool = messageQueueHanlder.sceneMessagesPool;
            if (!sceneMessagesPool.TryDequeue(out QueuedSceneMessage_Scene queuedMessage))
            {
                queuedMessage = new QueuedSceneMessage_Scene();
            }
            queuedMessage.method = MessagingTypes.CRDT_MESSAGE;
            queuedMessage.type = QueuedSceneMessage.Type.SCENE_MESSAGE;
            queuedMessage.sceneNumber = sceneNumber;
            queuedMessage.payload = message;

            messageQueueHanlder.EnqueueSceneMessage(queuedMessage);
        }
    }
}