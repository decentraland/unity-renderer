using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapHUDViewDesktop : MonoBehaviour
{
    internal static MinimapHUDView Create(MinimapHUDController controller)
    {
        var view = Object.Instantiate(Resources.Load<GameObject>("MinimapHUDDesktop")).GetComponent<MinimapHUDView>();
        view.Initialize(controller);

        return view;
    }
}
