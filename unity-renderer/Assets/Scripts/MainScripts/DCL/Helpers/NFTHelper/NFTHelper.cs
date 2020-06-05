using System;
using System.Collections;
using DCL.Helpers.NFT.Markets;

namespace DCL.Helpers.NFT
{
    public static class NFTHelper
    {
        static INFTMarket market = new OpenSea();

        static public IEnumerator FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            INFTMarket selectedMarket = null;
            yield return GetMarket(assetContractAddress, tokenId, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.FetchNFTInfo(assetContractAddress, tokenId, onSuccess, onError);
            }
            else
            {
                onError?.Invoke($"Market not found for asset {assetContractAddress}/{tokenId}");
            }
        }

        // NOTE: this method doesn't make sense now, but it will when support for other market is added
        static public IEnumerator GetMarket(string assetContractAddress, string tokenId, Action<INFTMarket> onSuccess)
        {
            onSuccess?.Invoke(market);
            yield break;
        }
    }
}