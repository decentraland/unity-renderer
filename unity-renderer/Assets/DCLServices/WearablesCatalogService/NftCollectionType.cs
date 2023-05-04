using System;

namespace DCLServices.WearablesCatalogService
{
    [Flags]
    public enum NftCollectionType
    {
        None = 0,
        Base = 1 << 1,
        OnChain = 1 << 2,
        ThirdParty = 1 << 3,
        All = -1,
    }
}
