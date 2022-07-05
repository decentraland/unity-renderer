using System.IO;
using System.Text;
using DCL;
using DCL.Interface;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

public class DCLWebSocketService : WebSocketBehavior
{
    public static bool VERBOSE = false;
    
    public event Action OnCloseEvent;

    public event Action<string> OnErrorEvent;

    public event Action<byte[]> OnMessageEvent;

    public event Action OnConnectEvent;    

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
            var serializeObject = JsonConvert.SerializeObject(x);
            
            Send(serializeObject);
        
            if (VERBOSE)
            {
                Debug.Log("SendMessageToWeb: " + type);
            }
        }
#endif
    }

    public void SendBinary(byte[] data)
    {
        Send(data);
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
        
        // if (e.IsBinary)
        // {
        //     return;
        // }        

        if (e.IsBinary)
        {
            OnMessageEvent?.Invoke(e.RawData);
            return;
        }
        
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
        OnErrorEvent?.Invoke(e.Message);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);
        WebInterface.OnMessageFromEngine -= SendMessageToWeb;
        DataStore.i.wsCommunication.communicationEstablished.Set(false);
        OnCloseEvent?.Invoke();
    }

    protected override void OnOpen()
    {
        Debug.Log("WebSocket Communication Established");
        base.OnOpen();

        WebInterface.OnMessageFromEngine += SendMessageToWeb;
        DataStore.i.wsCommunication.communicationEstablished.Set(true);
        OnConnectEvent?.Invoke();
    }
}
