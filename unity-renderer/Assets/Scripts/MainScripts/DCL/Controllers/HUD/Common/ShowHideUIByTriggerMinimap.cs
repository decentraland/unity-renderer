using DCL;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTriggerMinimap : ShowHideUIByTrigger
{

    protected override void SetUIVisibility(bool isVisible)
    {
        if (!DataStore.i.HUDs.minimapVisible.Get())
        {
            base.SetUIVisibility(false);
            return;
        }
        base.SetUIVisibility(isVisible);
    }
}
