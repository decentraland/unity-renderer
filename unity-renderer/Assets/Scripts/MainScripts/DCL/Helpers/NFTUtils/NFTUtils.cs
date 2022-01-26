using System;
using System.Collections;
using DCL.Helpers.NFT.Markets;

namespace DCL.Helpers.NFT
{
    public static class NFTUtils
    {
        /// <summary>
        /// Fetch NFT from owner
        /// </summary>
        /// <param name="address">owner address</param>
        /// <param name="onSuccess">success callback</param>
        /// <param name="onError">error callback</param>
        /// <returns>IEnumerator</returns>
        public static IEnumerator FetchNFTsFromOwner(string address, Action<NFTOwner> onSuccess, Action<string> onError)
        {
            INFTMarket selectedMarket = null;
            yield return GetMarket(address, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.FetchNFTsFromOwner(address, onSuccess, onError);
            }
            else
            {
                onError?.Invoke($"Market not found for asset {address}");
            }
        }

        /// <summary>
        /// Fetch NFT. Request is added to a batch of requests to reduce the amount of request to the api.
        /// NOTE: for ERC1155 result does not contain the array of owners and sell price for this asset
        /// </summary>
        /// <param name="assetContractAddress">asset contract address</param>
        /// <param name="tokenId">asset token id</param>
        /// <param name="onSuccess">success callback</param>
        /// <param name="onError">error callback</param>
        /// <returns>IEnumerator</returns>
        public static IEnumerator FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
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

        /// <summary>
        /// Fetch NFT. Request is fetch directly to the api instead of batched with other requests in a single query.
        /// Please try to use `FetchNFTInfo` if ownership info is not relevant for your use case.
        /// NOTE: result it does contain the array of owners for ERC1155 NFTs
        /// </summary>
        /// <param name="assetContractAddress">asset contract address</param>
        /// <param name="tokenId">asset token id</param>
        /// <param name="onSuccess">success callback</param>
        /// <param name="onError">error callback</param>
        /// <returns>IEnumerator</returns>
        public static IEnumerator FetchNFTInfoSingleAsset(string assetContractAddress, string tokenId, Action<NFTInfoSingleAsset> onSuccess, Action<string> onError)
        {
            INFTMarket selectedMarket = null;
            yield return GetMarket(assetContractAddress, tokenId, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.FetchNFTInfoSingleAsset(assetContractAddress, tokenId, onSuccess, onError);
            }
            else
            {
                onError?.Invoke($"Market not found for asset {assetContractAddress}/{tokenId}");
            }
        }

        // NOTE: this method doesn't make sense now, but it will when support for other market is added
        public static IEnumerator GetMarket(string assetContractAddress, string tokenId, Action<INFTMarket> onSuccess)
        {
            IServiceProviders serviceProviders = Environment.i.platform.serviceProviders;
            INFTMarket openSea = null;

            if ( serviceProviders != null )
                openSea = serviceProviders.openSea;

            onSuccess?.Invoke(openSea);
            yield break;
        }

        public static IEnumerator GetMarket(string assetContractAddress, Action<INFTMarket> onSuccess)
        {
            IServiceProviders serviceProviders = Environment.i.platform.serviceProviders;
            INFTMarket openSea = null;

            if ( serviceProviders != null )
                openSea = serviceProviders.openSea;

            onSuccess?.Invoke(openSea);
            yield break;
        }
    }
}