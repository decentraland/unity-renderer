using System;

[Serializable]
public class PublicChatChannelModel : BaseComponentModel
{
    public string channelId;
    public string name;
    public string description;
    public long lastMessageTimestamp;

    public PublicChatChannelModel(string channelId, string name, string description, long lastMessageTimestamp)
    {
        this.channelId = channelId;
        this.name = name;
        this.description = description;
        this.lastMessageTimestamp = lastMessageTimestamp;
    }

    public void CopyFrom(PublicChatChannelModel model)
    {
        channelId = model.channelId;
        name = model.name;
        description = model.description;
        lastMessageTimestamp = model.lastMessageTimestamp;
    }
}