namespace DCL.EmotesWheel
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Wheel feature.
    /// </summary>
    public class EmotesWheelUIPlugin : IPlugin
    {
        public EmotesHUDController emotesHUDController;

        public EmotesWheelUIPlugin() { emotesHUDController = new EmotesHUDController(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog); }

        public void Dispose() { }
    }
}