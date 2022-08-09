using System;

[Serializable]
public class WorldChatWindowModel : BaseComponentModel
{
    public PrivateChatEntry.PrivateChatEntryModel[] privateChats;
    public bool isLoadingDirectChats;

    public PublicChatEntry.PublicChatEntryModel[] publicChannels;
    public bool isLoadingChannels;
}