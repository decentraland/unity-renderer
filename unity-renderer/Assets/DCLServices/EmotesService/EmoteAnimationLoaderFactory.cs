using AvatarSystem;
using DCL.Providers;

namespace DCL.Emotes
{
    public class EmoteAnimationLoaderFactory
    {
        private readonly AddressableResourceProvider resourceProvider;

        public EmoteAnimationLoaderFactory(AddressableResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        public virtual IEmoteAnimationLoader Get() =>
            new EmoteAnimationLoader(new WearableRetriever(), resourceProvider);
    }
}
