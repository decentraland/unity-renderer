using Cysharp.Threading.Tasks;
using DCL.Social.Passports;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCl.Social.Passports
{

    public class WebInterfacePassportApiBridge : IPassportApiBridge
    {

        public WebInterfacePassportApiBridge() { }

        public async UniTask<List<Nft>> QueryNftCollectionsEthereum(string userId)
        {
            List<Nft> nftList = new List<Nft>();
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.ETHEREUM)
                     .Then((nfts) => nftList = nfts);

            return nftList;
        }

        public async UniTask<List<Nft>> QueryNftCollectionsMatic(string userId)
        {
            List<Nft> nftList = new List<Nft>();
            await DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, NftCollectionsLayer.MATIC)
                     .Then((nfts) => nftList = nfts);

            return nftList;
        }
    }
}
