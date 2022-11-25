using DCL;
using Decentraland.Bff;
using System;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Align skybox with the realms settings. It allows the world creator to control skybox state.
    /// </summary>
    public class RealmsSkyboxModifier : IRealmsModifier
    {
        public void Dispose() { }

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {
            if (realmConfiguration.Configurations.Skybox != null)
            {
                DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);
                DataStore.i.skyboxConfig.fixedTime.Set(
                    (float)TimeSpan.FromSeconds(realmConfiguration.Configurations.Skybox.FixedHour).TotalHours);
            }
        }
    }
}
