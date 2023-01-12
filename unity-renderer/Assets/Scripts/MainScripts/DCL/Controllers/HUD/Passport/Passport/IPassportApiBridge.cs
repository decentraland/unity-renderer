using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace DCL.Social.Passports
{
    public interface IPassportApiBridge
    {
        UniTask<List<Nft>> QueryNftCollectionsEthereum(string userId);
        UniTask<Nft> QueryNftCollectionEthereum(string userId, string urn);
        UniTask<List<Nft>> QueryNftCollectionsMatic(string userId);
        UniTask<Nft> QueryNftCollectionMatic(string userId, string urn);
    }
}
