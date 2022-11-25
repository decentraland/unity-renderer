using DCL;
using Decentraland.Bff;
using System;
using UnityEngine;

namespace DCLPlugins.RealmsPlugin
{
    /// <summary>
    /// Align skybox with the realms settings. It allows the world creator to control skybox state.
    /// </summary>
    public class RealmsSkyboxModifier: IRealmsModifier
    {
        public void Dispose() { throw new NotImplementedException(); }

        public void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
        {

            if (realmConfiguration.Configurations.Skybox != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(realmConfiguration.Configurations.Skybox.FixedHour);
                Debug.Log(time);                 // DataStore.i.skyboxConfig.fixedTime.Set(valueAsFloat);
            }
        }
    }
}
