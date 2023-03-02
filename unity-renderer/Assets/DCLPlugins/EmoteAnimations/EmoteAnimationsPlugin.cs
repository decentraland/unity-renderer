using DCLServices.WearablesCatalogService;

namespace DCL.Emotes
{
    // [ADR 66 - https://github.com/decentraland/adr]
    public class EmoteAnimationsPlugin : IPlugin
    {
        private readonly EmoteAnimationsTracker emotesAnimationTracker;

        public EmoteAnimationsPlugin()
        {
            var wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();

            emotesAnimationTracker = new EmoteAnimationsTracker(
                DataStore.i.emotes,
                new EmoteAnimationLoaderFactory(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                wearablesCatalogService);
        }

        public void Dispose() { emotesAnimationTracker?.Dispose(); }
    }
}
