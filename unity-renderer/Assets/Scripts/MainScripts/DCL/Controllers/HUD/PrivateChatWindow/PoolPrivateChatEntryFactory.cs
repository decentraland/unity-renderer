using System.Collections.Generic;
using UnityEngine;

namespace DCL.Chat.HUD
{
    [CreateAssetMenu(fileName = "PoolPrivateChatEntryFactory", menuName = "DCL/Social/PoolPrivateChatEntryFactory")]
    public class PoolPrivateChatEntryFactory : ScriptableObject, IChatEntryFactory
    {
        private const string POOL_NAME_PREFIX = "ChatDateSeparators_";
        private const int PRE_INSTANTIATED_ENTRIES = 30;
        
        [SerializeField] private PoolChatEntryFactory factory;
        [SerializeField] private DateSeparatorEntry separatorEntryPrefab;
        
        private readonly Dictionary<ChatEntry, PoolableObject> pooledObjects =
            new Dictionary<ChatEntry, PoolableObject>();

        public DateSeparatorEntry CreateDateSeparator()
        {
            var pool = GetPool();
            var pooledObj = pool.Get();
            var entry = pooledObj.gameObject.GetComponent<DateSeparatorEntry>();
            pooledObjects[entry] = pooledObj;
            return entry;
        }

        public ChatEntry Create(ChatEntryModel model) => factory.Create(model);

        public void Destroy(ChatEntry entry)
        {
            if (pooledObjects.TryGetValue(entry, out var pooledObj))
            {
                pooledObj.Release();
                pooledObjects.Remove(entry);    
            }
            
            factory.Destroy(entry);
        }

        private Pool GetPool()
        {
            var poolId = POOL_NAME_PREFIX + GetInstanceID();
            var entryPool = PoolManager.i.GetPool(poolId);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                poolId,
                Instantiate(separatorEntryPrefab).gameObject,
                maxPrewarmCount: PRE_INSTANTIATED_ENTRIES,
                isPersistent: true);
            entryPool.ForcePrewarm();

            return entryPool;
        }
    }
}