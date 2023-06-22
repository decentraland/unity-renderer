using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutfitsPlugin : IPlugin
{
    public OutfitsPlugin()
    {
        DataStore.i.HUDs.enableOutfits.Set(true);
    }

    public void Dispose()
    {
    }
}
