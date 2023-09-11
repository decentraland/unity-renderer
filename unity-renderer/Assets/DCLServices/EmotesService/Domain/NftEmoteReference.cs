namespace DCL.Emotes
{
    public class NftEmoteReference : IEmoteReference
    {
        private readonly WearableItem emoteItem;
        private readonly IEmoteAnimationLoader loader;
        private readonly EmoteClipData emoteClipData;

        public NftEmoteReference(WearableItem emoteItem, IEmoteAnimationLoader loader, bool loop)
        {
            this.emoteItem = emoteItem;
            this.loader = loader;
            emoteClipData = new EmoteClipData(loader.mainClip, loader.container, loader.audioSource, loop);
        }

        public WearableItem GetEntity() =>
            emoteItem;

        public EmoteClipData GetData() =>
            emoteClipData;

        public void Dispose()
        {
            loader.Dispose();
        }
    }
}
