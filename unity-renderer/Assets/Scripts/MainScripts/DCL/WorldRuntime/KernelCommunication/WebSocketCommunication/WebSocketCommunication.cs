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
public class WebSocketCommunication : IKernelCommunication
{
    private Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
    
    WebSocketServer ws;

    [System.NonSerialized]
    public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

    [System.NonSerialized]
    public static volatile bool queuedMessagesDirty;
    
    public bool isServerReady { get { return ws.IsListening; } }

    private bool CheckAvailableServerPort(int port) {
        bool isAvailable = true;

        // Evaluate current system tcp connections. This is the same information provided
        // by the netstat command line application, just in .Net strongly-typed object
        // form.  We will look through the list, and if our port we would like to use
        // in our TcpClient is occupied, we will set isAvailable to false.
        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

        foreach (IPEndPoint endpoint in tcpConnInfoArray) {
            if (endpoint.Port == port) {
                isAvailable = false;
                break;
            }
        }

        return isAvailable;
    }

    private int searchUnusedPort()
    {
        const int minPort = 5000;
        const int maxPort = 6000;

        for (int i = minPort; i < maxPort; ++i)
        {
            if (CheckAvailableServerPort(i))
            {
                return i;
            }
        }
        Debug.LogError("There are not unused ports.");
        return minPort; // error
    }
    public WebSocketCommunication()
    {
        DCL.DataStore.i.debugConfig.isWssDebugMode = true;

        int port = searchUnusedPort();
        
        string wssServerUrl = $"ws://localhost:{port}/";
        string wssServiceId = "dcl";
        
        DataStore.i.wsCommunication.wssServerUrl = wssServerUrl;
        DataStore.i.wsCommunication.wssServiceId = wssServiceId;
        DataStore.i.wsCommunication.communicationReady.OnChange += OnCommunicationReadyChangedValue;

        ws = new WebSocketServer(wssServerUrl);
        ws.AddWebSocketService<DCLWebSocketService>("/" + wssServiceId);
        ws.Start();
        
        DataStore.i.wsCommunication.communicationReady.Set(true);
    }

    private void OnCommunicationReadyChangedValue(bool newState, bool prevState)
    {
        
        //SendMessageToWeb("SystemInfoReport", "{}");
    }
    
    private string GetBridgeName(string messageType)
    {
        switch (messageType)
        {
            case "SetDebug":
            case "SetSceneDebugPanel":
            case "ShowFPSPanel":
            case "HideFPSPanel":
            case "SetEngineDebugPanel":
                return "Main";
            case "SendSceneMessage":
            case "LoadParcelScenes":
            case "UnloadScene":
            case "Reset":
            case "CreateGlobalScene":
            case "BuilderReady":
            case "UpdateParcelScenes":
                return "Main";
            case "Teleport":
                return "CharacterController";
            case "SetRotation":
                return "CameraController";
            case "LoadProfile":
            case "AddUserProfileToCatalog":
            case "AddUserProfilesToCatalog":
                case "RemoveUserProfilesFromCatalog":
                return "Main";
            case "ActivateRendering":
            case "DeactivateRendering":
                return "Main";
            case "ReportFocusOn":
            case "ReportFocusOff":
                return "Bridges";
            case "ForceActivateRendering":
                return "Main";
            case "ShowNotificationFromJson":
            case "ConfigureHUDElement":
            case "ShowTermsOfServices":
            case "RequestTeleport":
            case "ShowAvatarEditorInSignUp":
            case "SetUserTalking":
            case "SetUsersMuted":
            case "ShowWelcomeNotification":
            case "UpdateBalanceOfMANA":
            case "SetPlayerTalking":
            case "SetVoiceChatEnabledByScene":
                return "HUDController";
            
            case "GetMousePosition":
            case "SelectGizmo":
            case "ResetObject":
            case "ZoomDelta":
            case "SetPlayMode":
            case "TakeScreenshot":
            case "ResetBuilderScene":
            case "SetBuilderCameraPosition":
            case "SetBuilderCameraRotation":
            case "ResetBuilderCameraZoom":
            case "SetGridResolution":
            case "OnBuilderKeyDown":
            case "UnloadBuilderScene":
            case "SetSelectedEntities":
            case "GetCameraTargetBuilder":
            case "PreloadFile":
            case "SetBuilderConfiguration":
            case "TriggerSelfUserExpression":
            case "AirdroppingRequest":
                return "BuilderController";

            case "AddWearablesToCatalog":
                return "Main";
            case "WearablesRequestFailed":
            case "RemoveWearablesFromCatalog":
            case "ClearWearableCatalog":
            case "InitializeFriends":
            case "UpdateFriendshipStatus":
            case "UpdateUserPresence":
            case "FriendNotFound":
            case "AddMessageToChatWindow":
            case "UpdateMinimapSceneInformation":
            case "UpdateHotScenesList":
            case "SetRenderProfile":
                return "Main";

            case "SetTutorialEnabled":
            case "SetTutorialEnabledForUsersThatAlreadyDidTheTutorial":
                return "TutorialController";
            
            case "SetKernelConfiguration":
            case "UpdateRealmsInfo":
            case "ConnectionToRealmSuccess":
            case "ConnectionToRealmFailed":
            case "InitializeQuests":
            case "UpdateQuestProgress":
            case "SetENSOwnerQueryResult":
            case "UnpublishSceneResult":
            case "SetLoadingScreen":
                return "Bridges";
            case "CrashPayloadRequest":
            case "SetDisableAssetBundles":
            case "DumpRendererLockersInfo":
                return "Main";
            case "PublishSceneResult":
            case "BuilderProjectInfo":
            case "BuilderInWorldCatalogHeaders":
            case "AddAssets":
                return "Main"; // Kernel send `Main`, but this should be handled by BIW
            
            case "RunPerformanceMeterTool":
            case "InstantiateBotsAtWorldPos":
            case "InstantiateBotsAtCoords":
            case "RemoveBot":
            case "ClearBots":
                return "Main";
            default:
                Debug.Log(
                    "<b><color=#FF0000>WSSController:</color></b> received an unknown message from kernel to renderer: " +
                    messageType);
                return "";
        }
    }
    public override void Update()
    {
        lock (WebSocketCommunication.queuedMessages)
        {
            if (queuedMessagesDirty)
            {
                while (queuedMessages.Count > 0)
                {
                    DCLWebSocketService.Message msg = queuedMessages.Dequeue();


                    switch (msg.type)
                    {
                        // Add to this list the messages that are used a lot and you want better performance
                        case "SendSceneMessage":
                            DCL.Environment.i.world.sceneController.SendSceneMessage(msg.payload);
                            break;
                        default:
                            GameObject bridgeObject = null;
                            if (cache.TryGetValue(msg.type, out bridgeObject) == false)
                            {
                                bridgeObject = GameObject.Find(GetBridgeName(msg.type));
                                cache.Add(msg.type, bridgeObject);
                            }

                            if (bridgeObject != null)
                            {
                                bridgeObject.SendMessage(msg.type, msg.payload);
                            }
                            break;
                    }

                    if (DCLWebSocketService.VERBOSE)
                    {
                        Debug.Log(
                            "<b><color=#0000FF>WSSController</color></b> >>> Got it! passing message of type " +
                            msg.type);
                    }
                }
            }
        }
    }
}
