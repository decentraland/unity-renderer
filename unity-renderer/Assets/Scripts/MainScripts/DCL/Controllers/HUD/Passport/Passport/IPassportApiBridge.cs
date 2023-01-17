using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Passports
{
    public interface IPassportApiBridge
    {
        UniTask<List<Nft>> QueryNftCollectionsAsync(string userId, NftCollectionsLayer layer, CancellationToken ct);
        UniTask<Nft> QueryNftCollectionAsync(string userId, string urn, NftCollectionsLayer layer, CancellationToken ct);
    }
}
