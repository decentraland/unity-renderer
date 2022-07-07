using System;

[Serializable]
public class WorldChatWindowModel : BaseComponentModel
{
    public PrivateChatEntry.PrivateChatEntryModel[] privateChats;
    public PublicChatEntry.PublicChatEntryModel[] publicChannels;
    public bool isLoadingDirectChats;
}