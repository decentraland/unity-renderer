using DCL;
using UnityEngine;

public class DisablePassportModifier : IAvatarModifier
{

    public void ApplyModifier(GameObject avatar)
    {
        AvatarShape avatarShape = avatar.GetComponent<AvatarShape>();
        if (avatarShape != null)
        {
            avatarShape.DisablePassport();
        }
    }

    public void RemoveModifier(GameObject avatar)
    {
        AvatarShape avatarShape = avatar.GetComponent<AvatarShape>();
        if (avatarShape != null)
        {
            avatarShape.EnablePassport();
        }
    }
}