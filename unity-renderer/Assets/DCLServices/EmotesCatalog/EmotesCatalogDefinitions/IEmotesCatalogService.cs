using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;

public interface IEmotesCatalogService : IService
{
    void EmbedEmote(WearableItem embeddedEmote);
    void EmbedEmotes(WearableItem[] embeddedEmotes);

    Promise<WearableItem[]> RequestOwnedEmotes(string userId);
    UniTask<WearableItem[]> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default);

    Promise<WearableItem> RequestEmote(string id);
    List<Promise<WearableItem>> RequestEmotes(IList<string> ids);
    UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default);
    UniTask<WearableItem[]> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default);

    void ForgetEmote(string id);
    void ForgetEmotes(IList<string> ids);
}