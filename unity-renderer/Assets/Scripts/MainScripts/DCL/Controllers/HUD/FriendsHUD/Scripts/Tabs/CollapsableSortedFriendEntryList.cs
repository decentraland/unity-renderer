using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Helpers;
using UnityEngine;

public class CollapsableSortedFriendEntryList : MonoBehaviour
{
    private readonly Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();
    private readonly List<FriendEntryBase.Model> sortedFriends = new List<FriendEntryBase.Model>();

    [SerializeField] private Transform container;
    [SerializeField] private FriendsListToggleButton toggleButton;
    [SerializeField] private GameObject emptyStateContainer;

    private int filteredCount;

    public Comparison<FriendEntryBase.Model> SortingMethod { get; set; } = (model, model1) => 0;

    private void OnEnable()
    {
        UpdateEmptyState();
    }

    public void Expand() => toggleButton.Toggle(true);

    public void Collapse() => toggleButton.Toggle(false);

    public int Count() => entries.Count - filteredCount;

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void Add(string userId, FriendEntryBase entry)
    {
        if (entries.ContainsKey(userId)) return;
        entries.Add(userId, entry);
        sortedFriends.Add(entry.model);
        var entryTransform = entry.transform;
        entryTransform.SetParent(container, false);
        entryTransform.localScale = Vector3.one;
        Sort();
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
    }

    public FriendEntryBase Remove(string userId)
    {
        if (!entries.ContainsKey(userId)) return null;
        var entry = entries[userId];
        entries.Remove(userId);
        sortedFriends.RemoveAll(model => model.userId == userId);
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
        return entry;
    }

    public void Clear()
    {
        foreach (var userId in entries.Keys)
            Remove(userId);

        entries.Clear();
        sortedFriends.Clear();
        UpdateEmptyState();
        ((RectTransform) container).ForceUpdateLayout();
    }

    public void Filter(string search)
    {
        filteredCount = 0;
        var regex = new Regex(search, RegexOptions.IgnoreCase);

        foreach (var entry in entries)
        {
            var isMatch = regex.IsMatch(entry.Key)
                          || regex.IsMatch(entry.Value.model.userName)
                          || regex.IsMatch(entry.Value.model.realm);
            entry.Value.gameObject.SetActive(isMatch);
            
            if (!isMatch)
                filteredCount++;
        }

        ((RectTransform) container).ForceUpdateLayout();
    }

    public void Sort()
    {
        sortedFriends.Sort(SortingMethod);
        foreach (var item in sortedFriends.Where(item => entries.ContainsKey(item.userId)))
            entries[item.userId].transform.SetAsLastSibling();
    }

    private void UpdateEmptyState() => emptyStateContainer.SetActive(Count() == 0);
}