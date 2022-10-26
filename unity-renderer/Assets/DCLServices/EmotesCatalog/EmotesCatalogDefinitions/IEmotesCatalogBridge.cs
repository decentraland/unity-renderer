using System;

public delegate void EmoteRejectedDelegate(string emoteId, string errorMessage);

public interface IEmotesCatalogBridge : IDisposable
{
    
    event Action<WearableItem[]> OnEmotesReceived;
    event EmoteRejectedDelegate OnEmoteRejected;
    event Action<WearableItem[], string> OnOwnedEmotesReceived;

    void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}