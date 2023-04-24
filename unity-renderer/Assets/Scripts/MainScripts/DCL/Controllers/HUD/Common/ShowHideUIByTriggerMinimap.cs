using DCL;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTriggerMinimap : ShowHideUIByTrigger
{

    protected override void SetUIVisibility(bool isVisible)
    {
        base.SetUIVisibility(isVisible && DataStore.i.HUDs.minimapVisible.Get());
    }
}
