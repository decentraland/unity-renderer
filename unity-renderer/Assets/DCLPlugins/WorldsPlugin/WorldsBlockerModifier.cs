using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class WorldsBlockerModifier : IWorldsModifier
{
    public void EnteredWorld()
    {
        Environment.i.world.blockersController.SetEnabled(false);
    }
    public void ExitedWorld()
    {
        Environment.i.world.blockersController.SetEnabled(true);
    }
}
