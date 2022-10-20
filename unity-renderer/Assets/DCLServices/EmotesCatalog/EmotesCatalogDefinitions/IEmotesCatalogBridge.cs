using System;

public interface IEmotesCatalogBridge : IDisposable
{
    event Action<WearableItem[]> OnEmotesReceived;
    event Action<string, string> OnEmoteRejected;
    event Action<WearableItem[], string> OnOwnedEmotesReceived;

    void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}