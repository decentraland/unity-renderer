using DCL;

public class ChatProfanityFeatureFlagFromDataStore : IChatProfanityFeatureFlag
{
    public bool IsEnabled() => DataStore.i.settings.profanityChatFilteringEnabled.Get();
}