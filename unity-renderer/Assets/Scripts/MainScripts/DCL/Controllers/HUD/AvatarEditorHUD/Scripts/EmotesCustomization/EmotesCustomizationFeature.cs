namespace EmotesCustomization
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Customization feature.
    /// </summary>
    public class EmotesCustomizationFeature : IPlugin
    {
        public IEmotesCustomizationComponentController emotesCustomizationComponentController;
        public EmotesHUDController emotesHUDController;

        public EmotesCustomizationFeature()
        {
            emotesCustomizationComponentController = CreateController();
            emotesCustomizationComponentController.Initialize(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog);
            emotesHUDController = new EmotesHUDController(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog);
        }

        internal virtual IEmotesCustomizationComponentController CreateController() => new EmotesCustomizationComponentController();

        public void Dispose()
        {
            emotesCustomizationComponentController.Dispose();
        }
    }
}