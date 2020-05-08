using System.Collections.Generic;
using UnityEngine;

public class NotificationFactory : ScriptableObject
{
    public enum Type
    {
        GENERIC,
        SCRIPTING_ERROR,
        COMMS_ERROR,
        AIRDROPPING,
        GENERIC_WITHOUT_BUTTON,
        CUSTOM
    }

    [System.Serializable]
    public class Item
    {
        public Type type;
        public Notification prefab;
    }

    public Item[] factoryList;

    private Dictionary<Type, Item> factoryDict;

    public void EnsueFactoryDictionary()
    {
        if (factoryDict == null)
        {
            factoryDict = new Dictionary<Type, Item>();

            for (int i = 0; i < factoryList.Length; i++)
            {
                Item item = factoryList[i];

                if (!factoryDict.ContainsKey(item.type))
                {
                    factoryDict.Add(item.type, item);
                }
            }
        }
    }

    public Notification CreateNotificationFromType(Type type, Transform parent = null)
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
