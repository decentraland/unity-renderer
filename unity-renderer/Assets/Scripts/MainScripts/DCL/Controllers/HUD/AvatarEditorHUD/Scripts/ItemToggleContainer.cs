using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToggleContainer : MonoBehaviour
{
    public Transform itemContainer;

    [SerializeField] private ItemToggle itemPrefab;
    [SerializeField] private NFTSkinFactory skinFactory;
    [SerializeField] private RectTransform rectTransform;

    private readonly List<ItemToggle> items = new ();

    private int maxItems;

    public void Rebuild(int newMaxItems)
    {
        if (maxItems == newMaxItems) return;

        maxItems = newMaxItems;

        for (var i = 0; i < items.Count; i++)
        {
            var itemToggle = items[i];
            itemToggle.gameObject.SetActive(i < maxItems);
            itemToggle.transform.SetAsLastSibling();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public async UniTask<ItemToggle> LoadItemAsync(int index, WearableSettings wearableSettings)
    {
        var item = wearableSettings.Item;

        ItemToggle newToggle;

        if (index < items.Count)
            newToggle = items[index];
        else
        {
            await UniTask.Yield();

            newToggle = Instantiate(itemPrefab, itemContainer);
            items.Add(newToggle);
            newToggle.transform.SetAsLastSibling();
        }

        newToggle.Initialize(item, false, wearableSettings.Amount, skinFactory.GetSkinForRarity(wearableSettings.Item.rarity));
        newToggle.SetHideOtherWerablesToastStrategy(wearableSettings.HideOtherWearablesToastStrategy);
        newToggle.SetBodyShapeCompatibilityStrategy(wearableSettings.IncompatibleWearableToastStrategy);
        newToggle.SetReplaceOtherWearablesToastStrategy(wearableSettings.ReplaceOtherWearablesToastStrategy);

        return newToggle;
    }

    public void HideItem(int i)
    {
        if (i < items.Count)
            items[i].Hide();
    }
}
