using System;

[Serializable]
public class WorldChatWindowModel : BaseComponentModel
{
    public PrivateChatEntry.PrivateChatEntryModel[] privateChats;
    public PublicChannelEntry.PublicChannelEntryModel[] publicChannels;
    public bool isLoadingDirectChats;
}