using System;

namespace DCL.ECSComponents
{
    public class AvatarModifierFactory
    {
        private IAvatarModifier hideModifier;
        private IAvatarModifier hidePassportModifier;
        
        public IAvatarModifier GetOrCreateAvatarModifier(AvatarModifierType modifier)
        {
            switch (modifier)
            {
                case AvatarModifierType.AmtHideAvatars:
                    if (hideModifier == null)
                        hideModifier = new HideAvatarsModifier();
                    return hideModifier;
                    break;
                case AvatarModifierType.AmtDisablePassports:
                    if (hidePassportModifier == null)
                        hidePassportModifier = new HidePassportModifier();
                    return hidePassportModifier;
                default:
                    return null;
            }
        }

        public void Dispose()
        {
            hideModifier = null;
            hidePassportModifier = null;
        }
    }
}