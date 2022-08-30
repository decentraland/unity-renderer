namespace DCL.EmotesWheel
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Wheel feature.
    /// </summary>
    public class EmotesWheelUIPlugin : IPlugin
    {
        public EmotesWheelController emotesWheelController;

        public EmotesWheelUIPlugin() { emotesWheelController = new EmotesWheelController(UserProfile.GetOwnUserProfile(), CatalogController.wearableCatalog, Environment.i.serviceLocator.Get<IEmotesCatalogService>()); }

        public void Dispose() { }
    }
}