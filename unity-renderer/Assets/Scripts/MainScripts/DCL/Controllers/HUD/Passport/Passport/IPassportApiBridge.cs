using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Passports
{
    public interface IPassportApiBridge
    {
        UniTask<List<Nft>> QueryNftCollectionsEthereumAsync(string userId, CancellationToken ct);
        UniTask<Nft> QueryNftCollectionEthereumAsync(string userId, string urn, CancellationToken ct);
        UniTask<List<Nft>> QueryNftCollectionsMaticAsync(string userId, CancellationToken ct);
        UniTask<Nft> QueryNftCollectionMaticAsync(string userId, string urn, CancellationToken ct);
    }
}
