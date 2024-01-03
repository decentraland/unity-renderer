using MainScripts.DCL.ServiceProviders.OpenSea.Interfaces;
using MainScripts.DCL.ServiceProviders.OpenSea.Requests;
using System;
using System.Collections;
using UnityEngine;

namespace MainScripts.DCL.ServiceProviders.OpenSea
{
    public class OpenSeaService : IOpenSea
    {
        private readonly KernelConfig kernelConfig;
        private readonly MarketInfo openSeaMarketInfo = new () { name = "OpenSea" };
        private readonly RequestController requestController;

        public OpenSeaService(KernelConfig kernelConfig)
        {
            this.kernelConfig = kernelConfig;
            requestController = new RequestController(this.kernelConfig);
        }

        IEnumerator IOpenSea.FetchNFTsFromOwner(string address, Action<OwnNFTInfo> onSuccess, Action<string> onError)
        {
            var request = requestController.FetchOwnedNFT(address);

            yield return new WaitUntil(() => !request.pending);

            if (request.resolved)
                onSuccess?.Invoke(ResponseToNftOwner(address, request.resolvedValue));
            else
                onError?.Invoke(request.error);
        }

        IEnumerator IOpenSea.FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError)
        {
            RequestBase<OpenSeaNftDto> request = requestController.FetchNFT(assetContractAddress, tokenId);

            yield return new WaitUntil(() => !request.pending);

            if (request.resolved)
                onSuccess?.Invoke(ResponseToNftInfo(request.resolvedValue.nft));
            else
                onError?.Invoke(request.error);
        }

        private OwnNFTInfo ResponseToNftOwner(string ethAddress, OpenSeaManyNftDto response)
        {
            OwnNFTInfo infoInfo = OwnNFTInfo.Default;
            infoInfo.ethAddress = ethAddress;

            foreach (NftDto assetResponse in response.nfts)
                infoInfo.assets.Add(ResponseToNftInfo(assetResponse));

            return infoInfo;
        }

        private NFTInfo ResponseToNftInfo(NftDto nft)
        {
            NFTInfo ret = NFTInfo.Default;
            ret.marketInfo = openSeaMarketInfo;
            ret.name = nft.name;
            ret.description = nft.description;
            ret.thumbnailUrl = nft.image_url; // todo: we only have the original now
            ret.previewImageUrl = nft.image_url; //
            ret.originalImageUrl = nft.image_url; //
            ret.imageUrl = nft.image_url;
            ret.assetLink = nft.opensea_url;
            ret.marketLink = nft.opensea_url;
            ret.tokenId = nft.identifier;
            ret.assetContract.address = nft.contract;
            ret.assetContract.name = nft.contract; // we have no name now
            ret.owners = nft.owners;

            // TODO: Fetch market info? (on-sale value)

            return ret;
        }

        public void Dispose()
        {
            requestController?.Dispose();
        }
    }
}
