using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreV2MenuComponentViewDesktop : ExploreV2MenuComponentView
{
    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("ExploreV2MenuDesktop")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2Desktop";

        return exploreV2View;
    }
}
