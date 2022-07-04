using System;
using System.Collections.Generic;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    [SerializeField] private DateSeparatorEntry separatorEntryPrefab;
    
    private readonly Dictionary<DateTime, DateSeparatorEntry> dateSeparators = new Dictionary<DateTime, DateSeparatorEntry>();

    public override void AddEntry(ChatEntryModel model, bool setScrollPositionToBottom = false)
    {
        AddSeparatorEntryIfNeeded(model);
        base.AddEntry(model, setScrollPositionToBottom);
    }

    public override void ClearAllEntries()
    {
        base.ClearAllEntries();
        
        foreach (var separator in dateSeparators.Values)
            if (separator)
                Destroy(separator.gameObject);

        dateSeparators.Clear();
    }

    private void AddSeparatorEntryIfNeeded(ChatEntryModel chatEntryModel)
    {
        var entryDateTime = GetDateTimeFromUnixTimestampMilliseconds(chatEntryModel.timestamp).Date;
        if (dateSeparators.ContainsKey(entryDateTime)) return;
        var dateSeparatorEntry = Instantiate(separatorEntryPrefab, chatEntriesContainer);
        dateSeparatorEntry.Populate(chatEntryModel);
        dateSeparatorEntry.SetFadeout(IsFadeoutModeEnabled);
        dateSeparators.Add(entryDateTime, dateSeparatorEntry);
        entries.Add(dateSeparatorEntry);
    }

    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}