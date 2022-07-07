using System;

[Serializable]
public class PublicChatModel : BaseComponentModel
{
    public string channelId;
    public string name;
    public string description;
    public long lastMessageTimestamp;

    public PublicChatModel(string channelId, string name, string description, long lastMessageTimestamp)
    {
        this.channelId = channelId;
        this.name = name;
        this.description = description;
        this.lastMessageTimestamp = lastMessageTimestamp;
    }

    public void CopyFrom(PublicChatModel model)
    {
        channelId = model.channelId;
        name = model.name;
        description = model.description;
        lastMessageTimestamp = model.lastMessageTimestamp;
    }
}