using System;
using System.Globalization;
using DCL.Chat.HUD;
using TMPro;
using UnityEngine;

/// <summary>
/// Special type of entry to be used as date separator in chat conversations.
/// </summary>
public class DateSeparatorEntry : ChatEntry
{
    [SerializeField] internal TextMeshProUGUI title;

    private DateTime timestamp;
    private ChatEntryModel chatEntryModel;

    public override string HoverString =>
        GetDateFormat(GetDateTimeFromUnixTimestampMilliseconds(Model.timestamp));
    public override event Action<ChatEntry> OnUserNameClicked;
    public override event Action<ChatEntry> OnTriggerHover;
    public override event Action<ChatEntry, ParcelCoordinates> OnTriggerHoverGoto;
    public override event Action OnCancelHover;
    public override event Action OnCancelGotoHover;
    public override ChatEntryModel Model => chatEntryModel;

    public override void Populate(ChatEntryModel model)
    {
        chatEntryModel = model;
        title.text = GetDateFormat(GetDateTimeFromUnixTimestampMilliseconds(model.timestamp));
    }

    public override void SetFadeout(bool enabled)
    {
        if (enabled) return;
        group.alpha = 1;
    }

    public override void DockContextMenu(RectTransform panel)
    {
    }

    public override void DockHoverPanel(RectTransform panel)
    {
    }

    public override void ConfigureMentionLinkDetector(UserContextMenu userContextMenu)
    {
    }

    private string GetDateFormat(DateTime date)
    {
        string result = string.Empty;

        if (date.Year == DateTime.Now.Year &&
            date.Month == DateTime.Now.Month &&
            date.Day == DateTime.Now.Day)
        {
            result = "Today";
        }
        else
        {
            result = date.ToString("D", DateTimeFormatInfo.InvariantInfo);
        }

        return result;
    }

    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}
