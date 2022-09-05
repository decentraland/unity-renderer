using System;
using System.Collections.Generic;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    [SerializeField] private DateSeparatorEntry separatorEntryPrefab;
    
    private readonly Dictionary<string, DateSeparatorEntry> dateSeparators = new Dictionary<string, DateSeparatorEntry>();

    public override void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
    {
        AddSeparatorEntryIfNeeded(model);
        base.AddEntry(model, setScrollPositionToBottom);
    }

    private void AddSeparatorEntryIfNeeded(ChatEntryModel chatEntryModel)
    {
        var entryDateTime = GetDateTimeFromUnixTimestampMilliseconds(chatEntryModel.timestamp).Date;
        var separatorId = $"{entryDateTime.Ticks}";
        if (dateSeparators.ContainsKey(separatorId)) return;
        var dateSeparatorEntry = Instantiate(separatorEntryPrefab, chatEntriesContainer);
        dateSeparatorEntry.Populate(chatEntryModel);
        dateSeparatorEntry.SetFadeout(IsFadeoutModeEnabled);
        dateSeparators[separatorId] = dateSeparatorEntry;
        SetEntry(separatorId, dateSeparatorEntry);
    }

    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}