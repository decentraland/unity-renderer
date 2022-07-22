using DCL;
using UnityEngine;

public class HideAvatarsModifier : IAvatarModifier
{
    public void ApplyModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHideAvatarAreaHandler handler))
            return;
        handler.ApplyHideModifier();
    }

    public void RemoveModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHideAvatarAreaHandler handler))
            return;
        handler.RemoveHideModifier();
    }
    
}