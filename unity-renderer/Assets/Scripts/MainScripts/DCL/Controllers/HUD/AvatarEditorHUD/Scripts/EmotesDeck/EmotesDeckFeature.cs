namespace EmotesDeck
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Deck feature.
    /// </summary>
    public class EmotesDeckFeature : IPlugin
    {
        public IEmotesDeckComponentController emotesDeckComponentController;

        public EmotesDeckFeature()
        {
            emotesDeckComponentController = CreateController();
            emotesDeckComponentController.Initialize();
        }

        internal virtual IEmotesDeckComponentController CreateController() => new EmotesDeckComponentController();

        public void Dispose()
        {
            emotesDeckComponentController.Dispose();
        }
    }
}