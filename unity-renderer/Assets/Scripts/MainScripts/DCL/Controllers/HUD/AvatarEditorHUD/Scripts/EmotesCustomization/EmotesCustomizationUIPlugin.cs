namespace DCL.EmotesCustomization
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Customization feature.
    /// </summary>
    public class EmotesCustomizationUIPlugin : IPlugin
    {
        public IEmotesCustomizationComponentController emotesCustomizationComponentController;

        public EmotesCustomizationUIPlugin()
        {
            emotesCustomizationComponentController = CreateController();
            emotesCustomizationComponentController.Initialize(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog);
        }

        internal virtual IEmotesCustomizationComponentController CreateController() => new EmotesCustomizationComponentController();

        public void Dispose()
        {
            emotesCustomizationComponentController.Dispose();
        }
    }
}