using System;
using System.Text.RegularExpressions;
using DCL.Chat.HUD;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsablePublicChannelListComponentView : CollapsableSortedListComponentView<string, PublicChatEntry>
{
    [SerializeField] private ChannelEntryFactory entryFactory;

    private bool releaseEntriesFromPool = true;
    private IChatController chatController;

    public event Action<PublicChatEntry> OnOpenChat;
    
    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Filter(string search)
    {
        var regex = new Regex(search, RegexOptions.IgnoreCase);
        Filter(entry => regex.IsMatch(entry.Model.name));
    }

    public void Clear(bool releaseEntriesFromPool)
    {
        // avoids releasing instances from pool just for this clear
        this.releaseEntriesFromPool = releaseEntriesFromPool;
        base.Clear();
        this.releaseEntriesFromPool = true;
    }

    public override PublicChatEntry Remove(string key)
    {
        var entry = base.Remove(key);
        
        if (releaseEntriesFromPool && entry)
            Destroy(entry.gameObject);
        
        return entry;
    }

    public void Set(string channelId, PublicChatEntryModel entryModel)
    {
        if (!Contains(entryModel.channelId))
            CreateEntry(channelId);
            
        var entry = Get(channelId);
        entry.Configure(entryModel);
    }
    
    private void CreateEntry(string channelId)
    {
        var newFriendEntry = entryFactory.Create(channelId);
        var entry = newFriendEntry.gameObject.GetComponent<PublicChatEntry>();
        Add(channelId, entry);
        entry.Initialize(chatController);
        entry.OnOpenChat -= HandleEntryOpenChat;
        entry.OnOpenChat += HandleEntryOpenChat;
    }

    private void HandleEntryOpenChat(PublicChatEntry entry) => OnOpenChat?.Invoke(entry);
}