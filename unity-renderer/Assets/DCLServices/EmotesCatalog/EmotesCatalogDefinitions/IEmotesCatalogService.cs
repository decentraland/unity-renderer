using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using System.Threading.Tasks;

public interface IEmotesCatalogService : IService
{
    bool TryGetLoadedEmote(string id, out WearableItem emote);

    Promise<WearableItem[]> RequestOwnedEmotes(string userId);
    UniTask<WearableItem[]> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default);

    Promise<WearableItem> RequestEmote(string id);
    List<Promise<WearableItem>> RequestEmotes(IList<string> ids);
    UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default);
    UniTask<WearableItem[]> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default);

    Task<EmbeddedEmotesSO> GetEmbeddedEmotes();
    void ForgetEmote(string id);
    void ForgetEmotes(IList<string> ids);
}
