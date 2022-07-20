using DCL;
using UnityEngine;

public class HideAvatarsModifier : IAvatarModifier
{
    public void ApplyModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHideAvatarAreaHandler handler))
            return;
        handler.ApplyHideModifier(GetWarningDescription());
    }

    public void RemoveModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHideAvatarAreaHandler handler))
            return;
        handler.RemoveHideModifier(GetWarningDescription());
    }
    
    public string GetWarningDescription()
    {
        return "\u2022  The avatars are hidden";
    }
    
}