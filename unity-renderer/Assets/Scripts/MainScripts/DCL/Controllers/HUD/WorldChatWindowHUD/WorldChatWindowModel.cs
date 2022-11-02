using System;

namespace DCL.Chat.HUD
{
    [Serializable]
    public class WorldChatWindowModel : BaseComponentModel
    {
        public PrivateChatEntryModel[] privateChats;
        public bool isLoadingDirectChats;

        public PublicChatEntryModel[] publicChannels;
        public bool isLoadingChannels;
    }
}