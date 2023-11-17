using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Map;
using DCL.Tasks;
using RPC.Context;
using System.Threading;

public class TeleportPromptHUDController : IHUD
{
    private const string TELEPORT_COMMAND_MAGIC = "magic";
    private const string TELEPORT_COMMAND_CROWD = "crowd";

    private const string EVENT_STRING_LIVE = "LIVE";
    private const string EVENT_STRING_TODAY = "Today @ {0:HH:mm}";
    private const int INITIAL_ANIMATION_DELAY = 500;

    internal TeleportPromptHUDView view { get; private set; }

    private readonly RestrictedActionsContext restrictedActionsServiceContext;
    private readonly ITeleportController teleportController;
    private readonly DataStore dataStore;
    private readonly IMinimapApiBridge minimapApiBridge;
    private bool isVisible;
    private TeleportData teleportData;
    private CancellationTokenSource cancellationToken = new ();
    private EventData currentEvent;

    public TeleportPromptHUDController(DataStore dataStore,
        IMinimapApiBridge minimapApiBridge,
        RestrictedActionsContext restrictedActionsContext,
        ITeleportController teleportController)
    {
        this.dataStore = dataStore;
        this.minimapApiBridge = minimapApiBridge;

        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("TeleportPromptHUD")).GetComponent<TeleportPromptHUDView>();
        view.name = "_TeleportPromptHUD";
        view.OnCloseEvent += ClosePanel;
        view.OnTeleportEvent += OnTeleportPressed;

        dataStore.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
        dataStore.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
        dataStore.world.requestTeleportData.OnChange += ReceivedRequestTeleportData;

        restrictedActionsServiceContext = restrictedActionsContext;
        this.teleportController = teleportController;
        restrictedActionsServiceContext.TeleportToPrompt += RequestCoordinatesTeleport;
    }

    public void Dispose()
    {
        view.OnCloseEvent -= CloseView;
        view.OnTeleportEvent -= OnTeleportPressed;

        if (view)
        {
            view.OnCloseEvent -= ClosePanel;
            view.OnTeleportEvent -= OnTeleportPressed;
            UnityEngine.Object.Destroy(view.gameObject);
        }

        dataStore.HUDs.gotoPanelVisible.OnChange -= ChangeVisibility;
        dataStore.HUDs.gotoPanelCoordinates.OnChange -= SetCoordinates;
        restrictedActionsServiceContext.TeleportToPrompt -= RequestCoordinatesTeleport;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            view.SetInAnimation();
            AudioScriptableObjects.fadeIn.Play(true);
        }
        else
        {
            AudioScriptableObjects.dialogClose.Play(true);
            currentEvent = null;
            teleportData = null;
            view.SetOutAnimation();
            view.Reset();
            AudioScriptableObjects.dialogClose.Play(true);
        }
        isVisible = visible;
    }

    public void RequestJSONTeleport(string teleportDataJson)
    {
        Utils.UnlockCursor();

        teleportData = Utils.SafeFromJson<TeleportData>(teleportDataJson);
        currentEvent = teleportData.sceneEvent;
        SetVisibility(true);

        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_MAGIC:
                view.ShowTeleportToMagic();
                break;
            case TELEPORT_COMMAND_CROWD:
                view.ShowTeleportToCrowd();
                break;
            default:
                cancellationToken = cancellationToken.SafeRestart();
                SetCoordinatesAsync(CoordinateUtils.ParseCoordinatesString(teleportData.destination), null, null, cancellationToken.Token).Forget();
                break;
        }
    }

    private void ClosePanel()
    {
        SetVisibility(false);
    }

    private void ReceivedRequestTeleportData(string current, string previous)
    {
        if (string.IsNullOrEmpty(current) || isVisible)
            return;

        RequestJSONTeleport(current);
    }

    private void SetCoordinates((ParcelCoordinates coordinates, string realm, Action onAcceptedCallback) current,
        (ParcelCoordinates coordinates, string realm, Action onAcceptedCallback) previous)
    {
        if (current == previous) return;

        view.Reset();
        cancellationToken = cancellationToken.SafeRestart();
        SetCoordinatesAsync(current.coordinates, current.realm, current.onAcceptedCallback, cancellationToken.Token).Forget();
    }

    private async UniTaskVoid SetCoordinatesAsync(ParcelCoordinates coordinates, string realm,
        Action onAcceptedCallback, CancellationToken cancellationToken)
    {
        try
        {
            await minimapApiBridge.GetScenesInformationAroundParcel(new Vector2Int(coordinates.x, coordinates.y), 2, cancellationToken);
            MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(coordinates.x, coordinates.y);

            view.ShowTeleportToCoords(coordinates.ToString(), sceneInfo?.name, sceneInfo?.owner, sceneInfo?.previewImageUrl);

            teleportData = new TeleportData
            {
                coordinates = coordinates,
                sceneData = sceneInfo,
                sceneEvent = currentEvent,
                realm = realm,
                onAcceptedCallback = onAcceptedCallback,
            };

            SetSceneEvent(teleportData.sceneEvent);

            await UniTask.Delay(INITIAL_ANIMATION_DELAY, cancellationToken: cancellationToken);

            view.SetLoadingCompleted();
        }
        catch (OperationCanceledException)
        {
            teleportData = new TeleportData
            {
                destination = coordinates.ToString(),
                sceneData = null,
                sceneEvent = null,
            };
        }
        catch (Exception e) { Debug.LogException(e); }
    }

    private void CloseView() =>
        SetVisibility(false);

    private void ChangeVisibility(bool current, bool previous) =>
        SetVisibility(current);

    private bool RequestCoordinatesTeleport(int xCoordinate, int yCoordinate)
    {
        Utils.UnlockCursor();

        ParcelCoordinates coordinates = new ParcelCoordinates(xCoordinate, yCoordinate);
        teleportData = new TeleportData() { coordinates = coordinates };

        SetVisibility(true);
        cancellationToken = cancellationToken.SafeRestart();
        SetCoordinatesAsync(coordinates, null, null, cancellationToken.Token).Forget();
        return true;
    }

    private void SetSceneEvent(EventData eventData)
    {
        if (eventData == null) return;

        DateTime dateNow = DateTime.Now;
        DateTime eventStart;
        DateTime eventEnd;

        if (DateTime.TryParse(eventData.start_at, out eventStart) && DateTime.TryParse(eventData.finish_at, out eventEnd))
        {
            bool startsToday = eventStart.Date == dateNow.Date;
            bool isNow = eventStart <= dateNow && dateNow <= eventEnd;

            if (isNow || startsToday)
            {
                string eventStatus = EVENT_STRING_LIVE;

                if (!isNow && startsToday)
                    eventStatus = string.Format(EVENT_STRING_TODAY, eventStart);

                view.SetEventInfo(eventData.name, eventStatus, eventData.total_attendees);
            }
        }
    }

    private void OnTeleportPressed()
    {
        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_CROWD:
                teleportController.GoToCrowd();
                break;
            case TELEPORT_COMMAND_MAGIC:
                teleportController.GoToMagic();
                break;
            default:
                if (!string.IsNullOrEmpty(teleportData.realm))
                    teleportController.JumpIn(teleportData.GetCoordinates().x, teleportData.GetCoordinates().y, teleportData.realm, null);
                else
                    teleportController.Teleport(teleportData.GetCoordinates().x, teleportData.GetCoordinates().y);
                break;
        }

        teleportData?.onAcceptedCallback?.Invoke();

        CloseView();
    }

    [Serializable]
    internal class TeleportData
    {
        public Vector2Int? coordinates;

        public Vector2Int GetCoordinates() =>
            (coordinates ??= new Vector2Int(int.TryParse(destination.Split(',')[0], out int x) ? x : 0,
                int.TryParse(destination.Split(',')[1], out int y) ? y : 0));

        public string destination = "";
        public MinimapMetadata.MinimapSceneInfo sceneData;
        public EventData sceneEvent;
        public string realm;
        public Action onAcceptedCallback;
    }

    [Serializable]
    internal class EventData
    {
        public string name;
        public int total_attendees;
        public string start_at;
        public string finish_at;
    }
}
