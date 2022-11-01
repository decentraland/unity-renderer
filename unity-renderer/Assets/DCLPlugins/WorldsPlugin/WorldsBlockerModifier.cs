using System.Collections;
using System.Collections.Generic;
using DCL;
using Decentraland.Bff;
using UnityEngine;

public class WorldsBlockerModifier : IWorldsModifier
{
    public void EnteredRealm(bool isCatalyst, AboutResponse realmConfiguration)
    {
        if (isCatalyst)
        {
            Environment.i.world.blockersController.SetEnabled(true);
        }
        else
        {
            Environment.i.world.blockersController.SetEnabled(!string.IsNullOrEmpty(realmConfiguration.Configurations.CityLoaderContentServer));
        }
    }
    public void Dispose() { }
}
