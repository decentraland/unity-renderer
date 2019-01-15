using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DCL
{
  public class DCLWebSocketService : WebSocketBehavior
  {
    public class Message
    {
      public string type;
      public string payload;

      public override string ToString()
      {
        return string.Format("type = {0}... payload = {1}...", type, payload);
      }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      base.OnMessage(e);

      Debug.Log("<b><color=#0000FF>DCLWebSocketService</color></b> >>> Receiving message:" + e.Data);

      lock (WSSController.queuedMessages)
      {
        Message finalMessage;

        if (WSSController.debugMode)
        {
          finalMessage = new Message() { type = "LoadParcelScenes", payload = e.Data };
        }
        else
        {
          finalMessage = JsonUtility.FromJson<Message>(e.Data);
        }

        Send("OK!. Message received. " + finalMessage.ToString());

        WSSController.queuedMessages.Enqueue(finalMessage);
        WSSController.queuedMessagesDirty = true;
      }

    }

    protected override void OnError(ErrorEventArgs e)
    {
      base.OnError(e);
    }

    protected override void OnOpen()
    {
      base.OnOpen();
      Send("Mock service start!");
    }
  }

  public class WSSController : MonoBehaviour
  {
    WebSocketServer ws;
    public SceneController sceneController;
    public bool debugModeEnabled;

    public static bool debugMode;

    [System.NonSerialized]
    public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();
    [System.NonSerialized]
    public static volatile bool queuedMessagesDirty;

    public bool isServerReady { get { return ws.IsListening; } }

    private void OnEnable()
    {
#if UNITY_EDITOR
      WSSController.debugMode = debugModeEnabled;
      ws = new WebSocketServer("ws://localhost");
      ws.AddWebSocketService<DCLWebSocketService>("/dcl");

      ws.Start();
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
      ws.Stop();
      ws = null;
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
      lock (WSSController.queuedMessages)
      {
        if (queuedMessagesDirty)
        {
          while (queuedMessages.Count > 0)
          {
            DCLWebSocketService.Message msg = queuedMessages.Dequeue();
            Debug.Log("<b><color=#0000FF>WSSController</color></b> >>> Got it! passing message of type " + msg.type);
            switch (msg.type)
            {
              case "SendSceneMessage":
                sceneController.SendSceneMessage(msg.payload);
                break;
              case "LoadParcelScenes":
                sceneController.LoadParcelScenes(msg.payload);
                break;
              case "SetPosition":
                break;
            }
          }

          queuedMessagesDirty = false;
        }
      }
#endif
    }

  }
}
