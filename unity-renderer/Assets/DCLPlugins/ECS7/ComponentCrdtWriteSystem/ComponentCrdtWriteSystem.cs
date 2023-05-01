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
        public CrdtMessageType messageType;
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

    private readonly Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>> outgoingCrdt = new Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>>(60);
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
        ECSComponentWriteType writeType, CrdtMessageType messageType)
    {
        MessageData messageData = messagesPool.Count > 0 ? messagesPool.Dequeue() : new MessageData();

        messageData.messageType = messageType;
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

            CrdtMessage crdt;

            if (message.messageType == CrdtMessageType.APPEND_COMPONENT)
            {
                crdt = crdtExecutor.crdtProtocol.CreateSetMessage((int)message.entityId, message.componentId, message.data);
            }
            else
            {
                crdt = crdtExecutor.crdtProtocol.CreateLwwMessage((int)message.entityId, message.componentId, message.data);
            }

            if (message.minTimeStamp >= 0 && message.minTimeStamp > crdt.Timestamp)
            {
                crdt = new CrdtMessage(crdt.Type, crdt.EntityId, crdt.ComponentId, message.minTimeStamp, crdt.Data);
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
                crdtExecutor.ExecuteWithoutStoringState(crdt.EntityId, crdt.ComponentId, crdt.Data);
            }

            if (message.writeType.HasFlag(ECSComponentWriteType.SEND_TO_SCENE))
            {
                if (!outgoingCrdt.TryGetValue(message.sceneNumber, out DualKeyValueSet<int, long, CrdtMessage> sceneCrdtList))
                {
                    sceneCrdtList = new DualKeyValueSet<int, long, CrdtMessage>();
                    outgoingCrdt[message.sceneNumber] = sceneCrdtList;
                }

                if (!sceneCrdtList.TryGetValue(message.componentId, message.entityId, out CrdtMessage currentMsg))
                {
                    sceneCrdtList.Add(message.componentId, message.entityId, crdt);
                }
                else
                {
                    sceneCrdtList[message.componentId, message.entityId] = new CrdtMessage
                    (
                        currentMsg.Type,
                        currentMsg.EntityId,
                        currentMsg.ComponentId,
                        currentMsg.Timestamp,
                        message.data
                    );
                }

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
