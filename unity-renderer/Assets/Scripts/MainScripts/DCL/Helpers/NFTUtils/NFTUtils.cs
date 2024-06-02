using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using System;
using System.Collections;

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
        public static IEnumerator FetchNFTsFromOwner(string address, Action<OwnNFTInfo> onSuccess, Action<string> onError)
        {
            IOpenSea selectedMarket = null;
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
        /// <param name="chain">chain network id</param>
        /// <param name="assetContractAddress">asset contract address</param>
        /// <param name="tokenId">asset token id</param>
        /// <param name="onSuccess">success callback</param>
        /// <param name="onError">error callback</param>
        /// <returns>IEnumerator</returns>
        public static IEnumerator FetchNFTInfo(string chain, string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            IOpenSea selectedMarket = null;
            yield return GetMarket(chain, assetContractAddress, tokenId, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.FetchNFTInfo(chain, assetContractAddress, tokenId, onSuccess, onError);
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
        /// <param name="chain">chain network id</param>
        /// <param name="assetContractAddress">asset contract address</param>
        /// <param name="tokenId">asset token id</param>
        /// <param name="onSuccess">success callback</param>
        /// <param name="onError">error callback</param>
        /// <returns>IEnumerator</returns>
        public static IEnumerator FetchNFTInfoSingleAsset(string chain, string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            IOpenSea selectedMarket = null;
            yield return GetMarket(chain, assetContractAddress, tokenId, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.FetchNFTInfo(chain, assetContractAddress, tokenId, onSuccess, onError);
            }
            else
            {
                onError?.Invoke($"Market not found for asset {assetContractAddress}/{tokenId}");
            }
        }

        // NOTE: this method doesn't make sense now, but it will when support for other market is added
        public static IEnumerator GetMarket(string chain, string assetContractAddress, string tokenId, Action<IOpenSea> onSuccess)
        {
            IServiceProviders serviceProviders = Environment.i.platform.serviceProviders;
            IOpenSea openSea = null;

            if (serviceProviders != null)
                openSea = serviceProviders.openSea;

            onSuccess?.Invoke(openSea);
            yield break;
        }

        public static IEnumerator GetMarket(string assetContractAddress, Action<IOpenSea> onSuccess)
        {
            IServiceProviders serviceProviders = Environment.i.platform.serviceProviders;
            IOpenSea openSea = null;

            if (serviceProviders != null)
                openSea = serviceProviders.openSea;

            onSuccess?.Invoke(openSea);
            yield break;
        }

        /// <summary>
        /// Parses URNs with the format "urn:decentraland:{CHAIN}:{CONTRACT_STANDARD}:{CONTRACT_ADDRESS}:{TOKEN_ID}"
        /// and if successful stores the contract address and token ID in their corresponding reference parameters.
        /// example: urn:decentraland:ethereum:erc721:0x00...000:123
        /// </summary>
        /// <param name="urn">the raw URN of the NFT asset</param>
        /// <param name="chain">if successful the chain network id is stored in this reference parameter</param>
        /// <param name="contractAddress">if successful the contract address is stored in this reference parameter</param>
        /// <param name="tokenId">if successful the token id is stored in this reference parameter</param>
        /// <returns>bool</returns>
        // TODO: update this method when support for wearables/emotes/etc urn is added
        public static bool TryParseUrn(string urn, out string chain, out string contractAddress, out string tokenId)
        {
            const char SEPARATOR = ':';
            const string DCL_URN_ID = "urn:decentraland";
            const string COLLECTIONS_THIRDPARTY = "collections-thirdparty";

            contractAddress = string.Empty;
            tokenId = string.Empty;
            chain = string.Empty;

            try
            {
                var urnSpan = urn.AsSpan();

                // 1: "urn:decentraland"
                if (!urnSpan.Slice(0, DCL_URN_ID.Length).Equals(DCL_URN_ID, StringComparison.Ordinal))
                    return false;
                urnSpan = urnSpan.Slice(DCL_URN_ID.Length + 1);

                // 2: chain/network
                var chainSpan = urnSpan.Slice(0, urnSpan.IndexOf(SEPARATOR));
                urnSpan = urnSpan.Slice(chainSpan.Length + 1);

                // 3: contract standard
                var contractStandardSpan = urnSpan.Slice(0, urnSpan.IndexOf(SEPARATOR));
                urnSpan = urnSpan.Slice(contractStandardSpan.Length + 1);

                // Check if the URN contains "collections-thirdparty"
                if (contractStandardSpan.ToString().Equals(COLLECTIONS_THIRDPARTY, StringComparison.Ordinal))
                {
                    // 4: contract address
                    var contractAddressSpan = urnSpan;
                    contractAddress = contractAddressSpan.ToString();
                    chain = chainSpan.ToString();

                    return true;
                }
                else
                {
                    // 4: contract address
                    var contractAddressSpan = urnSpan.Slice(0, urnSpan.IndexOf(SEPARATOR));
                    urnSpan = urnSpan.Slice(contractAddressSpan.Length + 1);

                    // 5: token id
                    var tokenIdSpan = urnSpan;
                    contractAddress = contractAddressSpan.ToString();
                    tokenId = tokenIdSpan.ToString();
                    chain = chainSpan.ToString();

                    return true;
                }
            }
            catch (Exception e)
            { // ignored
            }

            return false;
        }
    }
}
