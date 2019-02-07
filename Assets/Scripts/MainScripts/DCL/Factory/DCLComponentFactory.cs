using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DCLComponentFactory : ScriptableObject 
    {
        [System.Serializable]
        public class Item
        {
            public CLASS_ID_COMPONENT classId;
            public BaseComponent prefab;
        }

        public Item[] factoryList;

        Dictionary<CLASS_ID_COMPONENT, Item> factoryDict;

        public void EnsueFactoryDictionary()
        {
            if (factoryDict == null)
            {
                factoryDict = new Dictionary<CLASS_ID_COMPONENT, Item>(new Utils.FastEnumIntEqualityComparer<CLASS_ID_COMPONENT>());
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

        public ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
        {
            EnsueFactoryDictionary();

            if (!factoryDict.ContainsKey(id))
            {
                Debug.LogWarning("Class " + id + " can't be instantiated because the field doesn't exists!");
                return default(ItemType);
            }

            if (factoryDict[id].prefab == null)
            {
                Debug.LogError("Prefab for class " + id + " is null!");
                return default(ItemType);
            }

            return Instantiate(factoryDict[id].prefab).GetComponent<ItemType>();
        }

    }
}
