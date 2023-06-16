using System;
using System.Collections.Generic;

public delegate void EmoteRejectedDelegate(string emoteId, string errorMessage);

public interface IEmotesRequestSource : IDisposable
{

    event Action<IReadOnlyList<WearableItem>> OnEmotesReceived;
    event EmoteRejectedDelegate OnEmoteRejected;
    event Action<IReadOnlyList<WearableItem>, string> OnOwnedEmotesReceived;

    void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}
