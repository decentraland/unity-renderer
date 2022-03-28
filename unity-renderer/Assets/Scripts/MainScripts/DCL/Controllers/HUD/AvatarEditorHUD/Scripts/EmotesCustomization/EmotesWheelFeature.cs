namespace EmotesCustomization
{
    public class EmotesWheelFeature : IPlugin
    {
        public EmotesHUDController emotesHUDController;

        public EmotesWheelFeature()
        {
            emotesHUDController = new EmotesHUDController(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog);
        }

        public void Dispose() { }
    }
}