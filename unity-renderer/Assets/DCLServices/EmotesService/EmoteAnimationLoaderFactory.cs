using AvatarSystem;
using DCL.Providers;
using DCLServices.EmotesService;

namespace DCL.Emotes
{
    public class EmoteAnimationLoaderFactory
    {
        private readonly AddressableResourceProvider resourceProvider;
        private readonly EmoteVolumeHandler emoteVolumeHandler;

        public EmoteAnimationLoaderFactory(AddressableResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider;
            emoteVolumeHandler = new EmoteVolumeHandler();
        }

        public virtual IEmoteAnimationLoader Get() =>
            new EmoteAnimationLoader(new WearableRetriever(), resourceProvider, emoteVolumeHandler);
    }
}
