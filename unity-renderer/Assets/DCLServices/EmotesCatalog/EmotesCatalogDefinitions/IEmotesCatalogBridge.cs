using System;

public interface IEmotesCatalogBridge : IDisposable
{
    public delegate void EmotesReceived(WearableItem[] emotes);
    public delegate void OwnedEmotesReceived(WearableItem[] emotes, string userId);

    event EmotesReceived OnEmotesReceived;
    event OwnedEmotesReceived OnOwnedEmotesReceived;

    public void RequestOwnedEmotes(string userId);
    void RequestEmote(string emoteId) ;
}