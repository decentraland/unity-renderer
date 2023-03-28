using System;
using DCL.Interface;
using UnityEngine.Serialization;

[Serializable]
public struct ChatEntryModel
{
    public enum SubType
    {
        NONE,
        RECEIVED,
        SENT
    }

    public ChatMessage.Type messageType;
    public string messageId;
    public string bodyText;
    public string senderId;
    public string senderName;
    public string recipientName;
    public ulong timestamp;
    public SubType subType;
    public bool isChannelMessage;
    public bool isLoadingNames;
}
