using DCL;
using DCL.Components.Video.Plugin;
using UnityEngine;

public class VideoProviderFactory
{
    public static IVideoPluginWrapper CreateVideoProvider()
    {
#if AV_PRO_PRESENT
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("use_avpro_player") && Application.platform != RuntimePlatform.LinuxPlayer)
        {
            return new VideoPluginWrapper_AVPro();
        }
#endif
        return new VideoPluginWrapper_Native();
    }
}
