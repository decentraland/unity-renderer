using DCL.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultChatEntryFactory", menuName = "DCL/Social/DefaultChatEntryFactory")]
public class DefaultChatEntryFactory : ScriptableObject, IChatEntryFactory
{
    [SerializeField] private ChatEntry defaultMessagePrefab;
    [SerializeField] private ChatEntry systemMessagePrefab;
    [SerializeField] private ChatEntry privateReceivedMessagePrefab;
    [SerializeField] private ChatEntry privateSentMessagePrefab;
    [SerializeField] private ChatEntry publicReceivedMessagePrefab;
    [SerializeField] private ChatEntry publicSentMessagePrefab;
    
    public ChatEntry Create(ChatEntry.Model model)
    {
        if (model.messageType == ChatMessage.Type.SYSTEM)
            return Instantiate(systemMessagePrefab);
        if (model.messageType == ChatMessage.Type.PUBLIC)
        {
            if (model.subType == ChatEntry.Model.SubType.RECEIVED)
                return Instantiate(publicReceivedMessagePrefab);
            if (model.subType == ChatEntry.Model.SubType.SENT)
                return Instantiate(publicSentMessagePrefab);
        }
        else if (model.messageType == ChatMessage.Type.PRIVATE)
        {
            if (model.subType == ChatEntry.Model.SubType.RECEIVED)
                return Instantiate(privateReceivedMessagePrefab);
            if (model.subType == ChatEntry.Model.SubType.SENT)
                return Instantiate(privateSentMessagePrefab);
        }
        return Instantiate(defaultMessagePrefab);
    }
}