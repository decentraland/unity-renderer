using System;
using System.Collections.Generic;
using DCL;

[Serializable]
public class WearableItemDummy : WearableItem
{
    protected override ContentProvider CreateContentProvider(string baseUrl, List<ContentServerUtils.MappingPair> contents)
    {
        return new ContentProvider_Dummy
        {
            baseUrl = baseUrl,
            contents = contents
        };
    }

    public WearableItemDummy Clone()
    {
        return (WearableItemDummy)MemberwiseClone();
    }
}