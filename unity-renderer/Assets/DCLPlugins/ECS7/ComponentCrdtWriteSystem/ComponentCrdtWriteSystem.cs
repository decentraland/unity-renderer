using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using KernelCommunication;
using RPC;
using UnityEngine;
using BinaryWriter = KernelCommunication.BinaryWriter;

public class ComponentCrdtWriteSystem : IDisposable
{
    internal const int BINARY_MSG_MAX_SIZE = 5242880;

    internal readonly Dictionary<string, Queue<CRDTMessage>> queuedMessages = new Dictionary<string, Queue<CRDTMessage>>();

    private readonly IUpdateEventHandler updateEventHandler;
    private readonly MemoryStream memoryStream;
    private readonly BinaryWriter binaryWriter;
    private readonly IWorldState worldState;

    public ComponentCrdtWriteSystem(IUpdateEventHandler updateEventHandler, IWorldState worldState)
    {
        this.updateEventHandler = updateEventHandler;
        this.worldState = worldState;
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, ProcessMessages);
        memoryStream = new MemoryStream();
        binaryWriter = new BinaryWriter(memoryStream);
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, ProcessMessages);
        binaryWriter.Dispose();
        memoryStream.Dispose();
    }

    public void WriteMessage(string sceneId, long entityId, int componentId, byte[] data, ECSComponentWriteType writeType)
    {
        if (!worldState.loadedScenes.TryGetValue(sceneId, out IParcelScene scene))
        {
            return;
        }

        CRDTMessage message = scene.crdtExecutor.crdtProtocol.Create((int)entityId, componentId, data);

        // If SEND_TO_LOCAL we should execute the message,
        // otherwise we just store it in the crdt protocol state
        if (writeType.HasFlag(ECSComponentWriteType.SEND_TO_LOCAL))
        {
            scene.crdtExecutor.Execute(message);
        }
        else
        {
            scene.crdtExecutor.crdtProtocol.ProcessMessage(message);
        }

        // enqueue messages to send to kernel
        if (writeType.HasFlag(ECSComponentWriteType.SEND_TO_SCENE))
        {
            if (!queuedMessages.TryGetValue(sceneId, out Queue<CRDTMessage> sceneMessages))
            {
                sceneMessages = new Queue<CRDTMessage>();
                queuedMessages.Add(sceneId, sceneMessages);
            }

            sceneMessages.Enqueue(message);
        }
    }

    internal void ProcessMessages()
    {
        if (queuedMessages.Count == 0)
        {
            return;
        }

        Queue<CRDTMessage> sceneMessages;

        // we prioritize current scene's messages
        string currentSceneId = CommonScriptableObjects.sceneID.Get();
        if (!string.IsNullOrEmpty(currentSceneId) && queuedMessages.TryGetValue(currentSceneId, out sceneMessages))
        {
            if (DispatchSceneMessages(currentSceneId, sceneMessages))
            {
                queuedMessages.Remove(currentSceneId);
            }
            else
            {
                // if we couldn't dispatch all scene's messages we return
                // and continue dispatching on the next frame
                return;
            }
        }


        // dispatch to the other scenes
        var sceneIds = queuedMessages.Keys.ToArray();
        for (int i = 0; i < sceneIds.Length; i++)
        {
            string sceneId = sceneIds[i];
            queuedMessages.TryGetValue(sceneId, out sceneMessages);
            if (DispatchSceneMessages(sceneId, sceneMessages))
            {
                queuedMessages.Remove(sceneId);
            }
            else
            {
                // if we couldn't dispatch all scene's messages we return
                // and continue dispatching on the next frame
                return;
            }
        }
    }

    private bool DispatchSceneMessages(string sceneId, Queue<CRDTMessage> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            return true;
        }

        while (messages.Count > 0)
        {
            try
            {
                KernelBinaryMessageSerializer.Serialize(binaryWriter, messages.Dequeue());

                if (memoryStream.Length >= BINARY_MSG_MAX_SIZE && messages.Count > 0)
                {
                    DispatchBinaryMessage(sceneId, memoryStream);
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        DispatchBinaryMessage(sceneId, memoryStream);
        return true;
    }

    private void DispatchBinaryMessage(string sceneId, MemoryStream stream)
    {
        RPCGlobalContext.context.crdtContext.notifications.Enqueue((sceneId, stream.ToArray()));
        stream.SetLength(0);
    }
}