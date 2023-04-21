using DCL;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTriggerJumpToHome : ShowHideUIByTrigger
{

    protected override void SetUIVisibility(bool isVisible)
    {
        base.SetUIVisibility(isVisible && DataStore.i.HUDs.jumpHomeButtonVisible.Get());
    }
}
