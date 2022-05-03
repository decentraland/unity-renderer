using System;
using KernelCommunication;
using UnityEngine;

namespace DCL.CRDT
{
    public class BinaryMessageBridge : MonoBehaviour
    {
        [Serializable]
        public class Message
        {
            public string sceneId;
            public string data;
        }

        private KernelBinaryMessageProcessor binaryMessageProcessor;

        public void BinaryMessage(string message)
        {
            binaryMessageProcessor ??= new KernelBinaryMessageProcessor(Environment.i.world.sceneController);

            Message msg = JsonUtility.FromJson<Message>(message);

            byte[] bytes = Convert.FromBase64String(msg.data);
            string sceneId = msg.sceneId;

            binaryMessageProcessor.Process(sceneId, bytes);
        }
    }
}