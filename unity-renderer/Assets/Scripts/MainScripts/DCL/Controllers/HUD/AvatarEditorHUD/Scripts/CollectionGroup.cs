using TMPro;
using UnityEngine;

public class CollectionGroup : MonoBehaviour
{
    public Transform itemContainer;
    [SerializeField] private ItemToggle itemPrefab;
    [SerializeField] private ItemToggle[] items;
    [SerializeField] private NFTSkinFactory skinFactory;
    
    public ItemToggle LoadItem(int index, WearableSettings wearableSettings)
    {
        var item = wearableSettings.Item;
        var newToggle = items[index];

        if (item.IsCollectible())
        {
            newToggle.transform.SetAsFirstSibling();
        }

        newToggle.Initialize(item, false, wearableSettings.Amount, skinFactory.GetSkinForRarity(wearableSettings.Item.rarity));
        newToggle.SetHideOtherWerablesToastStrategy(wearableSettings.HideOtherWearablesToastStrategy);
        newToggle.SetReplaceOtherWearablesToastStrategy(wearableSettings.ReplaceOtherWearablesToastStrategy);
        
        return newToggle;
    }
    public void HideItem(int i)
    {
        var newToggle = items[i];
        newToggle.Hide();
    }
}