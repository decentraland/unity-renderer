using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Special type of entry to be used as date separator in chat conversations.
/// </summary>
public class DateSeparatorEntry : MonoBehaviour
{
    public struct Model
    {
        public DateTime date;
    }

    public Model model { get; private set; }

    [SerializeField] internal TextMeshProUGUI title;

    /// <summary>
    /// Configures the separator entry with the date information.
    /// </summary>
    /// <param name="dateSeparatorModel">DateSeparatorEntry.Model</param>
    public void Populate(Model dateSeparatorModel)
    {
        model = dateSeparatorModel;
        title.text = GetDateFormat(dateSeparatorModel.date);
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
}
