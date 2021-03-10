using DCL;
using UnityEngine;

public class DisablePassportModifier : AvatarModifier
{

    public override void ApplyModifier(GameObject avatar)
    {
        AvatarShape avatarShape = avatar.GetComponent<AvatarShape>();
        if (avatarShape != null)
        {
            avatarShape.DisablePassport();
        }
    }

    public override void RemoveModifier(GameObject avatar)
    {
        AvatarShape avatarShape = avatar.GetComponent<AvatarShape>();
        if (avatarShape != null)
        {
            avatarShape.EnablePassport();
        }
    }
}
