using System.Collections.Generic;
using DCL.Helpers;

public class WearablesCatalogControllerBridge : IWearableCatalogBridge
{
    public Promise<WearableItem[]> RequestOwnedWearables(string userId)
    {
        return CatalogController.RequestOwnedWearables(userId);
    }

    public bool IsValidWearable(string wearableId)
    {
        if (!CatalogController.wearableCatalog.TryGetValue(wearableId, out var wearable))
            return false;
        return wearable != null;
    }

    public void RemoveWearablesInUse(List<string> loadedWearables)
    {
        CatalogController.RemoveWearablesInUse(loadedWearables);
    }
}