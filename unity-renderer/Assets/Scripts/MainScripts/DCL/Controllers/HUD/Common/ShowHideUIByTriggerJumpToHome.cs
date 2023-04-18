using DCL;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTriggerJumpToHome : ShowHideUIByTrigger
{

    protected override void SetUIVisibility(bool isVisible)
    {
        if (!DataStore.i.HUDs.jumpHomeButtonVisible.Get())
        {
            base.SetUIVisibility(false);
            return;
        }
        base.SetUIVisibility(isVisible);
    }
}
