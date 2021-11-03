public class MemoryChatProfanityFeatureFlag : IChatProfanityFeatureFlag
{
    private bool enabled;

    public MemoryChatProfanityFeatureFlag(bool enabled)
    {
        this.enabled = enabled;
    }

    public void Set(bool enabled) => this.enabled = enabled;

    public bool IsEnabled() => enabled;
}