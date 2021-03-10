using System;
using DCL.Components;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IRuntimeComponentFactory
    {
        void EnsureFactoryDictionary();
        CLASS_ID_COMPONENT GetIdForType<T>() where T : Component;
        void PrewarmPools();

        ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
            where ItemType : IPoolableObjectContainer;
    }

    public class RuntimeComponentFactory : ScriptableObject, IRuntimeComponentFactory
    {
        [System.Serializable]
        public class Item
        {
            public CLASS_ID_COMPONENT classId;
            public Component prefab;

            [Header("Pool Options")]
            public bool usePool;

            public int prewarmCount;
        }

        public Item[] factoryList;

        Dictionary<CLASS_ID_COMPONENT, Item> factoryDict;

        public static RuntimeComponentFactory Create()
        {
            return Resources.Load("RuntimeComponentFactory") as RuntimeComponentFactory;
        }

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
                    GetPoolForItem(item).ForcePrewarm();
                }
            }
        }

        private Pool GetPoolForItem(Item item)
        {
            return PoolManager.i.GetPool(GetIdForPool(item));
        }

        private object GetIdForPool(Item item)
        {
#if UNITY_EDITOR
            return item.classId.ToString() + "_POOL";
#else
            return item.classId;
#endif
        }

        private void EnsurePoolForItem(Item item)
        {
            Pool pool = GetPoolForItem(item);

            if (pool != null)
                return;

            GameObject original = Instantiate(item.prefab.gameObject);
            pool = PoolManager.i.AddPool(GetIdForPool(item), original, maxPrewarmCount: item.prewarmCount, isPersistent: true);
            pool.useLifecycleHandlers = true;
        }

        public ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
            where ItemType : IPoolableObjectContainer
        {
            EnsureFactoryDictionary();

            if (!factoryDict.ContainsKey(id))
            {
#if UNITY_EDITOR
                Debug.LogError("Class " + id + " can't be instantiated because the field doesn't exist!");
#endif
                return default(ItemType);
            }

            var factoryItem = factoryDict[id];

            if (factoryItem.prefab == null)
            {
                Debug.LogError("Prefab for class " + id + " is null!");
                return default(ItemType);
            }

            GameObject instancedGo;
            PoolableObject poolableObject = null;

            if (factoryItem.usePool)
            {
                EnsurePoolForItem(factoryItem);
                poolableObject = GetPoolForItem(factoryItem).Get();
                instancedGo = poolableObject.gameObject;
            }
            else
            {
                instancedGo = Instantiate(factoryItem.prefab.gameObject);
            }

            ItemType item = instancedGo.GetComponent<ItemType>();
            item.poolableObject = poolableObject;

            return item;
        }
    }
}