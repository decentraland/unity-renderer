using System;

namespace DCL.Emotes
{
    public class EmbedEmoteReference : IEmoteReference
    {
        private readonly WearableItem emoteItem;
        private readonly EmoteClipData clipData;

        public EmbedEmoteReference(WearableItem emoteItem, EmoteClipData clipData)
        {
            this.emoteItem = emoteItem;
            this.clipData = clipData;
        }

        public WearableItem GetEntity() =>
            emoteItem;

        public EmoteClipData GetData() =>
            clipData;

        public void Dispose()
        {

        }
    }
}
