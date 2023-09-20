using DCLServices.EmotesService.Domain;

namespace DCL.Emotes
{
    public class NftEmoteReference : IEmoteReference
    {
        private readonly WearableItem emoteItem;
        private readonly IEmoteAnimationLoader loader;
        private readonly EmoteAnimationData emoteAnimationData;

        public NftEmoteReference(WearableItem emoteItem, IEmoteAnimationLoader loader, bool loop)
        {
            this.emoteItem = emoteItem;
            this.loader = loader;
            emoteAnimationData = new EmoteAnimationData(loader.mainClip, loader.container, loader.audioSource, loop);

            if (loader.IsSequential)
            {
                emoteAnimationData.SetupSequentialAnimation(loader.GetSequence());
            }
        }

        public WearableItem GetEntity() =>
            emoteItem;

        public EmoteAnimationData GetData() =>
            emoteAnimationData;

        public void Dispose()
        {
            loader.Dispose();
        }
    }
}
