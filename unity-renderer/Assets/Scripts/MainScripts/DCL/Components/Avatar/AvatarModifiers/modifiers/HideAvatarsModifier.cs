using UnityEngine;

public class HideAvatarsModifier : IAvatarModifier
{
    public void ApplyModifier(GameObject avatar)
    {
        IHideAvatarAreaHandler[] handlers = avatar.GetComponentsInChildren<IHideAvatarAreaHandler>();
        
        if(handlers.Length.Equals(0))
            return;
        
        for (int i = 0; i < handlers.Length; i++)
        {
            handlers[i].ApplyHideModifier();
        }
    }

    public void RemoveModifier(GameObject avatar)
    {
        IHideAvatarAreaHandler[] handlers = avatar.GetComponentsInChildren<IHideAvatarAreaHandler>();

        if(handlers.Length.Equals(0))
            return;

        for (int i = 0; i < handlers.Length; i++)
        {
            handlers[i].RemoveHideModifier();
        }
    }
}