public class SkinFeature : IPlugin
{
    public SkinFeature(IBaseVariable<bool> isFeatureEnabled)
    {
        isFeatureEnabled.Set(true);
    }
    
    public void Dispose()
    {
    }
}