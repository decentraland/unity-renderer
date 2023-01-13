using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Social.Passports;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCl.Social.Passports
{

    public class WebInterfacePassportApiBridge : IPassportApiBridge
    {

        public WebInterfacePassportApiBridge() { }

        public async UniTask<List<Nft>> QueryNftCollectionsEthereumAsync(string userId, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.ETHEREUM)
                    .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList;
        }

        public async UniTask<Nft> QueryNftCollectionEthereumAsync(string userId, string urn, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollectionsByUrn(userId, urn, NftCollectionsLayer.ETHEREUM)
                    .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList?.Count > 0 ? nftList[0] : null;
        }

        public async UniTask<List<Nft>> QueryNftCollectionsMaticAsync(string userId, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.MATIC)
                         .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList;
        }

        public async UniTask<Nft> QueryNftCollectionMaticAsync(string userId, string urn, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollectionsByUrn(userId, urn, NftCollectionsLayer.MATIC)
                         .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList?.Count > 0 ? nftList[0] : null;
        }
    }
}
