using DCL;
using GPUSkinning;

public interface IGPUSkinningThrottlerService : IService
{
    public void Register(IGPUSkinning gpuSkinning, int framesBetweenUpdates);
    public void Unregister(IGPUSkinning gpuSkinning);
    public void ModifyThrottling(IGPUSkinning gpuSkinning, int framesBetweenUpdates);
}
