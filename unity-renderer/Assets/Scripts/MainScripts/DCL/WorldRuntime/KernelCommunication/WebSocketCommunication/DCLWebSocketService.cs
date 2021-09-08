using System;
using DCL.Interface;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using DCL;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class DCLWebSocketService : WebSocketBehavior
{
    public static bool VERBOSE = false;

    private void SendMessageToWeb(string type, string message)
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        var x = new Message()
        {
            type = type,
            payload = message
        };
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(x));
        if (VERBOSE)
        {
            Debug.Log("SendMessageToWeb: " + type);
        }
#endif
    }

    public class Message
    {
        public string type;
        public string payload;

        public override string ToString() { return string.Format("type = {0}... payload = {1}...", type, payload); }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        base.OnMessage(e);

        lock (WebSocketCommunication.queuedMessages)
        {
            Message finalMessage;
            finalMessage = JsonUtility.FromJson<Message>(e.Data);

            WebSocketCommunication.queuedMessages.Enqueue(finalMessage);
            WebSocketCommunication.queuedMessagesDirty = true;
        }
    }

    protected override void OnError(ErrorEventArgs e) { base.OnError(e); }

    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);
        WebInterface.OnMessageFromEngine -= SendMessageToWeb;
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        WebInterface.OnMessageFromEngine += SendMessageToWeb;
        
        DataStore.i.wsCommunication.communicationEstablished.Set(true);
        
        var callFromMainThread = new Action(WebInterface.SendSystemInfoReport); // `WebInterface.SendSystemInfoReport` can only be called from MainThread
        callFromMainThread.Invoke();
    }
}