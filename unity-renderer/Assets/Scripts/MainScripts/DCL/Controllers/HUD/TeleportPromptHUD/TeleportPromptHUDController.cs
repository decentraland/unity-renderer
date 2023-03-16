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
    const string TELEPORT_COMMAND_MAGIC = "magic";
    const string TELEPORT_COMMAND_CROWD = "crowd";

    const string EVENT_STRING_LIVE = "Current event";
    const string EVENT_STRING_TODAY = "Today @ {0:HH:mm}";

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
        view.content.SetActive(false);
        view.OnTeleportEvent += OnTeleportPressed;
        dataStore.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
        dataStore.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
    }

    private void SetCoordinates(ParcelCoordinates current, ParcelCoordinates previous)
    {
        if (current == previous) return;

        async UniTaskVoid SetCoordinatesAsync(ParcelCoordinates coordinates, CancellationToken cancellationToken)
        {
            //view.ShowLoading();


            try
            {
                MinimapMetadata.MinimapSceneInfo[] scenes = await minimapApiBridge.GetScenesInformationAroundParcel(new Vector2Int(coordinates.x, coordinates.y),
                    2,
                    cancellationToken);

                MinimapMetadata.MinimapSceneInfo sceneInfo = scenes.First(info => info.parcels.Exists(i => i.x == coordinates.x && i.y == coordinates.y));
                view.ShowTeleportToCoords(coordinates.ToString(), sceneInfo.name, sceneInfo.owner, sceneInfo.previewImageUrl);
                UniTask.Delay(500, cancellationToken: cancellationToken);
                view.SetLoadingCompleted();
            }
            catch (OperationCanceledException)
            {
                //view.SetPanelInfo(coordinates, null);
            }
            catch (Exception e)
            {
                //view.SetPanelInfo(coordinates, null);
                Debug.LogException(e);
            }
            finally
            {
                //view.HideLoading();
            }
        }

        cancellationToken = cancellationToken.SafeRestart();
        SetCoordinatesAsync(current, cancellationToken.Token).Forget();
    }

    private void ChangeVisibility(bool current, bool previous)
    {
        SetVisibility(current);
    }

    public void SetVisibility(bool visible)
    {
        if (view.contentAnimator.isVisible && !visible)
        {
            Debug.Log("Hide");
            view.SetOutAnimation();
            view.contentAnimator.Hide();
        }
        else if (!view.contentAnimator.isVisible && visible)
        {
            view.content.SetActive(true);
            view.contentAnimator.Show();

            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void RequestTeleport(string teleportDataJson)
    {
        if (view.contentAnimator.isVisible)
            return;

        Utils.UnlockCursor();

        view.Reset();
        SetVisibility(true);

        teleportData = Utils.SafeFromJson<TeleportData>(teleportDataJson);

        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_MAGIC:
                view.ShowTeleportToMagic();
                break;
            case TELEPORT_COMMAND_CROWD:
                view.ShowTeleportToCrowd();
                break;
            default:
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
        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_CROWD:
                DCL.Environment.i.world.teleportController.GoToCrowd();
                break;
            case TELEPORT_COMMAND_MAGIC:
                DCL.Environment.i.world.teleportController.GoToMagic();
                break;
            default:
                int x, y;
                string[] coordSplit = teleportData.destination.Split(',');
                if (coordSplit.Length == 2 && int.TryParse(coordSplit[0], out x) && int.TryParse(coordSplit[1], out y))
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
