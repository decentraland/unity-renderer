using DCL;
using Decentraland.Bff;
using System;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Align skybox with the realm skybox settings. It allows the world creator to force specific skybox state.
    /// </summary>
    public class RealmsSkyboxModifier : IRealmsModifier
    {
        private bool isOverriden;

        private bool cachedUseDynamicSkybox;
        private float cachedFixedTime;

        public void Dispose() { }

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            if (realmConfiguration.Configurations.Skybox is { HasFixedHour: true })
            {
                if (!isOverriden)
                {
                    isOverriden = true;
                    CacheCurrentSkyboxSettings();
                }

                OverrideFixedHours(realmConfiguration);
            }
            else if (isOverriden)
                ResetToCached();
        }

        private void ResetToCached()
        {
            DataStore.i.skyboxConfig.useDynamicSkybox.Set(cachedUseDynamicSkybox);

            if (!cachedUseDynamicSkybox)
                DataStore.i.skyboxConfig.fixedTime.Set(cachedFixedTime);

            isOverriden = false;
        }

        private void CacheCurrentSkyboxSettings()
        {
            cachedUseDynamicSkybox = DataStore.i.skyboxConfig.useDynamicSkybox.Get();
            cachedFixedTime = DataStore.i.skyboxConfig.fixedTime.Get();
        }

        private static void OverrideFixedHours(AboutResponse realmConfiguration)
        {
            DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);

            DataStore.i.skyboxConfig.fixedTime.Set(
                (float)TimeSpan.FromSeconds(realmConfiguration.Configurations.Skybox!.FixedHour).TotalHours);
        }
    }
}
