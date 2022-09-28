using System;
using UIComponents.CollapsableSortedList;
using UnityEngine;

public class CollapsableChatSearchListComponentView : CollapsableSortedListComponentView<string, BaseComponentView>
{
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;
    
    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
        publicChannelList.Initialize(chatController);
    }

    public override void Filter(Func<BaseComponentView, bool> comparision)
    {
        directChatList.Filter(comparision);
        publicChannelList.Filter(comparision);
        UpdateEmptyState();
    }

    public override int Count()
    {
        return directChatList.Count() + publicChannelList.Count();
    }

    public override void Clear()
    {
        directChatList.Clear();
        publicChannelList.Clear();
        UpdateEmptyState();
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
        var view = (BaseComponentView) directChatList.Remove(key) ?? publicChannelList.Remove(key);
        UpdateEmptyState();
        return view;
    }

    public void Set(PrivateChatEntry.PrivateChatEntryModel model)
    {
        directChatList.Set(model.userId, model);
        UpdateEmptyState();
    }
    
    public void Set(PublicChannelEntry.PublicChannelEntryModel model)
    {
        publicChannelList.Set(model.channelId, model);
        UpdateEmptyState();
    }
}