using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;

namespace DCL.Social.Chat
{
    [CreateAssetMenu(fileName = "PoolChatEntryFactory", menuName = "DCL/Social/PoolChatEntryFactory")]
    public class PoolChatEntryFactory : ScriptableObject, IChatEntryFactory
    {
        private const string POOL_NAME_PREFIX = "ChatMessages_";
        private const int PRE_INSTANTIATED_ENTRIES = 30;

        [SerializeField] private DefaultChatEntry defaultMessagePrefab;
        [SerializeField] private DefaultChatEntry systemMessagePrefab;
        [SerializeField] private DefaultChatEntry privateReceivedMessagePrefab;
        [SerializeField] private DefaultChatEntry privateSentMessagePrefab;
        [SerializeField] private DefaultChatEntry publicReceivedMessagePrefab;
        [SerializeField] private DefaultChatEntry publicSentMessagePrefab;

        private readonly Dictionary<ChatEntry, PoolableObject> pooledObjects =
            new Dictionary<ChatEntry, PoolableObject>();

        public ChatEntry Create(ChatEntryModel model)
        {
            if (model.messageType == ChatMessage.Type.SYSTEM)
                return GetEntryFromPool("SYSTEM", systemMessagePrefab);

            if (model.messageType == ChatMessage.Type.PUBLIC)
            {
                if (model.subType == ChatEntryModel.SubType.RECEIVED)
                    return GetEntryFromPool("PUBLIC_RECEIVED", publicReceivedMessagePrefab);
                if (model.subType == ChatEntryModel.SubType.SENT)
                    return GetEntryFromPool("PUBLIC_SENT", publicSentMessagePrefab);
            }
            else if (model.messageType == ChatMessage.Type.PRIVATE)
            {
                if (model.subType == ChatEntryModel.SubType.RECEIVED)
                    return GetEntryFromPool("PRIVATE_RECEIVED", privateReceivedMessagePrefab);
                if (model.subType == ChatEntryModel.SubType.SENT)
                    return GetEntryFromPool("PRIVATE_SENT", privateSentMessagePrefab);
            }
            return GetEntryFromPool("DEFAULT", defaultMessagePrefab);
        }

        public void Destroy(ChatEntry entry)
        {
            if (!pooledObjects.TryGetValue(entry, out var pooledObj)) return;
            pooledObj.Release();
            pooledObjects.Remove(entry);
        }

        private DefaultChatEntry GetEntryFromPool(string poolName, DefaultChatEntry prefab)
        {
            var pool = GetPool(poolName, prefab);
            var pooledObj = pool.Get();
            var entry = pooledObj.gameObject.GetComponent<DefaultChatEntry>();
            pooledObjects[entry] = pooledObj;
            return entry;
        }

        private Pool GetPool(string poolName, DefaultChatEntry prefab)
        {
            var poolId = POOL_NAME_PREFIX + poolName + GetInstanceID();
            var entryPool = PoolManager.i.GetPool(poolId);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                poolId,
                Instantiate(prefab).gameObject,
                maxPrewarmCount: PRE_INSTANTIATED_ENTRIES,
                isPersistent: true);
            entryPool.ForcePrewarm();

            return entryPool;
        }
    }
}
