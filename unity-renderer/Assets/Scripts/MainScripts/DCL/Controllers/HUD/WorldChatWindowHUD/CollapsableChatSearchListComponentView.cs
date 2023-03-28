using System;
using UIComponents.CollapsableSortedList;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class CollapsableChatSearchListComponentView : CollapsableSortedListComponentView<string, BaseComponentView>
    {
        [SerializeField] private CollapsableDirectChatListComponentView directChatList;
        [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;

        public event Action<PrivateChatEntry> OnOpenPrivateChat
        {
            add => directChatList.OnOpenChat += value;
            remove => directChatList.OnOpenChat += value;
        }

        public event Action<PublicChatEntry> OnOpenPublicChat
        {
            add => publicChannelList.OnOpenChat += value;
            remove => publicChannelList.OnOpenChat -= value;
        }

        public void Initialize(
            IChatController chatController,
            DataStore_Mentions mentionDataStore)
        {
            directChatList.Initialize(chatController, mentionDataStore);
            publicChannelList.Initialize(chatController, mentionDataStore);
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
            var entry = (BaseComponentView) directChatList.Remove(key) ?? publicChannelList.Remove(key);
            UpdateEmptyState();
            return entry;
        }

        public void Set(PrivateChatEntryModel model)
        {
            directChatList.Set(model.userId, model);
            directChatList.Get(model.userId).EnableAvatarSnapshotFetching();
            UpdateEmptyState();
        }

        public void Set(PublicChatEntryModel model)
        {
            publicChannelList.Set(model.channelId, model);
            UpdateEmptyState();
        }
    }
}
