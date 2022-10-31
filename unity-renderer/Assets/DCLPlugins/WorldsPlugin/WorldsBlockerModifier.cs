using System.Collections;
using System.Collections.Generic;
using DCL;
using Decentraland.Bff;
using UnityEngine;

public class WorldsBlockerModifier : IWorldsModifier
{
    public void EnteredRealm(bool isCatalyst, AboutResponse.Types.AboutConfiguration realmConfiguration)
    {
        if (isCatalyst)
        {
            Environment.i.world.blockersController.SetEnabled(true);
        }
        else
        {
            Environment.i.world.blockersController.SetEnabled(!string.IsNullOrEmpty(realmConfiguration.CityLoaderContentServer));
        }
    }
    public void Dispose() { }
}
