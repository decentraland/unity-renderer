using Cysharp.Threading.Tasks;
using DCL.Social.Passports;
using System.Collections.Generic;

namespace DCl.Social.Passports
{

    public class WebInterfacePassportApiBridge : IPassportApiBridge
    {

        public WebInterfacePassportApiBridge() { }

        public async UniTask<List<Nft>> QueryNftCollectionsEthereum(string userId)
        {
            List<Nft> nftList = null;
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.ETHEREUM)
                     .Then((nfts) => nftList = nfts);

            return nftList;
        }

        public async UniTask<Nft> QueryNftCollectionEthereum(string userId, string urn)
        {
            List<Nft> nftList = null;
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollectionsByUrn(userId, urn, NftCollectionsLayer.ETHEREUM)
                     .Then((nfts) => nftList = nfts);

            return nftList?.Count > 0 ? nftList[0] : null;
        }

        public async UniTask<List<Nft>> QueryNftCollectionsMatic(string userId)
        {
            List<Nft> nftList = null;
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.MATIC)
                     .Then((nfts) => nftList = nfts);

            return nftList;
        }

        public async UniTask<Nft> QueryNftCollectionMatic(string userId, string urn)
        {
            List<Nft> nftList = null;
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollectionsByUrn(userId, urn, NftCollectionsLayer.MATIC)
                     .Then((nfts) => nftList = nfts);

            return nftList?.Count > 0 ? nftList[0] : null;
        }
    }
}
