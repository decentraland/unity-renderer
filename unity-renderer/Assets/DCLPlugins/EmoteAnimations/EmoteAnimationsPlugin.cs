using AvatarSystem;

namespace DCL.Emotes
{
    // [ADR 66 - https://github.com/decentraland/adr]
    public class EmoteAnimationsPlugin : IPlugin
    {
        private readonly EmoteAnimationsTracker emotesAnimationTracker;

        public EmoteAnimationsPlugin() { emotesAnimationTracker = new EmoteAnimationsTracker(DataStore.i.emotes, new EmoteAnimationLoaderFactory(), new WearableItemResolver(), Environment.i.serviceLocator.Get<IEmotesCatalogService>()); }

        public void Dispose() { emotesAnimationTracker?.Dispose(); }
    }
}