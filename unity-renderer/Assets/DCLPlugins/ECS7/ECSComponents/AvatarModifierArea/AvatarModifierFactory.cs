using System;

namespace DCL.ECSComponents
{
    public class AvatarModifierFactory
    {
        private IAvatarModifier hideModifier;
        private IAvatarModifier disablePassportModifier;
        
        public IAvatarModifier GetOrCreateAvatarModifier(PBAvatarModifierArea.Types.Modifier modifier)
        {
            switch (modifier)
            {
                case PBAvatarModifierArea.Types.Modifier.HideAvatars:
                    if (hideModifier == null)
                        hideModifier = new HideAvatarsModifier();
                    return hideModifier;
                    break;
                case PBAvatarModifierArea.Types.Modifier.DisablePassports:
                    if (disablePassportModifier == null)
                        disablePassportModifier = new DisablePassportModifier();
                    return disablePassportModifier;
                default:
                    return null;
            }
        }

        public void Dispose()
        {
            hideModifier = null;
            disablePassportModifier = null;
        }
    }
}