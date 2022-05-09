using System.Text.RegularExpressions;
using UIComponents.CollapsableSortedList;

public class CollapsableSortedFriendEntryList : CollapsableSortedListComponentView<string, FriendEntryBase>
{
    public void Filter(string search)
    {   
        if (!gameObject.activeInHierarchy) return;
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        StartCoroutine(FilterAsync(entry => regex.IsMatch(entry.model.userId)
            || regex.IsMatch(entry.model.userName)
            /*|| regex.IsMatch(entry.model.realm)*/));
    }
}