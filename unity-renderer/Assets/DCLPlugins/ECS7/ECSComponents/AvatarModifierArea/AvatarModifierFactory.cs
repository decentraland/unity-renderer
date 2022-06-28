using System;

namespace DCL.ECSComponents
{
    public class AvatarModifierFactory
    {
        public IAvatarModifier GetOrCreateAvatarModifier(PBAvatarModifierArea.Types.Modifier modifier)
        {
            switch (modifier)
            {
                case PBAvatarModifierArea.Types.Modifier.HideAvatars:
                    return new HideAvatarsModifier();
                    break;
                case PBAvatarModifierArea.Types.Modifier.DisablePassports:
                    return new DisablePassportModifier();
                default:
                    return null;
            }
        }
    }
}