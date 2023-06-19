using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using System.Collections.Generic;
using System.Threading;

public interface IEmotesCatalogService : IService
{
    bool TryGetLoadedEmote(string id, out WearableItem emote);

    Promise<IReadOnlyList<WearableItem>> RequestOwnedEmotes(string userId);

    UniTask<IReadOnlyList<WearableItem>> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default);

    Promise<WearableItem> RequestEmote(string id);

    List<Promise<WearableItem>> RequestEmotes(IList<string> ids);

    UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default);

    UniTask<IReadOnlyList<WearableItem>> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default);

    UniTask<EmbeddedEmotesSO> GetEmbeddedEmotes();

    void ForgetEmote(string id);

    void ForgetEmotes(IList<string> ids);
}
