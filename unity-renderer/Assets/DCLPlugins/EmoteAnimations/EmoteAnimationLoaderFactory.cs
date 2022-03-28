using AvatarSystem;

namespace DCL.Emotes
{
    public class EmoteAnimationLoaderFactory
    {
        public virtual IEmoteAnimationLoader Get() { return new EmoteAnimationLoader(new WearableRetriever()); }
    }
}