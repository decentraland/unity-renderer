using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Passports
{
    public interface IPassportApiBridge
    {
        void OpenURL(string url);
        void SendBlockPlayer(string playedId);
        void SendUnblockPlayer(string playedId);
        void SendReportPlayer(string currentPlayerId, string name);
        UniTask<List<Nft>> QueryNftCollectionsAsync(string userId, NftCollectionsLayer layer, CancellationToken ct);
        UniTask<Nft> QueryNftCollectionAsync(string userId, string urn, NftCollectionsLayer layer, CancellationToken ct);
    }
}
