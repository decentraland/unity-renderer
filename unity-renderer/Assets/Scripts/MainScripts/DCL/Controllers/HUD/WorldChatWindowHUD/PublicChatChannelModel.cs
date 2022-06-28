﻿using System;

[Serializable]
public class PublicChatChannelModel : BaseComponentModel
{
    public string channelId;
    public string name;
    public string description;

    public PublicChatChannelModel(string channelId, string name, string description)
    {
        this.channelId = channelId;
        this.name = name;
        this.description = description;
    }

    public void CopyFrom(PublicChatChannelModel model)
    {
        channelId = model.channelId;
        name = model.name;
        description = model.description;
    }
}