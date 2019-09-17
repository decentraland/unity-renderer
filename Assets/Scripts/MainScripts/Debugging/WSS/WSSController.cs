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

        public enum BaseUrl
        {
            LOCAL_HOST,
            CUSTOM,
        }

        public enum ContentSource
        {
            USE_DEFAULT_FROM_URL,
            LOCAL,
            ZONE,
            TODAY,
            ORG
        }

        private const string ENGINE_DEBUG_PANEL = "ENGINE_DEBUG_PANEL";
        private const string SCENE_DEBUG_PANEL = "SCENE_DEBUG_PANEL";
        public static WSSController i { get; private set; }
        static bool VERBOSE = false;
        WebSocketServer ws;
        public SceneController sceneController;
        public DCLCharacterController characterController;
        public HUDController hudController;

        [System.NonSerialized]
        public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

        [System.NonSerialized]
        public static volatile bool queuedMessagesDirty;

        public bool isServerReady { get { return ws.IsListening; } }

        public bool openBrowserWhenStart;

        [Header("Kernel General Settings")]
        public BaseUrl baseUrlMode;

        public string baseUrlCustom;

        [Space(10)]
        public ContentSource contentSource;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Header("Kernel Misc Settings")]
        public bool forceLocalComms = true;

        public DebugPanel debugPanelMode = DebugPanel.Off;


        private void Awake()
        {
            i = this;
        }

        private void Start()
        {
#if UNITY_EDITOR
            SceneController.i.isWssDebugMode = true;
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            ws = new WebSocketServer("ws://localhost:5000");
            ws.AddWebSocketService<DCLWebSocketService>("/dcl");
            ws.Start();

            if (openBrowserWhenStart)
            {
                string baseUrl = "";
                string debugString = "";

                if (baseUrlMode == BaseUrl.CUSTOM)
                    baseUrl = baseUrlCustom;
                else
                    baseUrl = "http://localhost:8080/?";

                switch (contentSource)
                {
                    case ContentSource.USE_DEFAULT_FROM_URL:
                        break;
                    case ContentSource.LOCAL:
                        debugString = "DEBUG_MODE&";
                        break;
                    case ContentSource.ZONE:
                        debugString = "ENV=zone&";
                        break;
                    case ContentSource.TODAY:
                        debugString = "ENV=today&";
                        break;
                    case ContentSource.ORG:
                        debugString = "ENV=org&";
                        break;
                }

                if (contentSource != ContentSource.LOCAL && forceLocalComms)
                {
                    debugString += "LOCAL_COMMS&";
                }

                string debugPanelString = "";

                if (debugPanelMode == DebugPanel.Engine)
                {
                    debugPanelString = ENGINE_DEBUG_PANEL + "&";
                }
                else if (debugPanelMode == DebugPanel.Scene)
                {
                    debugPanelString = SCENE_DEBUG_PANEL + "&";
                }

                Application.OpenURL(
                    $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl");
            }
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
                            case "UnloadScene":
                                sceneController.UnloadScene(msg.payload);
                                break;
                            case "Teleport":
                                characterController.Teleport(msg.payload);
                                break;
                            case "Reset":
                                sceneController.UnloadAllScenesQueued();
                                break;
                            case "CreateUIScene":
                                sceneController.CreateUIScene(msg.payload);
                                break;
                            case "LoadProfile":
                                UserProfileController.i.LoadProfile(msg.payload);
                                break;
                            case "DeactivateRendering":
                                RenderingController.i.DeactivateRendering();
                                break;
                            case "ActivateRendering":
                                RenderingController.i.ActivateRendering();
                                break;
                            case "ShowNotification":
                                hudController.ShowNotificationFromJson(msg.payload);
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
