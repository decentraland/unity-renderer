using Cysharp.Threading.Tasks;
using DCL.Interface;
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

        public async UniTask<List<Nft>> QueryNftCollectionsAsync(string userId, NftCollectionsLayer layer, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userId, layer)
                    .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList;
        }

        public async UniTask<Nft> QueryNftCollectionAsync(string userId, string urn, NftCollectionsLayer layer, CancellationToken ct)
        {
            List<Nft> nftList = null;
            Promise<List<Nft>> promise = null;

            try
            {
                promise = DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollectionsByUrn(userId, urn, layer)
                    .Then((nfts) => nftList = nfts);

                await promise.WithCancellation(ct);
            }
            catch (OperationCanceledException e)
            {
                promise?.Reject("Canceled");
            }

            return nftList?.Count > 0 ? nftList[0] : null;
        }

        public void OpenURL(string url)
        {
            WebInterface.OpenURL(url);
        }

        public void SendBlockPlayer(string playerId)
        {
            WebInterface.SendBlockPlayer(playerId);
        }

        public void SendUnblockPlayer(string playerId)
        {
            WebInterface.SendUnblockPlayer(playerId);
        }

        public void SendReportPlayer(string currentPlayerId, string name)
        {
            WebInterface.SendReportPlayer(currentPlayerId, name);
        }
    }
}
