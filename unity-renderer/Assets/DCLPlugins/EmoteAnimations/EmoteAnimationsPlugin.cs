using AvatarSystem;

namespace DCL.Emotes
{
    public class EmoteAnimationsPlugin : IPlugin
    {
        private readonly EmoteAnimationsTracker emotesAnimationTracker;

        public EmoteAnimationsPlugin() { emotesAnimationTracker = new EmoteAnimationsTracker(DataStore.i.emotes, new EmoteAnimationLoaderFactory(), new WearableItemResolver()); }

        public void Dispose() { emotesAnimationTracker?.Dispose(); }
    }
}