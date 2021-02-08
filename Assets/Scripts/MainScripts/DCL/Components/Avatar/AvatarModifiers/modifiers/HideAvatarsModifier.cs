using DCL;
using UnityEngine;

public class HideAvatarsModifier : AvatarModifier
{
    private const string HIDE_AVATARS_MODIFIER = "HIDE_AVATARS_MODIFIER";

    public override void ApplyModifier(GameObject avatar)
    {
        AvatarVisibility avatarVisibility = avatar.GetComponent<AvatarVisibility>();
        if (avatarVisibility != null)
        {
            avatarVisibility.SetVisibility(HIDE_AVATARS_MODIFIER, false);
        }
    }

    public override void RemoveModifier(GameObject avatar)
    {
        AvatarVisibility avatarVisibility = avatar.GetComponent<AvatarVisibility>();
        if (avatarVisibility != null)
        {
            avatarVisibility.SetVisibility(HIDE_AVATARS_MODIFIER, true);
        }
    }
}
