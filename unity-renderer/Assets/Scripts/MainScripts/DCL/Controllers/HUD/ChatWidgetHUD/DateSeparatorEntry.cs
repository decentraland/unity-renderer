using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Special type of entry to be used as date separator in chat conversations.
/// </summary>
public class DateSeparatorEntry : ChatEntry
{
    private DateTime timestamp;
    private ChatEntryModel chatEntryModel;

    [SerializeField] internal TextMeshProUGUI title;

    public override ChatEntryModel Model => chatEntryModel;

    public override void Populate(ChatEntryModel model)
    {
        chatEntryModel = model;
        title.text = GetDateFormat(GetDateTimeFromUnixTimestampMilliseconds(model.timestamp));
    }

    public override void SetFadeout(bool enabled)
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
            result = date.ToLongDateString();
        }

        return result;
    }
    
    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}