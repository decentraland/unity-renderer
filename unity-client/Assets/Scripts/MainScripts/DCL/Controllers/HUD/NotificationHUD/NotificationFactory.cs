using System.Collections.Generic;
using UnityEngine;

public class NotificationFactory : ScriptableObject
{
    [System.Serializable]
    public class NotificationFactoryItem
    {
        public NotificationModel.NotificationType type;
        public Notification prefab;
    }

    public NotificationFactoryItem[] factoryList;

    private Dictionary<NotificationModel.NotificationType, NotificationFactoryItem> factoryDict;

    public void EnsueFactoryDictionary()
    {
        if (factoryDict == null)
        {
            factoryDict = new Dictionary<NotificationModel.NotificationType, NotificationFactoryItem>();

            for (int i = 0; i < factoryList.Length; i++)
            {
                NotificationFactoryItem item = factoryList[i];

                if (!factoryDict.ContainsKey(item.type))
                {
                    factoryDict.Add(item.type, item);
                }
            }
        }
    }

    public Notification CreateNotificationFromType(NotificationModel.NotificationType type, Transform parent = null)
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