using System.Collections;
using System.Collections.Generic;
using DCL;
using DCLPlugins.RealmPlugin;
using Decentraland.Bff;
using UnityEngine;

namespace DCLPlugins.RealmsPlugin
{
    public class RealmsInfiniteFloorModifier : IRealmModifier
    {
        private BaseVariable<Texture> mapMainTexture;
        private BaseVariable<Texture> mapEstatesTexture;

        private BaseVariable<Texture> latestMapMainTexture;
        private BaseVariable<Texture> latestEstatesMainTexture;

        private bool currentlyInWorld;

        public RealmsInfiniteFloorModifier(DataStore_HUDs hudsDataStore)
        {
            this.mapMainTexture = hudsDataStore.mapMainTexture;
            this.mapEstatesTexture = hudsDataStore.mapEstatesTexture;
            this.latestMapMainTexture = hudsDataStore.latestDownloadedMainTexture;
            this.latestEstatesMainTexture = hudsDataStore.latestDownloadedMapEstatesTexture;
            
            latestMapMainTexture.OnChange += UpdateMapMainTexture;
            latestEstatesMainTexture.OnChange += UpdateEstatesMainTexture;
        }

        private void UpdateEstatesMainTexture(Texture current, Texture _)
        {
            if (currentlyInWorld) return;
            mapEstatesTexture.Set(current);
        }

        private void UpdateMapMainTexture(Texture current, Texture _)
        {
            if (currentlyInWorld) return;
            mapMainTexture.Set(current);
        }

        public void OnEnteredRealm(bool isWorld, AboutResponse.Types.AboutConfiguration realmConfiguration)
        {
            currentlyInWorld = isWorld;
            if (isWorld)
            {
                //TODO: We can offer the possibility to add an infinite world layout per world. For now,
                // we nullify and show the orange plaza color
                mapMainTexture.Set(null);
                mapEstatesTexture.Set(null);
            }
            else
            {
                mapMainTexture.Set(latestMapMainTexture.Get());
                mapEstatesTexture.Set(latestEstatesMainTexture.Get());
            }
        }
        
        public void Dispose()
        {
            latestMapMainTexture.OnChange -= UpdateMapMainTexture;
            latestEstatesMainTexture.OnChange -= UpdateEstatesMainTexture;
        }
    }
}