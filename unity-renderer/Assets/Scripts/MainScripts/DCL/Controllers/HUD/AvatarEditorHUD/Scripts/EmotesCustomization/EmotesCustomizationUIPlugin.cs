namespace DCL.EmotesCustomization
{
    /// <summary>
    /// Plugin feature that initialize the Emotes Customization feature.
    /// </summary>
    public class EmotesCustomizationUIPlugin : IPlugin
    {
        internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

        public EmotesCustomizationUIPlugin() { emotesCustomizationDataStore.isEmotesCustomizationInitialized.Set(true); }

        public void Dispose() { }
    }
}