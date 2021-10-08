using System;
using System.Collections;

namespace DCL.Helpers.NFT.Markets
{
    public interface INFTMarket : IDisposable
    {
        IEnumerator FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError);
        IEnumerator FetchNFTsFromOwner(string assetContractAddress, Action<NFTOwner> onSuccess, Action<string> onError);
        IEnumerator FetchNFTInfoSingleAsset(string assetContractAddress, string tokenId, Action<NFTInfoSingleAsset> onSuccess, Action<string> onError);
    }
}