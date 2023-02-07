using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingHUDViewDesktop : MonoBehaviour
{
    internal static LoadingHUDView Create(LoadingHUDController controller)
    {
        var view = Object.Instantiate(Resources.Load<GameObject>("LoadingHUDDesktop")).GetComponent<LoadingHUDView>();
        view.Initialize();
        return view;
    }
}
