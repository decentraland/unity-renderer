using System;

[Serializable]
public class PublicChatModel : BaseComponentModel
{
    public string channelId;
    public string name;
    public string description;
    public long lastMessageTimestamp;
    public bool joined;
    public int memberCount;

    public PublicChatModel(string channelId, string name, string description, long lastMessageTimestamp, bool joined,
        int memberCount)
    {
        this.channelId = channelId;
        this.name = name;
        this.description = description;
        this.lastMessageTimestamp = lastMessageTimestamp;
        this.joined = joined;
        this.memberCount = memberCount;
    }

    public void CopyFrom(PublicChatModel model)
    {
        channelId = model.channelId;
        name = model.name;
        description = model.description;
        lastMessageTimestamp = model.lastMessageTimestamp;
        joined = model.joined;
        memberCount = model.memberCount;
    }
}