using DCL;
using DCL.SettingsCommon;
using DCLPlugins.RealmPlugin;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Align skybox with the realm skybox settings. It allows the world creator to force specific skybox state.
    /// </summary>
    public class RealmsSkyboxModifier : IRealmModifier
    {
        private readonly DataStore_SkyboxConfig skyboxConfig;

        private bool hasCached;

        private SkyboxMode cachedMode;
        private float cachedFixedTime;

        public RealmsSkyboxModifier(DataStore_SkyboxConfig skyboxConfig)
        {
            this.skyboxConfig = skyboxConfig;
        }

        public void Dispose() { }

        public void OnEnteredRealm(bool _, AboutConfiguration realmConfiguration)
        {
            if (realmConfiguration.Skybox is { HasFixedHour: true })
            {
                if (!hasCached)
                    CacheCurrentSkyboxSettings();

                CommonSettingsScriptableObjects.SkyboxControlledByRealm.Set(true);
                skyboxConfig.UseFixedTimeFromSeconds(realmConfiguration.Skybox!.FixedHour, SkyboxMode.HoursFixedByWorld);
            }
            else if (hasCached)
                ResetToCached();
        }

        private void ResetToCached()
        {
            skyboxConfig.UseFixedTimeFromHours(cachedFixedTime, cachedMode);
            hasCached = false;

            CommonSettingsScriptableObjects.SkyboxControlledByRealm.Set(false);
        }

        private void CacheCurrentSkyboxSettings()
        {
            cachedMode = skyboxConfig.mode.Get();
            cachedFixedTime = skyboxConfig.fixedTime.Get();

            hasCached = true;
        }
    }
}
