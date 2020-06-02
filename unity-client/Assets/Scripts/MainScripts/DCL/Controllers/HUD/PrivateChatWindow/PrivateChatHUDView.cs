using DCL.Helpers;
using System;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    string ENTRY_PATH_SENT = "ChatEntrySent";
    string ENTRY_PATH_RECEIVED = "ChatEntryReceived";
    string ENTRY_PATH_SEPARATOR = "ChatEntrySeparator";

    public override void AddEntry(ChatEntry.Model chatEntryModel, bool setScrollPositionToBottom = false)
    {
        AddSeparatorEntryIfNeeded(chatEntryModel);

        var chatEntryGO = Instantiate(Resources.Load(chatEntryModel.subType == ChatEntry.Model.SubType.PRIVATE_TO ? ENTRY_PATH_SENT : ENTRY_PATH_RECEIVED) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        chatEntry.SetFadeout(false);
        chatEntry.Populate(chatEntryModel);

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;

        entries.Add(chatEntry);
        Utils.ForceUpdateLayout(transform as RectTransform, delayed: false);

        if (setScrollPositionToBottom)
            scrollRect.verticalNormalizedPosition = 0;
    }

    protected override void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        (messageHoverPanel.transform as RectTransform).pivot = new Vector2(chatEntry.model.subType == ChatEntry.Model.SubType.PRIVATE_TO ? 1 : 0, 0.5f);

        base.OnMessageTriggerHover(chatEntry);
    }

    private void AddSeparatorEntryIfNeeded(ChatEntry.Model chatEntryModel)
    {
        DateTime entryDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        entryDateTime = GetDateTimeFromUnixTimestampMilliseconds(chatEntryModel.timestamp);

        if (!dateSeparators.Exists(separator =>
            separator.model.date.Year == entryDateTime.Year &&
            separator.model.date.Month == entryDateTime.Month &&
            separator.model.date.Day == entryDateTime.Day))
        {
            var chatEntrySeparatorGO = Instantiate(Resources.Load(ENTRY_PATH_SEPARATOR) as GameObject, chatEntriesContainer);
            DateSeparatorEntry dateSeparatorEntry = chatEntrySeparatorGO.GetComponent<DateSeparatorEntry>();
            dateSeparatorEntry.Populate(new DateSeparatorEntry.Model
            {
                date = entryDateTime
            });
            dateSeparators.Add(dateSeparatorEntry);
        }
    }

    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        System.DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}
