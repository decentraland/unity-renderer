using System;

/// <summary>
/// This Class is a container of data to be used for the sole purpose of loading new ItemToggles with their respective data
/// </summary>
public class WearableSettings
{
    public WearableItem Item { get; }
    public string CollectionName { get; }
    public int Amount { get; }
    
    public bool isLoading { get; set; }
    public Func<WearableItem, bool> HideOtherWearablesToastStrategy { get; }
    public Func<WearableItem, bool> ReplaceOtherWearablesToastStrategy { get; }
    public WearableSettings(WearableItem item, string collectionName, int amount, Func<WearableItem, bool> hideOtherWearablesToastStrategy, Func<WearableItem, bool> replaceOtherWearablesToastStrategy)
    {
        Item = item;
        CollectionName = collectionName;
        Amount = amount;
        HideOtherWearablesToastStrategy = hideOtherWearablesToastStrategy;
        ReplaceOtherWearablesToastStrategy = replaceOtherWearablesToastStrategy;
    }

    public override string ToString() { return Item.id; }
}