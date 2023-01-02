using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ItemToggleContainer : MonoBehaviour
{
    public Transform itemContainer;

    [SerializeField] private ItemToggle itemPrefab;
    [SerializeField] private NFTSkinFactory skinFactory;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private GridLayoutGroup layout;

    private readonly List<ItemToggle> items = new ();

    private int maxItems;

    public void SetLayoutConstraint(bool isFlexible)
    {
        layout.constraint = isFlexible ? GridLayoutGroup.Constraint.Flexible : GridLayoutGroup.Constraint.FixedRowCount;
        layout.constraintCount = 3;
    }

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

    public async UniTask<ItemToggle> LoadItemAsync(int index, WearableSettings wearableSettings, CancellationToken token)
    {
        var item = wearableSettings.Item;

        ItemToggle newToggle;

        if (index < items.Count)
            newToggle = items[index];
        else
        {
            await UniTask.Yield(token);

            token.ThrowIfCancellationRequested();

            newToggle = Instantiate(itemPrefab, itemContainer);
            items.Add(newToggle);
            newToggle.transform.SetAsLastSibling();
        }

        token.ThrowIfCancellationRequested();

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
