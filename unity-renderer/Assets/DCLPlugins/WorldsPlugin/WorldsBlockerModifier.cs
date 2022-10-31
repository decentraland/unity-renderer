using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class WorldsBlockerModifier : IWorldsModifier
{
    public void EnteredRealm(bool isRegularRealm, AboutResponse_AboutConfiguration realmConfiguration)
    {
        if (isRegularRealm)
        {
            Environment.i.world.blockersController.SetEnabled(true);
        }
        else
        {
            Environment.i.world.blockersController.SetEnabled(!string.IsNullOrEmpty(realmConfiguration.cityLoaderContentServer));
        }
    }
}
