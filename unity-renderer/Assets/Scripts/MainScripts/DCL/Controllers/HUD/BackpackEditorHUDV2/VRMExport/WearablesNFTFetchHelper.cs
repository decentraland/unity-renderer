using Cysharp.Threading.Tasks;
using DCL.Helpers;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class WearablesNFTFetchHelper : INFTFetchHelper
    {
        public UniTask<string> GetNFTItems(List<string> wearableUrns) =>
            WearablesFetchingHelper.GetNFTItems(wearableUrns);
    }
}
