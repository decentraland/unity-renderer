using System;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    [SerializeField] private DateSeparatorEntry separatorEntryPrefab;

    public override void AddEntry(ChatEntry.Model model, bool setScrollPositionToBottom = false)
    {
        AddSeparatorEntryIfNeeded(model);
        base.AddEntry(model, setScrollPositionToBottom);
    }

    protected override void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        (messageHoverPanel.transform as RectTransform).pivot =
            new Vector2(chatEntry.model.subType == ChatEntry.Model.SubType.SENT ? 1 : 0, 0.5f);
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
            var dateSeparatorEntry = Instantiate(separatorEntryPrefab, chatEntriesContainer);
            dateSeparatorEntry.Populate(new DateSeparatorEntry.Model
            {
                date = entryDateTime
            });
            dateSeparators.Add(dateSeparatorEntry);
        }
    }

    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}