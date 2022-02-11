using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollapsableSortedFriendEntryList : MonoBehaviour
{
    private readonly Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();
    private List<(string userId, ulong timestamp)> sortedTimestamps = new List<(string userId, ulong timestamp)>();

    [SerializeField] private Transform container;
    [SerializeField] private FriendsListToggleButton toggleButton;

    private string amountOriginalText;

    public void Expand() => toggleButton.Toggle(true);
    
    public void Collapse() => toggleButton.Toggle(false);

    public int Count()
    {
        return entries.Count;
    }

    public void Add(string userId, FriendEntryBase entry)
    {
        if (entries.ContainsKey(userId)) return;
        entries.Add(userId, entry);
        var entryTransform = entry.transform;
        entryTransform.SetParent(container, false);
        entryTransform.localScale = Vector3.one;
        SortEntries();
    }

    public FriendEntryBase Remove(string userId)
    {
        if (!entries.ContainsKey(userId)) return null;
        var entry = entries[userId];
        entries.Remove(userId);
        return entry;
    }

    public void SetTimestamp((string userId, ulong timestamp) timestamp)
    {
        if (timestamp == default) return;
        var existingTimestamp = sortedTimestamps.FirstOrDefault(t => t.userId == timestamp.userId);
        if (existingTimestamp == default)
            sortedTimestamps.Add(timestamp);
        else if (timestamp.timestamp > existingTimestamp.timestamp)
            existingTimestamp.timestamp = timestamp.timestamp;
        SortTimestamps();
        SortEntries();
    }

    public (string userId, ulong timestamp) RemoveTimestamp(string userId)
    {
        var timestampToRemove = sortedTimestamps.FirstOrDefault(t => t.userId == userId);
        if (timestampToRemove == default) return default;
        sortedTimestamps.Remove(timestampToRemove);
        return timestampToRemove;
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
    }

    private void SortTimestamps()
    {
        sortedTimestamps = sortedTimestamps.OrderByDescending(pair => pair.timestamp).ToList();
    }

    private void SortEntries()
    {
        foreach (var item in sortedTimestamps.Where(item => entries.ContainsKey(item.userId)))
        {
            entries[item.userId].transform.SetAsLastSibling();
        }
    }
}