namespace Emotes
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Customization feature.
    /// </summary>
    public class EmotesCustomizationFeature : IPlugin
    {
        public IEmotesCustomizationComponentController emotesCustomizationComponentController;

        public EmotesCustomizationFeature()
        {
            emotesCustomizationComponentController = CreateController();
            emotesCustomizationComponentController.Initialize();
        }

        internal virtual IEmotesCustomizationComponentController CreateController() => new EmotesCustomizationComponentController();

        public void Dispose()
        {
            emotesCustomizationComponentController.Dispose();
        }
    }
}