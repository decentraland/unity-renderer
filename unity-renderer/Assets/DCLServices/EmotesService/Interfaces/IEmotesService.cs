using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCL.Emotes
{
    public interface IEmotesService : IService
    {
        UniTask<IEmoteReference> RequestEmote(string bodyShapeId, string emoteId, CancellationToken cancellationToken);
    }
}
