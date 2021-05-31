using System;
using System.Collections.Generic;
using System.Linq;
using DCL;

[Serializable]
public class WearableItemDummy : WearableItem
{
    protected override ContentProvider CreateContentProvider(string baseUrl, MappingPair[] contents)
    {
        return new ContentProvider_Dummy
        {
            baseUrl = baseUrl,
            contents = contents.Select(mapping => new ContentServerUtils.MappingPair() { file = mapping.key, hash = mapping.hash }).ToList()
        };
    }

    public WearableItemDummy Clone() { return (WearableItemDummy)MemberwiseClone(); }
}