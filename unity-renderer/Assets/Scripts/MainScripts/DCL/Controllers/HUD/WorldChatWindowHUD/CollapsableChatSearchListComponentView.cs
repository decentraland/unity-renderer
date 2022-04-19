using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsableChatSearchListComponentView : CollapsableSortedListComponentView<string, BaseComponentView>
{
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;
    
    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
    }

    public void Filter(string search)
    {
        directChatList.Filter(search);
        publicChannelList.Filter(search);
    }

    public override int Count()
    {
        return directChatList.Count() + publicChannelList.Count();
    }

    public override void Clear()
    {
        directChatList.Clear();
        publicChannelList.Clear();
    }

    public override BaseComponentView Get(string key)
    {
        return (BaseComponentView) directChatList.Get(key) ?? publicChannelList.Get(key);
    }

    public override void Dispose()
    {
        base.Dispose();
        directChatList.Dispose();
        publicChannelList.Dispose();
    }

    public override BaseComponentView Remove(string key)
    {
        return (BaseComponentView) directChatList.Remove(key) ?? publicChannelList.Remove(key);
    }

    public void Export(CollapsablePublicChannelListComponentView publicChannelList,
        CollapsableDirectChatListComponentView privateChatList)
    {
        foreach (var pair in this.publicChannelList.Entries)
            publicChannelList.Set(pair.Key, pair.Value.Model);
        foreach (var pair in directChatList.Entries)
            privateChatList.Set(pair.Key, pair.Value.Model);
    }

    public void Import(CollapsablePublicChannelListComponentView publicChannelList,
        CollapsableDirectChatListComponentView privateChatList)
    {
        foreach (var pair in privateChatList.Entries)
            directChatList.Set(pair.Key, pair.Value.Model);
        foreach (var pair in publicChannelList.Entries)
            this.publicChannelList.Set(pair.Key, pair.Value.Model);
    }
}