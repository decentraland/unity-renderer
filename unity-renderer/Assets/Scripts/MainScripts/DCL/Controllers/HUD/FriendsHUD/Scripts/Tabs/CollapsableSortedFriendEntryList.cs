using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Helpers;
using UnityEngine;

public class CollapsableSortedFriendEntryList : MonoBehaviour
{
    private readonly Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();
    private List<UserTimestamp> sortedTimestamps = new List<UserTimestamp>();

    [SerializeField] private Transform container;
    [SerializeField] private FriendsListToggleButton toggleButton;
    [SerializeField] private GameObject emptyStateContainer;

    private void OnEnable()
    {
        UpdateEmptyState();
    }

    public void Expand() => toggleButton.Toggle(true);

    public void Collapse() => toggleButton.Toggle(false);

    public int Count() => entries.Count;

    public void Add(string userId, FriendEntryBase entry)
    {
        if (entries.ContainsKey(userId)) return;
        entries.Add(userId, entry);
        var entryTransform = entry.transform;
        entryTransform.SetParent(container, false);
        entryTransform.localScale = Vector3.one;
        SortEntries();
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
    }

    public FriendEntryBase Remove(string userId)
    {
        if (!entries.ContainsKey(userId)) return null;
        var entry = entries[userId];
        entries.Remove(userId);
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
        return entry;
    }

    public void SortEntriesByTimestamp(string userId, ulong timestamp)
    {
        var userTimestamp = sortedTimestamps.FirstOrDefault(ut => ut.UserId == userId);
        if (userTimestamp == default)
        {
            userTimestamp = new UserTimestamp(userId, timestamp);
            sortedTimestamps.Add(userTimestamp);
        }

        if (timestamp > userTimestamp.Timestamp)
            userTimestamp.Timestamp = timestamp;
        
        SortTimestamps();
        SortEntries();
    }

    public ulong RemoveTimestamp(string userId)
    {
        var timestampToRemove = sortedTimestamps.FirstOrDefault(ut => ut.UserId == userId);
        if (timestampToRemove == default) return 0;
        sortedTimestamps.Remove(timestampToRemove);
        return timestampToRemove.Timestamp;
    }

    public void Clear()
    {
        foreach (var userId in entries.Keys)
        {
            Remove(userId);
            RemoveTimestamp(userId);
        }

        entries.Clear();
        sortedTimestamps.Clear();
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
    }

    public void Filter(string search)
    {
        var regex = new Regex(search, RegexOptions.IgnoreCase);

        foreach (var entry in entries)
        {
            var isMatch = regex.IsMatch(entry.Key)
                          || regex.IsMatch(entry.Value.model.userName)
                          || regex.IsMatch(entry.Value.model.realm);
            entry.Value.gameObject.SetActive(isMatch);
        }

        ((RectTransform) container).ForceUpdateLayout();
    }

    private void SortTimestamps()
    {
        sortedTimestamps = sortedTimestamps.OrderByDescending(ut => ut.Timestamp).ToList();
    }

    private void SortEntries()
    {
        foreach (var item in sortedTimestamps.Where(item => entries.ContainsKey(item.UserId)))
            entries[item.UserId].transform.SetAsLastSibling();
    }

    private void UpdateEmptyState()
    {
        emptyStateContainer.SetActive(entries.Count == 0);
    }
    
    private class UserTimestamp
    {
        public string UserId { get; set; }
        public ulong Timestamp { get; set; }
        
        public UserTimestamp(string userId, ulong timestamp)
        {
            UserId = userId;
            Timestamp = timestamp;
        }
    }
}