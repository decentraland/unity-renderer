using System;
using DCL.Components;
using DCL.Models;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace DCL
{
    public interface IPoolableComponentFactory
    {
        void EnsureFactoryDictionary();
        CLASS_ID_COMPONENT GetIdForType<T>() where T : Component;
        void PrewarmPools();

        ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
            where ItemType : IPoolableObjectContainer;
    }

    public class PoolableComponentFactory : ScriptableObject, IPoolableComponentFactory
    {
        [Serializable]
        public class Item
        {
            public CLASS_ID_COMPONENT classId;
            public Component prefab;

            [Header("Pool Options")]
            public bool usePool;
            public int prewarmCount;
        }

        public Item[] factoryList;

        private Dictionary<CLASS_ID_COMPONENT, Item> factoryDict;

        public void EnsureFactoryDictionary()
        {
            if (factoryDict == null)
            {
                factoryDict = new Dictionary<CLASS_ID_COMPONENT, Item>();

                for (int i = 0; i < factoryList.Length; i++)
                {
                    Item item = factoryList[i];

                    if (!factoryDict.ContainsKey(item.classId))
                    {
                        factoryDict.Add(item.classId, item);
                    }
                }
            }
        }

        public CLASS_ID_COMPONENT GetIdForType<T>() where T : Component
        {
            for (int i = 0; i < factoryList.Length; i++)
            {
                Item item = factoryList[i];

                if (item != null && item.prefab != null && item.prefab.GetComponent<T>() != null)
                {
                    return item.classId;
                }
            }

            return CLASS_ID_COMPONENT.NONE;
        }

        public void PrewarmPools()
        {
            for (int i = 0; i < factoryList.Length; i++)
            {
                Item item = factoryList[i];

                if (item.usePool)
                {
                    EnsurePoolForItem(item);

                    bool forceActivate = item.classId == CLASS_ID_COMPONENT.TEXT_SHAPE;
                    GetPoolForItem(item).ForcePrewarm(forceActivate);
                }
            }
        }

        private static Pool GetPoolForItem(Item item) =>
            PoolManager.i.GetPool(GetIdForPool(item));

        private static object GetIdForPool(Item item)
        {
#if UNITY_EDITOR
            return item.classId + "_POOL";
#else
            return item.classId;
#endif
        }

        ProfilerMarker m_AddPool = new ("VV.Factory.AddPool");

        private void EnsurePoolForItem(Item item)
        {
            Pool pool = GetPoolForItem(item);

            if (pool != null)
                return;

            m_AddPool.Begin(item.classId.ToString());
            GameObject original = Instantiate(item.prefab.gameObject);
            pool = PoolManager.i.AddPool(GetIdForPool(item), original, maxPrewarmCount: item.prewarmCount, isPersistent: true);
            pool.useLifecycleHandlers = true;
            m_AddPool.End();
        }

        ProfilerMarker m_CreateItemFromId = new ("VV.Factory.CreateItemFromId");
        ProfilerMarker m_CreateItemFromIdUsePool = new ("VV.Factory.CreateItemFromId.UsePool");
        ProfilerMarker m_CreateItemFromIdInstantiate = new ("VV.Factory.CreateItemFromId.Instantiate");

        public ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
            where ItemType : IPoolableObjectContainer
        {
            m_CreateItemFromId.Begin(id.ToString());
            EnsureFactoryDictionary();

            if (!factoryDict.ContainsKey(id))
            {
#if UNITY_EDITOR
                Debug.LogError("Class " + id + " can't be instantiated because the field doesn't exist!");
#endif
                m_CreateItemFromId.End();
                return default(ItemType);
            }

            var factoryItem = factoryDict[id];

            if (factoryItem.prefab == null)
            {
                Debug.LogError("Prefab for class " + id + " is null!");
                m_CreateItemFromId.End();
                return default(ItemType);
            }

            GameObject instancedGo;
            PoolableObject poolableObject = null;

            if (factoryItem.usePool)
            {
                m_CreateItemFromIdUsePool.Begin(id.ToString());

                EnsurePoolForItem(factoryItem);
                poolableObject = GetPoolForItem(factoryItem).Get();
                instancedGo = poolableObject.gameObject;
                m_CreateItemFromIdUsePool.End();
            }
            else
            {
                m_CreateItemFromIdInstantiate.Begin(id.ToString());
                instancedGo = Instantiate(factoryItem.prefab.gameObject);
                m_CreateItemFromIdInstantiate.End();
            }

            ItemType item = default;

            if(instancedGo != null)
            {
                item = instancedGo.GetComponent<ItemType>();
                item.poolableObject = poolableObject;
            }

            m_CreateItemFromId.End();
            return item;
        }
    }
}
