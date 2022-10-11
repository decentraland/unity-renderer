using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class ItemToggleContainer : MonoBehaviour
{
    public Transform itemContainer;
    [SerializeField] private ItemToggle itemPrefab;
    [SerializeField] private NFTSkinFactory skinFactory;
    [SerializeField] private RectTransform rectTransform;

    private List<ItemToggle> items = new List<ItemToggle>();
    private int maxItems;

    public void Setup(int newMaxItems)
    {
        if (maxItems == newMaxItems) return;
        maxItems = newMaxItems;
        
        var diff = maxItems - items.Count;

        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                var newItemToggle = Instantiate(itemPrefab, itemContainer);
                items.Add(newItemToggle);
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            var itemToggle = items[i];
            itemToggle.gameObject.SetActive(i < maxItems);
            itemToggle.transform.SetAsLastSibling();
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public ItemToggle LoadItem(int index, WearableSettings wearableSettings)
    {
        var item = wearableSettings.Item;
        var newToggle = items[index];
        
        newToggle.Initialize(item, false, wearableSettings.Amount, skinFactory.GetSkinForRarity(wearableSettings.Item.rarity));
        newToggle.SetHideOtherWerablesToastStrategy(wearableSettings.HideOtherWearablesToastStrategy);
        newToggle.SetBodyShapeCompatibilityStrategy(wearableSettings.IncompatibleWearableToastStrategy);
        newToggle.SetReplaceOtherWearablesToastStrategy(wearableSettings.ReplaceOtherWearablesToastStrategy);

        return newToggle;
    }
    public void HideItem(int i)
    {
        var newToggle = items[i];
        newToggle.Hide();
    }
}