namespace DCL
{
    public class DataStore_Common
    {
        public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();
        public readonly BaseVariable<bool> isApplicationQuitting = new BaseVariable<bool>();
        public readonly BaseDictionary<string, WearableItem> wearables = new BaseDictionary<string, WearableItem>();
        public readonly BaseVariable<bool> isPlayerRendererLoaded = new BaseVariable<bool>();
        public readonly BaseVariable<AppMode> appMode = new BaseVariable<AppMode>();
        public readonly BaseVariable<NFTPromptModel> onOpenNFTPrompt = new BaseVariable<NFTPromptModel>();
        public readonly BaseVariable<bool> isTutorialRunning = new BaseVariable<bool>(false);
    }
}