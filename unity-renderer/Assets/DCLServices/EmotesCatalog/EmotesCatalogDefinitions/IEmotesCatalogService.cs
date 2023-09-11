using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using System.Collections.Generic;
using System.Threading;

public interface IEmotesCatalogService : IService
{
    bool TryGetLoadedEmote(string id, out WearableItem emote);

    UniTask<IReadOnlyList<WearableItem>> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default);
    UniTask<IReadOnlyList<WearableItem>> RequestEmoteCollectionAsync(IEnumerable<string> collectionIds, CancellationToken cancellationToken,
        List<WearableItem> emoteBuffer = null);
    UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default);
    UniTask<IReadOnlyList<WearableItem>> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default);
    UniTask<EmbeddedEmotesSO> GetEmbeddedEmotes();
    UniTask<IReadOnlyList<WearableItem>> RequestEmoteCollectionInBuilderAsync(IEnumerable<string> collectionIds,
        CancellationToken cancellationToken, List<WearableItem> emoteBuffer = null);
    UniTask<WearableItem> RequestEmoteFromBuilderAsync(string emoteId, CancellationToken cancellationToken);

    Promise<IReadOnlyList<WearableItem>> RequestOwnedEmotes(string userId);
    Promise<WearableItem> RequestEmote(string id);
    List<Promise<WearableItem>> RequestEmotes(IList<string> ids);

    void ForgetEmote(string id);

    void ForgetEmotes(IList<string> ids);

}
