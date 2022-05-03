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

        private void Awake()
        {
            binaryMessageProcessor = new KernelBinaryMessageProcessor(Environment.i.world.sceneController);
        }

        public void CRDTMessage(string message)
        {
            Message msg = JsonUtility.FromJson<Message>(message);

            byte[] bytes = Convert.FromBase64String(msg.data);
            string sceneId = msg.sceneId;

            binaryMessageProcessor.Process(sceneId, bytes);
        }
    }
}