using System.Collections.Generic;
using UnityEngine;

public class ItemToggleFactory : ScriptableObject
{
    [System.Serializable]
    public class ItemToggleItem
    {
        public string type;
        public ItemToggle prefab;
    }

    public ItemToggleItem[] factoryList;

    private Dictionary<string, ItemToggleItem> factoryDict;

    public void EnsueFactoryDictionary()
    {
        if (factoryDict == null)
        {
            factoryDict = new Dictionary<string, ItemToggleItem>();

            for (int i = 0; i < factoryList.Length; i++)
            {
                ItemToggleItem itemToggleItem = factoryList[i];

                if (!factoryDict.ContainsKey(itemToggleItem.type))
                {
                    factoryDict.Add(itemToggleItem.type, itemToggleItem);
                }
            }
        }
    }

    public ItemToggle CreateItemToggleFromType(string type, Transform parent = null)
    {
        EnsueFactoryDictionary();

        if (!factoryDict.ContainsKey(type))
        {
#if UNITY_EDITOR
            Debug.LogError("Notification of type " + type + " can't be instantiated because it does not exist in the factory!");
#endif
            return null;
        }

        if (factoryDict[type].prefab == null)
        {
            Debug.LogError("Prefab for type " + type + " is null!");
            return null;
        }

        return parent == null ? Instantiate(factoryDict[type].prefab) : Instantiate(factoryDict[type].prefab, parent);
    }
}