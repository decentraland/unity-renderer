using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using DCL;
using UnityEngine;
using WebSocketSharp.Server;

public class WebSocketCommunication : IKernelCommunication
{
    public static DCLWebSocketService service;

    [System.NonSerialized]
    public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

    [System.NonSerialized]
    public static volatile bool queuedMessagesDirty;

    private Dictionary<string, GameObject> bridgeGameObjects = new Dictionary<string, GameObject>();

    public Dictionary<string, string> messageTypeToBridgeName = new Dictionary<string, string>(); // Public to be able to modify it from `explorer-desktop`
    private bool requestStop = false;
    private Coroutine updateCoroutine;

    WebSocketServer ws;

    public WebSocketCommunication(bool withSSL = false, int startPort = 7666, int endPort = 7800)
    {
        InitMessageTypeToBridgeName();

        DCL.DataStore.i.debugConfig.isWssDebugMode = true;

        string url = StartServer(startPort, endPort, withSSL);

        Debug.Log("WebSocket Server URL: " + url);

        DataStore.i.wsCommunication.url = url;

        DataStore.i.wsCommunication.communicationReady.Set(true);

        updateCoroutine = CoroutineStarter.Start(ProcessMessages());
    }

    public bool isServerReady => ws.IsListening;
    public void Dispose() { ws.Stop(); }
    public static event Action<DCLWebSocketService> OnWebSocketServiceAdded;

    private string StartServer(int port, int maxPort, bool withSSL)
    {
        if (port > maxPort)
        {
            throw new SocketException((int)SocketError.AddressAlreadyInUse);
        }
        string wssServerUrl;
        string wssServiceId = "dcl";
        try
        {
            if (withSSL)
            {
                wssServerUrl = $"wss://localhost:{port}/";
                ws = new WebSocketServer(wssServerUrl)
                {
                    SslConfiguration =
                    {
                        ServerCertificate = CertificateUtils.CreateSelfSignedCert(),
                        ClientCertificateRequired = false,
                        CheckCertificateRevocation = false,
                        ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                    },
                    KeepClean = false
                };
            }
            else
            {
                wssServerUrl = $"ws://localhost:{port}/";
                ws = new WebSocketServer(wssServerUrl);
            }

            ws.AddWebSocketService("/" + wssServiceId, () =>
            {
                service = new DCLWebSocketService();
                OnWebSocketServiceAdded?.Invoke(service);
                return service;
            });
            ws.Start();
        }
        catch (InvalidOperationException e)
        {
            ws.Stop();
            if (withSSL) // Search for available ports only if we're using SSL
            {
                SocketException se = (SocketException)e.InnerException;
                if (se is { SocketErrorCode: SocketError.AddressAlreadyInUse })
                {
                    return StartServer(port + 1, maxPort, withSSL);
                }
            }
            throw new InvalidOperationException(e.Message, e.InnerException);
        }

        string wssUrl = wssServerUrl + wssServiceId;
        return wssUrl;
    }

    private void InitMessageTypeToBridgeName()
    {
        // Please, use `Bridges` as a bridge name, avoid adding messages here. The system will use `Bridges` as the default bridge name.
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
        messageTypeToBridgeName["CrashPayloadRequest"] = "Main";
        messageTypeToBridgeName["SetDisableAssetBundles"] = "Main";
        messageTypeToBridgeName["DumpRendererLockersInfo"] = "Main";
        messageTypeToBridgeName["PublishSceneResult"] = "Main";
        messageTypeToBridgeName["BuilderProjectInfo"] = "Main";
        messageTypeToBridgeName["BuilderInWorldCatalogHeaders"] = "Main";
        messageTypeToBridgeName["RequestedHeaders"] = "Main";
        messageTypeToBridgeName["AddAssets"] = "Main";
        messageTypeToBridgeName["RunPerformanceMeterTool"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtWorldPos"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtCoords"] = "Main";
        messageTypeToBridgeName["StartBotsRandomizedMovement"] = "Main";
        messageTypeToBridgeName["StopBotsMovement"] = "Main";
        messageTypeToBridgeName["RemoveBot"] = "Main";
        messageTypeToBridgeName["ClearBots"] = "Main";
        messageTypeToBridgeName["ToggleSceneBoundingBoxes"] = "Main";
        messageTypeToBridgeName["TogglePreviewMenu"] = "Main";
        messageTypeToBridgeName["ToggleSceneSpawnPoints"] = "Main";
        messageTypeToBridgeName["AddFriendsWithDirectMessages"] = "Main";
        messageTypeToBridgeName["AddFriends"] = "Main";
        messageTypeToBridgeName["AddFriendRequests"] = "Main";
        messageTypeToBridgeName["UpdateTotalUnseenMessagesByUser"] = "Main";
        messageTypeToBridgeName["UpdateTotalFriendRequests"] = "Main";
        messageTypeToBridgeName["UpdateTotalFriends"] = "Main";
        messageTypeToBridgeName["InitializeChat"] = "Main";
        messageTypeToBridgeName["AddChatMessages"] = "Main";
        messageTypeToBridgeName["UpdateTotalUnseenMessages"] = "Main";
        messageTypeToBridgeName["UpdateUserUnseenMessages"] = "Main";
        messageTypeToBridgeName["UpdateHomeScene"] = "Main";

        messageTypeToBridgeName["Teleport"] = "CharacterController";

        messageTypeToBridgeName["SetRotation"] = "CameraController";

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
        messageTypeToBridgeName["TriggerSelfUserExpression"] = "HUDController";
        messageTypeToBridgeName["AirdroppingRequest"] = "HUDController";

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

        messageTypeToBridgeName["SetTutorialEnabled"] = "TutorialController";
        messageTypeToBridgeName["SetTutorialEnabledForUsersThatAlreadyDidTheTutorial"] = "TutorialController";

        messageTypeToBridgeName["VoiceChatStatus"] = "VoiceChatController";
    }

    IEnumerator ProcessMessages()
    {
        var hudControllerGO = GameObject.Find("HUDController");
        var mainGO = GameObject.Find("Main");

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
                            case "Reset":
                                DCL.Environment.i.world.sceneController.UnloadAllScenesQueued();
                                break;
                            case "SetVoiceChatEnabledByScene":
                                if (int.TryParse(msg.payload, out int value)) // The payload should be `string`, this will be changed in a `renderer-protocol` refactor
                                {
                                    hudControllerGO.SendMessage(msg.type, value);
                                }
                                break;
                            case "RunPerformanceMeterTool":
                                if (float.TryParse(msg.payload, out float durationInSeconds)) // The payload should be `string`, this will be changed in a `renderer-protocol` refactor
                                {
                                    mainGO.SendMessage(msg.type, durationInSeconds);
                                }
                                break;
                            default:
                                if (!messageTypeToBridgeName.TryGetValue(msg.type, out string bridgeName))
                                {
                                    bridgeName = "Bridges"; // Default bridge
                                }

                                if (bridgeGameObjects.TryGetValue(bridgeName, out GameObject bridgeObject) == false)
                                {
                                    bridgeObject = GameObject.Find(bridgeName);
                                    bridgeGameObjects.Add(bridgeName, bridgeObject);
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