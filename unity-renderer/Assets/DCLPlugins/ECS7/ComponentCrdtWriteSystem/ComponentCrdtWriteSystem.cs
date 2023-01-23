using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using RPC;
using System;
using System.Collections.Generic;

public class ComponentCrdtWriteSystem : IDisposable
{
    private class MessageData
    {
        public int sceneNumber;
        public long entityId;
        public int componentId;
        public byte[] data;
        public int minTimeStamp;
        public ECSComponentWriteType writeType;
    }

    private readonly RPCContext rpcContext;
    private readonly ISceneController sceneController;
    private readonly IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors;

    private readonly Dictionary<int, List<CRDTMessage>> outgoingCrdt = new Dictionary<int, List<CRDTMessage>>(60);
    private readonly Queue<MessageData> queuedMessages = new Queue<MessageData>(60);
    private readonly Queue<MessageData> messagesPool = new Queue<MessageData>(60);

    public ComponentCrdtWriteSystem(IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors, ISceneController sceneController, RPCContext rpcContext)
    {
        this.sceneController = sceneController;
        this.rpcContext = rpcContext;
        this.crdtExecutors = crdtExecutors;

        sceneController.OnSceneRemoved += OnSceneRemoved;
    }

    public void Dispose()
    {
        sceneController.OnSceneRemoved -= OnSceneRemoved;
    }

    public void WriteMessage(int sceneNumber, long entityId, int componentId, byte[] data, int minTimeStamp,
        ECSComponentWriteType writeType)
    {
        MessageData messageData = messagesPool.Count > 0 ? messagesPool.Dequeue() : new MessageData();

        messageData.sceneNumber = sceneNumber;
        messageData.entityId = entityId;
        messageData.componentId = componentId;
        messageData.data = data;
        messageData.writeType = writeType;
        messageData.minTimeStamp = minTimeStamp;

        queuedMessages.Enqueue(messageData);
    }

    public void LateUpdate()
    {
        int messagesCount = queuedMessages.Count;

        if (messagesCount == 0)
        {
            return;
        }

        for (int i = 0; i < messagesCount; i++)
        {
            var message = queuedMessages.Dequeue();
            messagesPool.Enqueue(message);

            if (!crdtExecutors.TryGetValue(message.sceneNumber, out ICRDTExecutor crdtExecutor))
                continue;

            CRDTMessage crdt = crdtExecutor.crdtProtocol.Create((int)message.entityId, message.componentId, message.data);

            if (message.minTimeStamp >= 0 && message.minTimeStamp > crdt.timestamp)
            {
                crdt.timestamp = message.minTimeStamp;
            }

            if (message.writeType.HasFlag(ECSComponentWriteType.SEND_TO_LOCAL))
            {
                crdtExecutor.Execute(crdt);
            }
            else if (message.writeType.HasFlag(ECSComponentWriteType.WRITE_STATE_LOCALLY))
            {
                crdtExecutor.crdtProtocol.ProcessMessage(crdt);
            }
            else if (message.writeType.HasFlag(ECSComponentWriteType.EXECUTE_LOCALLY))
            {
                crdtExecutor.ExecuteWithoutStoringState(crdt.entityId, crdt.componentId, crdt.data);
            }

            if (message.writeType.HasFlag(ECSComponentWriteType.SEND_TO_SCENE))
            {
                if (!outgoingCrdt.TryGetValue(message.sceneNumber, out List<CRDTMessage> sceneCrdtList))
                {
                    sceneCrdtList = new List<CRDTMessage>();
                    outgoingCrdt[message.sceneNumber] = sceneCrdtList;
                }

                sceneCrdtList.Add(crdt);

                if (!rpcContext.crdt.scenesOutgoingCrdts.ContainsKey(message.sceneNumber))
                {
                    rpcContext.crdt.scenesOutgoingCrdts.Add(message.sceneNumber, sceneCrdtList);
                }
            }
        }
    }

    private void OnSceneRemoved(IParcelScene scene)
    {
        outgoingCrdt.Remove(scene.sceneData.sceneNumber);
        rpcContext.crdt.scenesOutgoingCrdts.Remove(scene.sceneData.sceneNumber);
    }
}
