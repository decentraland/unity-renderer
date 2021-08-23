using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using DCL;
using UnityEngine;
using WebSocketSharp.Server;
public class WebSocketCommunication : IKernelCommunication
{
    WebSocketServer ws;
    private Coroutine updateCoroutine;
    private bool requestStop = false;
    
    private Dictionary<string, GameObject> bridgeGameObjects = new Dictionary<string, GameObject>();
    
    public Dictionary<string, string> messageTypeToBridgeName = new Dictionary<string, string>(); // Public to be able to modify it from `explorer-desktop`

    [System.NonSerialized]
    public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

    [System.NonSerialized]
    public static volatile bool queuedMessagesDirty;
    
    public bool isServerReady { get { return ws.IsListening; } }

    private bool CheckAvailableServerPort(int port)
    {
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

    private int SearchUnusedPort()
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
        InitMessageTypeToBridgeName();
        
        DCL.DataStore.i.debugConfig.isWssDebugMode = true;

        int port = SearchUnusedPort();
        
        string wssServerUrl = $"ws://localhost:{port}/";
        string wssServiceId = "dcl";
        
        DataStore.i.wsCommunication.wssServerUrl = wssServerUrl;
        DataStore.i.wsCommunication.wssServiceId = wssServiceId;

        ws = new WebSocketServer(wssServerUrl);
        ws.AddWebSocketService<DCLWebSocketService>("/" + wssServiceId);
        ws.Start();
        
        DataStore.i.wsCommunication.communicationReady.Set(true);
        
        updateCoroutine = CoroutineStarter.Start(ProcessMessages());
    }
    private void InitMessageTypeToBridgeName()
    {
        messageTypeToBridgeName["SetDebug"] = "Main";
        messageTypeToBridgeName["SetSceneDebugPanel"] = "Main";
        messageTypeToBridgeName["ShowFPSPanel"] = "Main";
        messageTypeToBridgeName["HideFPSPanel"] = "Main";
        messageTypeToBridgeName["SetEngineDebugPanel"] = "Main";
        messageTypeToBridgeName["SendSceneMessage"] = "Main";
        messageTypeToBridgeName["LoadParcelScenes"] = "Main";
        messageTypeToBridgeName["UnloadScene"] = "Main";
        messageTypeToBridgeName["Reset"] = "Main";
        messageTypeToBridgeName["CreateGlobalScene"] = "Main";
        messageTypeToBridgeName["BuilderReady"] = "Main";
        messageTypeToBridgeName["UpdateParcelScenes"] = "Main";
        messageTypeToBridgeName["LoadProfile"] = "Main";
        messageTypeToBridgeName["AddUserProfileToCatalog"] = "Main";
        messageTypeToBridgeName["AddUserProfilesToCatalog"] = "Main";
        messageTypeToBridgeName["RemoveUserProfilesFromCatalog"] = "Main";
        messageTypeToBridgeName["ActivateRendering"] = "Main";
        messageTypeToBridgeName["DeactivateRendering"] = "Main";
        messageTypeToBridgeName["ForceActivateRendering"] = "Main";
        messageTypeToBridgeName["AddWearablesToCatalog"] = "Main";
        messageTypeToBridgeName["WearablesRequestFailed"] = "Main";
        messageTypeToBridgeName["RemoveWearablesFromCatalog"] = "Main";
        messageTypeToBridgeName["ClearWearableCatalog"] = "Main";
        messageTypeToBridgeName["InitializeFriends"] = "Main";
        messageTypeToBridgeName["UpdateFriendshipStatus"] = "Main";
        messageTypeToBridgeName["UpdateUserPresence"] = "Main";
        messageTypeToBridgeName["FriendNotFound"] = "Main";
        messageTypeToBridgeName["AddMessageToChatWindow"] = "Main";
        messageTypeToBridgeName["UpdateMinimapSceneInformation"] = "Main";
        messageTypeToBridgeName["UpdateHotScenesList"] = "Main";
        messageTypeToBridgeName["SetRenderProfile"] = "Main";

        messageTypeToBridgeName["Teleport"] = "CharacterController";

        messageTypeToBridgeName["SetRotation"] = "CameraController";

        messageTypeToBridgeName["ReportFocusOn"] = "Bridges";
        messageTypeToBridgeName["ReportFocusOff"] = "Bridges";
        messageTypeToBridgeName["SetKernelConfiguration"] = "Bridges";
        messageTypeToBridgeName["UpdateRealmsInfo"] = "Bridges";
        messageTypeToBridgeName["ConnectionToRealmSuccess"] = "Bridges";
        messageTypeToBridgeName["ConnectionToRealmFailed"] = "Bridges";
        messageTypeToBridgeName["InitializeQuests"] = "Bridges";
        messageTypeToBridgeName["UpdateQuestProgress"] = "Bridges";
        messageTypeToBridgeName["SetENSOwnerQueryResult"] = "Bridges";
        messageTypeToBridgeName["UnpublishSceneResult"] = "Bridges";
        messageTypeToBridgeName["SetLoadingScreen"] = "Bridges";

        messageTypeToBridgeName["ShowNotificationFromJson"] = "HUDController";
        messageTypeToBridgeName["ConfigureHUDElement"] = "HUDController";
        messageTypeToBridgeName["ShowTermsOfServices"] = "HUDController";
        messageTypeToBridgeName["RequestTeleport"] = "HUDController";
        messageTypeToBridgeName["ShowAvatarEditorInSignUp"] = "HUDController";
        messageTypeToBridgeName["SetUserTalking"] = "HUDController";
        messageTypeToBridgeName["SetUsersMuted"] = "HUDController";
        messageTypeToBridgeName["ShowWelcomeNotification"] = "HUDController";
        messageTypeToBridgeName["UpdateBalanceOfMANA"] = "HUDController";
        messageTypeToBridgeName["SetPlayerTalking"] = "HUDController";
        messageTypeToBridgeName["SetVoiceChatEnabledByScene"] = "HUDController";
        
        messageTypeToBridgeName["GetMousePosition"] = "BuilderController";
        messageTypeToBridgeName["SelectGizmo"] = "BuilderController";
        messageTypeToBridgeName["ResetObject"] = "BuilderController";
        messageTypeToBridgeName["ZoomDelta"] = "BuilderController";
        messageTypeToBridgeName["SetPlayMode"] = "BuilderController";
        messageTypeToBridgeName["TakeScreenshot"] = "BuilderController";
        messageTypeToBridgeName["ResetBuilderScene"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderCameraPosition"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderCameraRotation"] = "BuilderController";
        messageTypeToBridgeName["ResetBuilderCameraZoom"] = "BuilderController";
        messageTypeToBridgeName["SetGridResolution"] = "BuilderController";
        messageTypeToBridgeName["OnBuilderKeyDown"] = "BuilderController";
        messageTypeToBridgeName["UnloadBuilderScene"] = "BuilderController";
        messageTypeToBridgeName["SetSelectedEntities"] = "BuilderController";
        messageTypeToBridgeName["GetCameraTargetBuilder"] = "BuilderController";
        messageTypeToBridgeName["PreloadFile"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderConfiguration"] = "BuilderController";
        messageTypeToBridgeName["TriggerSelfUserExpression"] = "BuilderController";
        messageTypeToBridgeName["AirdroppingRequest"] = "BuilderController";

        messageTypeToBridgeName["SetTutorialEnabled"] = "TutorialController";
        messageTypeToBridgeName["SetTutorialEnabledForUsersThatAlreadyDidTheTutorial"] = "TutorialController";
        
        messageTypeToBridgeName["CrashPayloadRequest"] = "Main";
        messageTypeToBridgeName["SetDisableAssetBundles"] = "Main";
        messageTypeToBridgeName["DumpRendererLockersInfo"] = "Main";
        messageTypeToBridgeName["PublishSceneResult"] = "Main";
        messageTypeToBridgeName["BuilderProjectInfo"] = "Main";
        messageTypeToBridgeName["BuilderInWorldCatalogHeaders"] = "Main";
        messageTypeToBridgeName["AddAssets"] = "Main";
        messageTypeToBridgeName["RunPerformanceMeterTool"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtWorldPos"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtCoords"] = "Main";
        messageTypeToBridgeName["RemoveBot"] = "Main";
        messageTypeToBridgeName["ClearBots"] = "Main";
    }
    IEnumerator ProcessMessages()
    {
        while (!requestStop)
        {
            lock (queuedMessages)
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
                                if (messageTypeToBridgeName.TryGetValue(msg.type, out string bridgeName))
                                {
                                    if (bridgeGameObjects.TryGetValue(bridgeName, out GameObject bridgeObject) == false)
                                    {
                                        bridgeObject = GameObject.Find(bridgeName);
                                        bridgeGameObjects.Add(bridgeName, bridgeObject);
                                    }

                                    if (bridgeObject != null)
                                    {
                                        bridgeObject.SendMessage(msg.type, msg.payload);
                                    }
                                }
                                else
                                {
                                    Debug.Log(
                                        "<b><color=#FF0000>WebSocketCommunication:</color></b> received an unknown message from kernel to renderer: " +
                                        msg.type);
                                }
                                break;
                        }

                        if (DCLWebSocketService.VERBOSE)
                        {
                            Debug.Log(
                                "<b><color=#0000FF>WebSocketCommunication</color></b> >>> Got it! passing message of type " +
                                msg.type);
                        }
                    }
                }
            }
            yield return null;
        }
    }
}
