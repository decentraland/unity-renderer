using Cysharp.Threading.Tasks;
using DCL.Providers;
using DCL.Quests;
using System.Threading;

public class QuestsPlugin : IPlugin
{
    private const string QUEST_TRACKER_HUD = "QuestTrackerHUD";
    private const string QUEST_COMPLETED_HUD = "QuestCompletedHUD";
    private const string QUEST_STARTED_POPUP = "QuestStartedPopupHUD";
    private readonly IAddressableResourceProvider resourceProvider;
    private IUserProfileBridge userProfileBridge = new UserProfileWebInterfaceBridge();

    private QuestTrackerComponentView questTrackerComponentView;
    private QuestCompletedComponentView questCompletedComponentView;
    private QuestStartedPopupComponentView questStartedPopupComponentView;

    private CancellationTokenSource cts;

    public QuestsPlugin(IAddressableResourceProvider resourceProvider)
    {
        this.resourceProvider = resourceProvider;
        cts = new CancellationTokenSource();

        InstantiateUIs(cts).Forget();
    }

    private async UniTaskVoid InstantiateUIs(CancellationTokenSource cts)
    {
        questTrackerComponentView = await resourceProvider.Instantiate<QuestTrackerComponentView>(QUEST_TRACKER_HUD, $"_{QUEST_TRACKER_HUD}", cts.Token);
        questCompletedComponentView = await resourceProvider.Instantiate<QuestCompletedComponentView>(QUEST_COMPLETED_HUD, $"_{QUEST_COMPLETED_HUD}", cts.Token);
        questStartedPopupComponentView = await resourceProvider.Instantiate<QuestStartedPopupComponentView>(QUEST_STARTED_POPUP, $"_{QUEST_STARTED_POPUP}", cts.Token);
        QuestsController controller = new QuestsController(null, userProfileBridge, questTrackerComponentView, questCompletedComponentView, questStartedPopupComponentView);
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
