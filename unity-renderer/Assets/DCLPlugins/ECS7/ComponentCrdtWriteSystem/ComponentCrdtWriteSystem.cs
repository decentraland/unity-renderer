using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using RPC;

public class ComponentCrdtWriteSystem : IDisposable
{
    private class MessageData
    {
        public string sceneId;
        public long entityId;
        public int componentId;
        public byte[] data;
        public long minTimeStamp;
        public ECSComponentWriteType writeType;
    }

    private readonly RPCContext rpcContext;
    private readonly ISceneController sceneController;
    private readonly IWorldState worldState;

    private readonly Dictionary<string, CRDTProtocol> outgoingCrdt = new Dictionary<string, CRDTProtocol>(60);
    private readonly Queue<MessageData> queuedMessages = new Queue<MessageData>(60);
    private readonly Queue<MessageData> messagesPool = new Queue<MessageData>(60);

    public ComponentCrdtWriteSystem(IWorldState worldState, ISceneController sceneController, RPCContext rpcContext)
    {
        this.sceneController = sceneController;
        this.rpcContext = rpcContext;
        this.worldState = worldState;

        sceneController.OnSceneRemoved += OnSceneRemoved;
    }

    public void Dispose()
    {
        sceneController.OnSceneRemoved -= OnSceneRemoved;
    }

    public void WriteMessage(string sceneId, long entityId, int componentId, byte[] data, long minTimeStamp, ECSComponentWriteType writeType)
    {
        MessageData messageData = messagesPool.Count > 0 ? messagesPool.Dequeue() : new MessageData();

        messageData.sceneId = sceneId;
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

            if (!worldState.TryGetScene(message.sceneId, out IParcelScene scene))
                continue;

            CRDTMessage crdt = scene.crdtExecutor.crdtProtocol.Create((int)message.entityId, message.componentId, message.data);
            if (message.minTimeStamp >= 0 && message.minTimeStamp > crdt.timestamp)
            {
                crdt.timestamp = message.minTimeStamp;
            }

            if (message.writeType.HasFlag(ECSComponentWriteType.SEND_TO_LOCAL))
            {
                scene.crdtExecutor.Execute(crdt);
            }
            else if (message.writeType.HasFlag(ECSComponentWriteType.WRITE_STATE_LOCALLY))
            {
                scene.crdtExecutor.crdtProtocol.ProcessMessage(crdt);
            }
            else if (message.writeType.HasFlag(ECSComponentWriteType.EXECUTE_LOCALLY))
            {
                scene.crdtExecutor.ExecuteWithoutStoringState(crdt.key1, crdt.key2, crdt.data);
            }

            if (message.writeType.HasFlag(ECSComponentWriteType.SEND_TO_SCENE))
            {
                if (!outgoingCrdt.TryGetValue(message.sceneId, out CRDTProtocol sceneCrdtState))
                {
                    sceneCrdtState = new CRDTProtocol();
                    outgoingCrdt[message.sceneId] = sceneCrdtState;
                }

                sceneCrdtState.ProcessMessage(crdt);

                if (!rpcContext.crdtContext.scenesOutgoingCrdts.ContainsKey(message.sceneId))
                {
                    rpcContext.crdtContext.scenesOutgoingCrdts.Add(message.sceneId, sceneCrdtState);
                }
            }
        }
    }

    private void OnSceneRemoved(IParcelScene scene)
    {
        string sceneId = scene.sceneData.id;
        outgoingCrdt.Remove(sceneId);
        rpcContext.crdtContext.scenesOutgoingCrdts.Remove(sceneId);
    }
}