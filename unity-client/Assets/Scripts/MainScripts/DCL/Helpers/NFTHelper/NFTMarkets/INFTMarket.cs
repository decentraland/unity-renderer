using System;
using System.Collections;

namespace DCL.Helpers.NFT.Markets
{
    public interface INFTMarket
    {
        IEnumerator FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError);
    }
}