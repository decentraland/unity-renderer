using System;

namespace DCL.ECSComponents
{
    public class AvatarModifierFactory
    {
        private IAvatarModifier hideModifier;
        private IAvatarModifier hidePassportModifier;
        
        public IAvatarModifier GetOrCreateAvatarModifier(AvatarModifier modifier)
        {
            switch (modifier)
            {
                case AvatarModifier.AmHideAvatars:
                    if (hideModifier == null)
                        hideModifier = new HideAvatarsModifier();
                    return hideModifier;
                    break;
                case AvatarModifier.AmDisablePassports:
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