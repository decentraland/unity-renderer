using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.EnvironmentProvider;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class WearablesNFTFetchHelper : INFTFetchHelper
    {
        public UniTask<string> GetNFTItems(List<string> wearableUrns, IEnvironmentProviderService environmentProvider) =>
            WearablesFetchingHelper.GetNFTItems(wearableUrns, environmentProvider);
    }
}
