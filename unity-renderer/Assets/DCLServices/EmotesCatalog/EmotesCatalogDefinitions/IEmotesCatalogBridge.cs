using System;

public interface IEmotesCatalogBridge : IDisposable
{
    delegate void EmotesReceived(WearableItem[] emotes);
    delegate void OwnedEmotesReceived(WearableItem[] emotes, string userId);

    event EmotesReceived OnEmotesReceived;
    event OwnedEmotesReceived OnOwnedEmotesReceived;

    void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}