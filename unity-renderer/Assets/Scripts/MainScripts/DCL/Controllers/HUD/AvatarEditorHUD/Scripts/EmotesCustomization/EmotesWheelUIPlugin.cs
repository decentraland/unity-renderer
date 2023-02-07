using DCLServices.WearablesCatalogService;

namespace DCL.EmotesWheel
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Wheel feature.
    /// </summary>
    public class EmotesWheelUIPlugin : IPlugin
    {
        public EmotesWheelController emotesWheelController;

        public EmotesWheelUIPlugin()
        {
            emotesWheelController = new EmotesWheelController(
                UserProfile.GetOwnUserProfile(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                Environment.i.serviceLocator.Get<IWearablesCatalogService>());
        }

        public void Dispose() { }
    }
}
