using DCL.Interface;
using System.Collections.Generic;
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

                finalMessage = JsonUtility.FromJson<Message>(e.Data);

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
        public enum DebugPanel
        {
            Off,
            Scene,
            Engine
        }

        private const string ENGINE_DEBUG_PANEL = "ENGINE_DEBUG_PANEL";
        private const string SCENE_DEBUG_PANEL = "SCENE_DEBUG_PANEL";
        public static WSSController i { get; private set; }
        static bool VERBOSE = false;
        WebSocketServer ws;
        public SceneController sceneController;
        public DCLCharacterController characterController;

        [System.NonSerialized]
        public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

        [System.NonSerialized]
        public static volatile bool queuedMessagesDirty;

        public bool isServerReady { get { return ws.IsListening; } }

        public bool openBrowserWhenStart;
        public bool useClientDebugMode = true;
        public Vector2 startInCoords = new Vector2(-99, 109);

        public DebugPanel debugPanelMode = DebugPanel.Off;


        private void Awake()
        {
            i = this;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            ws = new WebSocketServer("ws://localhost:5000");
            ws.AddWebSocketService<DCLWebSocketService>("/dcl");
            ws.Start();

            if (openBrowserWhenStart)
            {
                string debugString = "";

                if (useClientDebugMode)
                {
                    debugString = "DEBUG&";
                }
                else
                {
                    debugString = "DISABLE_AUTH&";
                }

                string debugPanelString = "";
                if(debugPanelMode == DebugPanel.Engine)
                {
                    debugPanelString = ENGINE_DEBUG_PANEL + "&";
                }
                else if (debugPanelMode == DebugPanel.Scene)
                {
                    debugPanelString = SCENE_DEBUG_PANEL + "&";
                }

                Application.OpenURL(
                    $"http://localhost:8080/tetra.html?{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl");
            }
#else
            useClientDebugMode = false;
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

                        if (VERBOSE)
                        {
                            Debug.Log(
                                "<b><color=#0000FF>WSSController</color></b> >>> Got it! passing message of type " +
                                msg.type);
                        }

                        switch (msg.type)
                        {
                            case "SetDebug":
                                sceneController.SetDebug();
                                break;
                            case "SetSceneDebugPanel":
                                sceneController.SetSceneDebugPanel();
                                break;
                            case "SetEngineDebugPanel":
                                sceneController.SetEngineDebugPanel();
                                break;
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
                                sceneController.UnloadAllScenesQueued();
                                break;
                            case "CreateUIScene":
                                sceneController.CreateUIScene(msg.payload);
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