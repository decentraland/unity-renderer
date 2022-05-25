using System.Text.RegularExpressions;
using UIComponents.CollapsableSortedList;

public class CollapsableSortedFriendEntryList : CollapsableSortedListComponentView<string, FriendEntryBase>
{
    public void Filter(string search)
    {   
        if (!gameObject.activeInHierarchy) return;
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry =>
        {
            if (regex.IsMatch(entry.Model.userId)) return true;
            return !string.IsNullOrEmpty(entry.Model.userName) && regex.IsMatch(entry.Model.userName);
        });
        // throttling may intruduce race conditions & artifacts into the ui
        // StartCoroutine(FilterAsync(entry => regex.IsMatch(entry.model.userId)
        //     || regex.IsMatch(entry.model.userName)));
    }
}