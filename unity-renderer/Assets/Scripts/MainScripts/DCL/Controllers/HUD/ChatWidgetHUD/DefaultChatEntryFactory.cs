using DCL.Interface;
using UnityEngine;

namespace DCL.Social.Chat
{
    [CreateAssetMenu(fileName = "DefaultChatEntryFactory", menuName = "DCL/Social/DefaultChatEntryFactory")]
    public class DefaultChatEntryFactory : ScriptableObject, IChatEntryFactory
    {
        [SerializeField] private DefaultChatEntry defaultMessagePrefab;
        [SerializeField] private DefaultChatEntry systemMessagePrefab;
        [SerializeField] private DefaultChatEntry privateReceivedMessagePrefab;
        [SerializeField] private DefaultChatEntry privateSentMessagePrefab;
        [SerializeField] private DefaultChatEntry publicReceivedMessagePrefab;
        [SerializeField] private DefaultChatEntry publicSentMessagePrefab;

        public ChatEntry Create(ChatEntryModel model)
        {
            if (model.messageType == ChatMessage.Type.SYSTEM)
                return Instantiate(systemMessagePrefab);
            if (model.messageType == ChatMessage.Type.PUBLIC)
            {
                if (model.subType == ChatEntryModel.SubType.RECEIVED)
                    return Instantiate(publicReceivedMessagePrefab);
                if (model.subType == ChatEntryModel.SubType.SENT)
                    return Instantiate(publicSentMessagePrefab);
            }
            else if (model.messageType == ChatMessage.Type.PRIVATE)
            {
                if (model.subType == ChatEntryModel.SubType.RECEIVED)
                    return Instantiate(privateReceivedMessagePrefab);
                if (model.subType == ChatEntryModel.SubType.SENT)
                    return Instantiate(privateSentMessagePrefab);
            }
            return Instantiate(defaultMessagePrefab);
        }

        public void Destroy(ChatEntry entry)
        {
            if (!entry) return;
            Destroy(entry.gameObject);
        }
    }
}
