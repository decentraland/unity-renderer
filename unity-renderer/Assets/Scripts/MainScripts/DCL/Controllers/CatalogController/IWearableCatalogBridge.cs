using System.Collections.Generic;
using DCL.Helpers;

public interface IWearableCatalogBridge
{
    Promise<WearableItem[]> RequestOwnedWearables(string userId);
    bool IsValidWearable(string wearableId);
    void RemoveWearablesInUse(List<string> loadedWearables);
}