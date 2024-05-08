using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface INFTFetchHelper
    {
        UniTask<string> GetNFTItems(List<string> wearableUrns);
    }
}
