using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Providers;
using DCL.Quests;
using DCLServices.QuestsService;
using Decentraland.Quests;
using rpc_csharp;
using RPC.Transports;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class QuestsPlugin : IPlugin
{
    private const string QUEST_TRACKER_HUD = "QuestTrackerHUD";
    private const string QUEST_COMPLETED_HUD = "QuestCompletedHUD";
    private const string QUEST_STARTED_POPUP = "QuestStartedPopupHUD";
    private const string QUEST_LOG = "QuestLogHUD";
    private readonly IAddressableResourceProvider resourceProvider;
    private IUserProfileBridge userProfileBridge = new UserProfileWebInterfaceBridge();

    private QuestTrackerComponentView questTrackerComponentView;
    private QuestCompletedComponentView questCompletedComponentView;
    private QuestStartedPopupComponentView questStartedPopupComponentView;
    private QuestLogComponentView questLogComponentView;
    private QuestsService questService;

    private CancellationTokenSource cts;

    public QuestsPlugin(IAddressableResourceProvider resourceProvider)
    {
        this.resourceProvider = resourceProvider;
        cts = new CancellationTokenSource();

        InstantiateUIs(cts).Forget();
    }

    private async UniTaskVoid InstantiateUIs(CancellationTokenSource cts)
    {
        questService = new QuestsService(await GetClientQuestsService());
        await UniTask.SwitchToMainThread(cts.Token);

        questTrackerComponentView = await resourceProvider.Instantiate<QuestTrackerComponentView>(QUEST_TRACKER_HUD, $"_{QUEST_TRACKER_HUD}", cts.Token);
        questCompletedComponentView = await resourceProvider.Instantiate<QuestCompletedComponentView>(QUEST_COMPLETED_HUD, $"_{QUEST_COMPLETED_HUD}", cts.Token);
        questStartedPopupComponentView = await resourceProvider.Instantiate<QuestStartedPopupComponentView>(QUEST_STARTED_POPUP, $"_{QUEST_STARTED_POPUP}", cts.Token);
        questLogComponentView = await resourceProvider.Instantiate<QuestLogComponentView>(QUEST_LOG, $"_{QUEST_LOG}", cts.Token);

        questLogComponentView.gameObject.SetActive(false);
        DataStore.i.Quests.isInitialized.Set(true);

        QuestsController controller = new QuestsController(
            questService,
            questTrackerComponentView,
            questCompletedComponentView,
            questStartedPopupComponentView,
            questLogComponentView,
            userProfileBridge,
            new DefaultPlayerPrefs(),
            DataStore.i,
            Environment.i.world.teleportController);
    }

    public async UniTask<ClientQuestsService> GetClientQuestsService()
    {
        var rpc = Environment.i.serviceLocator.Get<IRPC>();
        await rpc.EnsureRpc();
        var rpcSignRequest = new RPCSignRequest(rpc);

        AuthedWebSocketClientTransport webSocketClientTransport = new AuthedWebSocketClientTransport(rpcSignRequest, "wss://quests-rpc.decentraland.zone");

        //TODO Quest Server is not accepting the correct url and by now it needs "/". Change it as soon as QuestServer is ready to have a generic authed WebSocket Client
        await webSocketClientTransport.Connect("/");

        RpcClient rpcClient = new RpcClient(webSocketClientTransport);
        var clientPort = await rpcClient.CreatePort("Unity_QuestsService");
        var module = await clientPort.LoadModule(QuestsServiceCodeGen.ServiceName);
        return new ClientQuestsService(module);
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
