using AvatarSystem;
using DCLServices.WearablesCatalogService;

namespace DCL.Emotes
{
    // [ADR 66 - https://github.com/decentraland/adr]
    public class EmoteAnimationsPlugin : IPlugin
    {
        private readonly EmoteAnimationsTracker emotesAnimationTracker;

        public EmoteAnimationsPlugin()
        {
            emotesAnimationTracker = new EmoteAnimationsTracker(
                DataStore.i.emotes,
                new EmoteAnimationLoaderFactory(),
                new WearableItemResolver(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                Environment.i.serviceLocator.Get<IWearablesCatalogService>());
        }

        public void Dispose() { emotesAnimationTracker?.Dispose(); }
    }
}
