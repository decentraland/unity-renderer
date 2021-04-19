using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Interface;

public class TeleportPromptHUDController : IHUD
{
    const string TELEPORT_COMMAND_MAGIC = "magic";
    const string TELEPORT_COMMAND_CROWD = "crowd";

    const string EVENT_STRING_LIVE = "Current event";
    const string EVENT_STRING_TODAY = "Today @ {0:HH:mm}";

    internal TeleportPromptHUDView view { get; private set; }

    TeleportData teleportData;

    public TeleportPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("TeleportPromptHUD")).GetComponent<TeleportPromptHUDView>();
        view.name = "_TeleportPromptHUD";
        view.content.SetActive(false);
        view.OnTeleportEvent += OnTeleportPressed;
    }

    public void SetVisibility(bool visible)
    {
        if (view.contentAnimator.isVisible && !visible)
        {
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
                if (!isNow && startsToday) eventStatus = string.Format(EVENT_STRING_TODAY, eventStart);
                view.SetEventInfo(eventData.name, eventStatus, eventData.total_attendees);
            }
        }
    }

    private void OnTeleportPressed()
    {
        switch (teleportData.destination)
        {
            case TELEPORT_COMMAND_CROWD:
                WebInterface.GoToCrowd();
                break;
            case TELEPORT_COMMAND_MAGIC:
                WebInterface.GoToMagic();
                break;
            default:
                int x, y;
                string[] coordSplit = teleportData.destination.Split(',');
                if (coordSplit.Length == 2 && int.TryParse(coordSplit[0], out x) && int.TryParse(coordSplit[1], out y))
                {
                    WebInterface.GoTo(x, y);
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