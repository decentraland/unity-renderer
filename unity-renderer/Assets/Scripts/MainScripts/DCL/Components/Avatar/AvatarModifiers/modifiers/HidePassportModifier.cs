using DCL;
using UnityEngine;

public class HidePassportModifier : IAvatarModifier
{

    public void ApplyModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHidePassportAreaHandler handler))
            return;
        handler.ApplyHidePassportModifier();
    }

    public void RemoveModifier(GameObject avatar)
    {
        if (!avatar.TryGetComponent(out IHidePassportAreaHandler handler))
            return;
        handler.RemoveHidePassportModifier();
    }
    
}