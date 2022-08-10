using System;
using UIComponents.CollapsableSortedList;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class CollapsableChatSearchListComponentView : CollapsableSortedListComponentView<string, BaseComponentView>
    {
        [SerializeField] private CollapsableDirectChatListComponentView directChatList;
        [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;
    
        public void Initialize(IChatController chatController)
        {
            directChatList.Initialize(chatController);
            publicChannelList.Initialize(chatController);
        }

        public void Filter(string search)
        {
            directChatList.Filter(search);
            publicChannelList.Filter(search);
            UpdateEmptyState();
        }

        public void Filter(Func<PrivateChatEntry, bool> privateComparision,
            Func<PublicChatEntry, bool> publicComparision)
        {
            directChatList.Filter(privateComparision);
            publicChannelList.Filter(publicComparision);
            UpdateEmptyState();
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

        public void Clear(bool releaseEntriesFromPool)
        {
            directChatList.Clear(releaseEntriesFromPool);
            publicChannelList.Clear(releaseEntriesFromPool);
            UpdateEmptyState();
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
            var entry = (BaseComponentView) directChatList.Remove(key) ?? publicChannelList.Remove(key);
            UpdateEmptyState();
            return entry;
        }

        public void Set(PrivateChatEntryModel model)
        {
            directChatList.Set(model.userId, model);
            UpdateEmptyState();
        }

        public void Set(PublicChatEntryModel model)
        {
            publicChannelList.Set(model.channelId, model);
            UpdateEmptyState();
        }

        public void Export(CollapsablePublicChannelListComponentView publicChannelList,
            CollapsableDirectChatListComponentView privateChatList)
        {
            foreach (var pair in this.publicChannelList.Entries)
                publicChannelList.Add(pair.Key, pair.Value);
            foreach (var pair in directChatList.Entries)
                privateChatList.Add(pair.Key, pair.Value);
        
            Clear(false);
        }

        public void Import(CollapsablePublicChannelListComponentView publicChannelList,
            CollapsableDirectChatListComponentView privateChatList)
        {
            foreach (var pair in privateChatList.Entries)
                directChatList.Add(pair.Key, pair.Value);
            foreach (var pair in publicChannelList.Entries)
                this.publicChannelList.Add(pair.Key, pair.Value);
        
            privateChatList.Clear(false);
            publicChannelList.Clear(false);
            UpdateEmptyState();
        }
    }
}