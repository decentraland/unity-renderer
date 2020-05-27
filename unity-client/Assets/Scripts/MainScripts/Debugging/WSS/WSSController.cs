using DCL.Components;
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

        public enum Environment
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
        public RenderingController renderingController;
        public DCLCharacterController characterController;
        private Builder.DCLBuilderBridge builderBridge = null;
        public CameraController cameraController;

        [System.NonSerialized]
        public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

        [System.NonSerialized] public static volatile bool queuedMessagesDirty;

        public bool isServerReady
        {
            get { return ws.IsListening; }
        }

        public bool openBrowserWhenStart;

        [Header("Kernel General Settings")] public bool useCustomContentServer = false;
        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)] public BaseUrl baseUrlMode;
        public string baseUrlCustom;

        [Space(10)] public Environment environment;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Header("Kernel Misc Settings")] public bool forceLocalComms = true;
        public bool allWearables = false;
        public bool testWearables = false;
        public bool enableTutorial = false;
        public bool useNewChat = true;
        public DebugPanel debugPanelMode = DebugPanel.Off;


        private void Awake()
        {
            i = this;
        }

        private void Start()
        {
#if UNITY_EDITOR
            SceneController.i.isWssDebugMode = true;

            ws = new WebSocketServer("ws://localhost:5000");
            ws.AddWebSocketService<DCLWebSocketService>("/dcl");
            ws.Start();

            if (useCustomContentServer)
            {
                RendereableAssetLoadHelper.useCustomContentServerUrl = true;
                RendereableAssetLoadHelper.customContentServerUrl = customContentServerUrl;
            }

            if (openBrowserWhenStart)
            {
                string baseUrl = "";
                string debugString = "";

                if (baseUrlMode == BaseUrl.CUSTOM)
                    baseUrl = baseUrlCustom;
                else
                    baseUrl = "http://localhost:8080/?";

                switch (environment)
                {
                    case Environment.USE_DEFAULT_FROM_URL:
                        break;
                    case Environment.LOCAL:
                        debugString = "DEBUG_MODE&";
                        break;
                    case Environment.ZONE:
                        debugString = "ENV=zone&";
                        break;
                    case Environment.TODAY:
                        debugString = "ENV=today&";
                        break;
                    case Environment.ORG:
                        debugString = "ENV=org&";
                        break;
                }

                if (forceLocalComms)
                {
                    debugString += "LOCAL_COMMS&";
                }

                if (allWearables)
                {
                    debugString += "ALL_WEARABLES&";
                }

                if (testWearables)
                {
                    debugString += "TEST_WEARABLES&";
                }

                if (enableTutorial)
                {
                    debugString += "RESET_TUTORIAL&";
                }

                if (useNewChat)
                {
                    debugString += "USE_NEW_CHAT&";
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

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (ws != null)
            {
                ws.Stop();
                ws = null;
            }
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
                            case "ShowFPSPanel":
                                sceneController.ShowFPSPanel();
                                break;
                            case "HideFPSPanel":
                                sceneController.HideFPSPanel();
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
                            case "SetRotation":
                                cameraController.SetRotation(msg.payload);
                                break;
                            case "Reset":
                                sceneController.UnloadAllScenesQueued();
                                break;
                            case "CreateUIScene":
                                sceneController.CreateUIScene(msg.payload);
                                break;
                            case "LoadProfile":
                                UserProfileController.i?.LoadProfile(msg.payload);
                                break;
                            case "AddUserProfileToCatalog":
                                UserProfileController.i.AddUserProfileToCatalog(msg.payload);
                                break;
                            case "AddUserProfilesToCatalog":
                                UserProfileController.i.AddUserProfilesToCatalog(msg.payload);
                                break;
                            case "RemoveUserProfilesFromCatalog":
                                UserProfileController.i.RemoveUserProfilesFromCatalog(msg.payload);
                                break;
                            case "DeactivateRendering":
                                renderingController.DeactivateRendering();
                                break;
                            case "ActivateRendering":
                                renderingController.ActivateRendering();
                                break;
                            case "ShowNotificationFromJson":
                                NotificationsController.i.ShowNotificationFromJson(msg.payload);
                                break;
                            case "BuilderReady":
                                sceneController.BuilderReady();
                                break;
                            case "UpdateParcelScenes":
                                sceneController.UpdateParcelScenes(msg.payload);
                                break;
                            case "GetMousePosition":
                                GetBuilderBridge()?.GetMousePosition(msg.payload);
                                break;
                            case "SelectGizmo":
                                GetBuilderBridge()?.SelectGizmo(msg.payload);
                                break;
                            case "ResetObject":
                                GetBuilderBridge()?.ResetObject();
                                break;
                            case "ZoomDelta":
                                GetBuilderBridge()?.ZoomDelta(msg.payload);
                                break;
                            case "SetPlayMode":
                                GetBuilderBridge()?.SetPlayMode(msg.payload);
                                break;
                            case "TakeScreenshot":
                                GetBuilderBridge()?.TakeScreenshot(msg.payload);
                                break;
                            case "ResetBuilderScene":
                                GetBuilderBridge()?.ResetBuilderScene();
                                break;
                            case "SetBuilderCameraPosition":
                                GetBuilderBridge()?.SetBuilderCameraPosition(msg.payload);
                                break;
                            case "SetBuilderCameraRotation":
                                GetBuilderBridge()?.SetBuilderCameraRotation(msg.payload);
                                break;
                            case "ResetBuilderCameraZoom":
                                GetBuilderBridge()?.ResetBuilderCameraZoom();
                                break;
                            case "SetGridResolution":
                                GetBuilderBridge()?.SetGridResolution(msg.payload);
                                break;
                            case "OnBuilderKeyDown":
                                GetBuilderBridge()?.OnBuilderKeyDown(msg.payload);
                                break;
                            case "UnloadBuilderScene":
                                GetBuilderBridge()?.UnloadBuilderScene(msg.payload);
                                break;
                            case "SetSelectedEntities":
                                GetBuilderBridge()?.SetSelectedEntities(msg.payload);
                                break;
                            case "GetCameraTargetBuilder":
                                GetBuilderBridge()?.GetCameraTargetBuilder(msg.payload);
                                break;
                            case "PreloadFile":
                                GetBuilderBridge()?.PreloadFile(msg.payload);
                                break;
                            case "AddWearableToCatalog":
                                CatalogController.i?.AddWearableToCatalog(msg.payload);
                                break;
                            case "AddWearablesToCatalog":
                                CatalogController.i?.AddWearablesToCatalog(msg.payload);
                                break;
                            case "RemoveWearablesFromCatalog":
                                CatalogController.i?.RemoveWearablesFromCatalog(msg.payload);
                                break;
                            case "ClearWearableCatalog":
                                CatalogController.i?.ClearWearableCatalog();
                                break;
                            case "ShowNewWearablesNotification":
                                HUDController.i?.ShowNewWearablesNotification(msg.payload);
                                break;
                            case "ConfigureHUDElement":
                                HUDController.i?.ConfigureHUDElement(msg.payload);
                                break;
                            case "InitializeFriends":
                                FriendsController.i?.InitializeFriends(msg.payload);
                                break;
                            case "UpdateFriendshipStatus":
                                FriendsController.i?.UpdateFriendshipStatus(msg.payload);
                                break;
                            case "UpdateUserPresence":
                                FriendsController.i?.UpdateUserPresence(msg.payload);
                                break;
                            case "FriendNotFound":
                                FriendsController.i?.FriendNotFound(msg.payload);
                                break;
                            case "AddMessageToChatWindow":
                                ChatController.i?.AddMessageToChatWindow(msg.payload);
                                break;
                            case "UpdateMinimapSceneInformation":
                                MinimapMetadataController.i?.UpdateMinimapSceneInformation(msg.payload);
                                break;
                            case "SetTutorialEnabled":
                                DCL.Tutorial.TutorialController.i?.SetTutorialEnabled();
                                break;
                            case "TriggerSelfUserExpression":
                                HUDController.i.TriggerSelfUserExpression(msg.payload);
                                break;
                            case "AirdroppingRequest":
                                HUDController.i.AirdroppingRequest(msg.payload);
                                break;
                            case "ShowWelcomeNotification":
                                NotificationsController.i.ShowWelcomeNotification();
                                break;
                            case "ShowTermsOfServices":
                                HUDController.i.ShowTermsOfServices(msg.payload);
                                break;
                            default:
                                Debug.Log(
                                    "<b><color=#FF0000>WSSController:</color></b> received an unknown message from kernel to renderer: " +
                                    msg.type);
                                break;
                        }
                    }

                    queuedMessagesDirty = false;
                }
            }
#endif
        }

        private Builder.DCLBuilderBridge GetBuilderBridge()
        {
            if (builderBridge == null)
            {
                builderBridge = FindObjectOfType<Builder.DCLBuilderBridge>();
            }

            return builderBridge;
        }
    }
}