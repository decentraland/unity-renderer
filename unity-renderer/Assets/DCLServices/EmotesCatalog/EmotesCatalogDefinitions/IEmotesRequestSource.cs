using System;
using System.Collections.Generic;

public delegate void EmoteRejectedDelegate(string emoteId, string errorMessage);

public interface IEmotesRequestSource : IDisposable
{
    public delegate void OwnedEmotesReceived(IReadOnlyList<WearableItem> emotes, string userId, IReadOnlyDictionary<string, string> extendedUrns);

    event Action<IReadOnlyList<WearableItem>> OnEmotesReceived;
    event EmoteRejectedDelegate OnEmoteRejected;
    event OwnedEmotesReceived OnOwnedEmotesReceived;

    void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}
