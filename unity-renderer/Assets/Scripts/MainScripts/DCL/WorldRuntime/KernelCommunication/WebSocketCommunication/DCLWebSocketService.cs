using System;
using DCL.Interface;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
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

        if (ConnectionState == WebSocketState.Open)
        {
            Send(Newtonsoft.Json.JsonConvert.SerializeObject(x));
        
            if (VERBOSE)
            {
                Debug.Log("SendMessageToWeb: " + type);
            }
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
            Message finalMessage = JsonUtility.FromJson<Message>(e.Data);

            WebSocketCommunication.queuedMessages.Enqueue(finalMessage);
            WebSocketCommunication.queuedMessagesDirty = true;
        }
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Message);
        base.OnError(e);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);
        WebInterface.OnMessageFromEngine -= SendMessageToWeb;
        DataStore.i.wsCommunication.communicationEstablished.Set(false);
    }

    protected override void OnOpen()
    {
        Debug.Log("WebSocket Communication Established");
        base.OnOpen();

        WebInterface.OnMessageFromEngine += SendMessageToWeb;
        DataStore.i.wsCommunication.communicationEstablished.Set(true);
    }
}