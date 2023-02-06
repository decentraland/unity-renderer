using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingHUDControllerDesktop : LoadingHUDController
{
    protected override LoadingHUDView CreateView()
    {
        return LoadingHUDViewDesktop.Create(this);
    }
}
