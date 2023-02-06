using DCL;
using DCL.Helpers;
using ExploreV2Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables.RealmsInfo;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentControllerDesktop : ExploreV2MenuComponentController
{
    protected override IExploreV2MenuComponentView CreateView() =>
        ExploreV2MenuComponentViewDesktop.Create();
}
