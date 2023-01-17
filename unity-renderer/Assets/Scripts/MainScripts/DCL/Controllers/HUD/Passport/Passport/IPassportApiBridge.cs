using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace DCL.Social.Passports
{
    public interface IPassportApiBridge
    {
        UniTask<List<Nft>> QueryNftCollectionsEthereum(string userId);
        UniTask<List<Nft>> QueryNftCollectionsMatic(string userId);
        void OpenURL(string url);
        void SendBlockPlayer(string playedId);
        void SendUnblockPlayer(string playedId);
        void SendReportPlayer(string currentPlayerId, string name);
    }
}
