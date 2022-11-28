using DCL;
using Decentraland.Bff;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Align skybox with the realm skybox settings. It allows the world creator to force specific skybox state.
    /// </summary>
    public class RealmsSkyboxModifier : IRealmsModifier
    {
        private readonly DataStore_SkyboxConfig skyboxConfig;

        private bool hasCached;

        private bool cachedUseDynamicSkybox;
        private float cachedFixedTime;

        public RealmsSkyboxModifier(DataStore_SkyboxConfig skyboxConfig)
        {
            this.skyboxConfig = skyboxConfig;
        }

        public void Dispose() { }

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            if (realmConfiguration.Configurations.Skybox is { HasFixedHour: true })
            {
                if (!hasCached)
                    CacheCurrentSkyboxSettings();

                skyboxConfig.UseFixedTimeFromSeconds(realmConfiguration.Configurations.Skybox!.FixedHour);
            }
            else if (hasCached)
                ResetToCached();
        }

        private void ResetToCached()
        {
            if (cachedUseDynamicSkybox)
                skyboxConfig.useDynamicSkybox.Set(cachedUseDynamicSkybox);
            else
                skyboxConfig.UseFixedTimeFromHours(cachedFixedTime);

            hasCached = false;
        }

        private void CacheCurrentSkyboxSettings()
        {
            cachedUseDynamicSkybox = skyboxConfig.useDynamicSkybox.Get();
            cachedFixedTime = skyboxConfig.fixedTime.Get();

            hasCached = true;
        }
    }
}
