using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Map;
using DCL.Tasks;
using System.Linq;
using System.Threading;

public class TeleportPromptHUDController : IHUD
{
    private const string TELEPORT_COMMAND_MAGIC = "magic";
    private const string TELEPORT_COMMAND_CROWD = "crowd";

    private const string EVENT_STRING_LIVE = "Current event";
    private const string EVENT_STRING_TODAY = "Today @ {0:HH:mm}";
    private const int INITIAL_ANIMATION_DELAY = 500;

    internal TeleportPromptHUDView view { get; private set; }

    private readonly DataStore dataStore;
    private readonly IMinimapApiBridge minimapApiBridge;

    TeleportData teleportData;
    private CancellationTokenSource cancellationToken = new ();

    public TeleportPromptHUDController(DataStore dataStore, IMinimapApiBridge minimapApiBridge)
    {
        this.dataStore = dataStore;
        this.minimapApiBridge = minimapApiBridge;

        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("TeleportPromptHUD")).GetComponent<TeleportPromptHUDView>();
        view.name = "_TeleportPromptHUD";
        view.OnCloseEvent += view.SetOutAnimation;
        view.OnTeleportEvent += OnTeleportPressed;

        dataStore.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
        dataStore.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
        dataStore.world.requestTeleportData.OnChange += ReceivedRequestTeleportData;
    }

    private void ReceivedRequestTeleportData(string current, string previous)
    {
        if (string.IsNullOrEmpty(current))
            return;

        RequestTeleport(current);
    }

    private void SetCoordinates(ParcelCoordinates current, ParcelCoordinates previous)
    {
        if (current == previous) return;

        async UniTaskVoid SetCoordinatesAsync(ParcelCoordinates coordinates, CancellationToken cancellationToken)
        {
            try
            {
                MinimapMetadata.MinimapSceneInfo[] scenes = await minimapApiBridge.GetScenesInformationAroundParcel(new Vector2Int(coordinates.x, coordinates.y),
                    2,
                    cancellationToken);

                MinimapMetadata.MinimapSceneInfo sceneInfo = scenes.First(info => info.parcels.Exists(i => i.x == coordinates.x && i.y == coordinates.y));
                view.ShowTeleportToCoords(coordinates.ToString(), sceneInfo.name, sceneInfo.owner, sceneInfo.previewImageUrl);

                teleportData = new TeleportData()
                {
                    destination = coordinates.ToString(),
                    sceneData = sceneInfo,
                    sceneEvent = null
                };
                UniTask.Delay(INITIAL_ANIMATION_DELAY, cancellationToken: cancellationToken);
                view.SetLoadingCompleted();
            }
            catch (OperationCanceledException)
            {
                teleportData = new TeleportData()
                {
                    destination = coordinates.ToString(),
                    sceneData = null,
                    sceneEvent = null
                };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        cancellationToken = cancellationToken.SafeRestart();
        SetCoordinatesAsync(current, cancellationToken.Token).Forget();
    }

    private void ChangeVisibility(bool current, bool previous) =>
        SetVisibility(current);

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            view.SetInAnimation();
            AudioScriptableObjects.fadeIn.Play(true);
        }
        else
        {
            dataStore.HUDs.gotoPanelVisible.Set(false, false);
            view.SetOutAnimation();
            view.Reset();
        }
    }

    public void RequestTeleport(string teleportDataJson)
    {
        Utils.UnlockCursor();

        view.Reset();
        teleportData = Utils.SafeFromJson<TeleportData>(teleportDataJson);

        dataStore.HUDs.gotoPanelVisible.Set(true, true);
        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_MAGIC:
                view.ShowTeleportToMagic();
                break;
            case TELEPORT_COMMAND_CROWD:
                view.ShowTeleportToCrowd();
                break;
            default:
                dataStore.HUDs.gotoPanelCoordinates.Set(CoordinateUtils.ParseCoordinatesString(teleportData.destination));
                view.ShowTeleportToCoords(teleportData.destination,
                    teleportData.sceneData.name,
                    teleportData.sceneData.owner,
                    teleportData.sceneData.previewImageUrl);
                SetSceneEvent();
                break;
        }
    }

    public void Dispose()
    {
        if (view)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
        dataStore.HUDs.gotoPanelVisible.OnChange -= ChangeVisibility;
        dataStore.HUDs.gotoPanelCoordinates.OnChange -= SetCoordinates;
    }

    private void SetSceneEvent()
    {
        EventData eventData = teleportData.sceneEvent;

        if (eventData == null)
            return;

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
        ChangeVisibility(false, false);
        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_CROWD:
                DCL.Environment.i.world.teleportController.GoToCrowd();
                break;
            case TELEPORT_COMMAND_MAGIC:
                DCL.Environment.i.world.teleportController.GoToMagic();
                break;
            default:
                string[] coordSplit = teleportData.destination.Split(',');
                if (coordSplit.Length == 2 && int.TryParse(coordSplit[0], out int x) && int.TryParse(coordSplit[1], out int y))
                {
                    DCL.Environment.i.world.teleportController.Teleport(x, y);
                }

                break;
        }
    }

    [Serializable]
    internal class TeleportData
    {
        public string destination = "";
        public MinimapMetadata.MinimapSceneInfo sceneData = null;
        public EventData sceneEvent = null;
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
