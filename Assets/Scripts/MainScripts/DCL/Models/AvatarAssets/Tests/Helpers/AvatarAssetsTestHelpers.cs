using System.Collections.Generic;
using DCL.Helpers;
using System.Linq;
using UnityEngine;

public static class AvatarAssetsTestHelpers
{
    public static WearableDictionary CreateTestCatalogLocal()
    {
        List<WearableItemDummy> wearables = Object.Instantiate(Resources.Load<WearableItemDummyListVariable>("TestCatalogArrayLocalAssets")).list;
        foreach (var wearableItem in wearables)
        {
            wearableItem.baseUrl = Utils.GetTestsAssetsPath() + "/Avatar/Assets/";

            foreach (var rep in wearableItem.representations)
            {
                rep.contents = rep.contents.Select((x) =>
                {
                    x.hash = x.file;
                    return x;
                }).ToArray();
            }

            wearableItem.thumbnail = "";
        }

        CatalogController.wearableCatalog.Clear();
        CatalogController.wearableCatalog.Add(wearables.Select(x => new KeyValuePair<string, WearableItem>(x.id, x)).ToArray());

        return CatalogController.wearableCatalog;
    }
}