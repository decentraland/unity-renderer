using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DCL
{
    public class DCLWebSocketService : WebSocketBehavior
    {
        static bool VERBOSE = false;

        private void SendMessageToWeb(string type, string message)
        {
#if UNITY_EDITOR
            var x = new Message()
            {
                type = type,
                payload = message
            };
            Send(Newtonsoft.Json.JsonConvert.SerializeObject(x));
#endif
        }

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


                WSSController.queuedMessages.Enqueue(finalMessage);
                WSSController.queuedMessagesDirty = true;
            }

        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            WebInterface.OnMessageFromEngine -= SendMessageToWeb;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            WebInterface.OnMessageFromEngine += SendMessageToWeb;
            Send("{\"welcome\": true}");
        }
    }

    public class WSSController : MonoBehaviour
    {
        static bool VERBOSE = false;
        WebSocketServer ws;
        public SceneController sceneController;
        public DCLCharacterController characterController;
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
            ws = new WebSocketServer("ws://localhost:5000");
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

                        if ( VERBOSE ) Debug.Log("<b><color=#0000FF>WSSController</color></b> >>> Got it! passing message of type " + msg.type);

                        switch (msg.type)
                        {
                            case "SendSceneMessage":
                                sceneController.SendSceneMessage(msg.payload);
                                break;
                            case "LoadParcelScenes":
                                sceneController.LoadParcelScenes(msg.payload);
                                break;
                            case "SetPosition":
                                characterController.SetPosition(msg.payload);
                                break;
                            case "Reset":
                                sceneController.UnloadAllScenes();
                                break;
                            default:
                                Debug.Log("<b><color=#FF0000>WSSController</color></b> WHAT IS " + msg.type);
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
