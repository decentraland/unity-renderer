namespace DCL
{
    public class DataStore_Common
    {
        public readonly BaseVariable<bool> isSignUpFlow = new ();
        public readonly BaseVariable<bool> isApplicationQuitting = new ();
        public readonly BaseDictionary<string, WearableItem> wearables = new ();
        public readonly BaseVariable<bool> isPlayerRendererLoaded = new ();
        public readonly BaseVariable<AppMode> appMode = new ();
        public readonly BaseVariable<NFTPromptModel> onOpenNFTPrompt = new ();
        public readonly BaseVariable<bool> isTutorialRunning = new (false);
        public readonly BaseVariable<bool> isWorld = new (false);
        public readonly BaseVariable<bool> exitedWorldThroughGoBackButton = new (false);
    }
}
