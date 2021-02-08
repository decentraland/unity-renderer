using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CatalogController : MonoBehaviour
{
    public static bool VERBOSE = false;
    public static CatalogController i { get; private set; }

    private static ItemDictionary itemCatalogValue;

    public static ItemDictionary itemCatalog
    {
        get
        {
            if (itemCatalogValue == null)
            {
                itemCatalogValue = Resources.Load<ItemDictionary>("ItemCatalog");
            }

            return itemCatalogValue;
        }
    }

    private static WearableDictionary wearableCatalogValue;

    public static WearableDictionary wearableCatalog
    {
        get
        {
            if (wearableCatalogValue == null)
            {
                wearableCatalogValue = Resources.Load<WearableDictionary>("WearableCatalog");
            }

            return wearableCatalogValue;
        }
    }

    public void Awake()
    {
        i = this;
    }
 
    public void AddWearableToCatalog(string payload)
    {
        Item item = JsonUtility.FromJson<Item>(payload);

        if (VERBOSE)
            Debug.Log("add wearable: " + payload);

        switch (item.type)
        {
            case "wearable":
                {
                    WearableItem wearableItem = JsonUtility.FromJson<WearableItem>(payload);
                    wearableCatalog.Add(wearableItem.id, wearableItem);
                    break;
                }
            case "item":
                {
                    itemCatalog.Add(item.id, item);
                    break;
                }
            default:
                {
                    Debug.LogError("Bad type in item, will not be added to catalog");
                    break;
                }
        }
    }

    public void AddWearablesToCatalog(string payload)
    {
        Item[] items = JsonUtility.FromJson<Item[]>(payload);

        int count = items.Length;
        for (int i = 0; i < count; ++i)
        {
            itemCatalog.Add(items[i].id, items[i]);
        }
    }

    public void RemoveWearablesFromCatalog(string payload)
    {
        string[] itemIDs = JsonUtility.FromJson<string[]>(payload);

        int count = itemIDs.Length;
        for (int i = 0; i < count; ++i)
        {
            itemCatalog.Remove(itemIDs[i]);
            wearableCatalog.Remove(itemIDs[i]);
        }
    }

    public void ClearWearableCatalog()
    {
        itemCatalog?.Clear();
        wearableCatalog?.Clear();
    }
}
