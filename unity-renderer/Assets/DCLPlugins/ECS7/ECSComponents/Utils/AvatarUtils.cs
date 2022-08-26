using System.Linq;

namespace DCL.ECSComponents
{
    public static class AvatarUtils
    {
        public static bool HaveSameWearablesAndColors(PBAvatarShape avatar, PBAvatarShape otheravatar)
        {
            if (otheravatar == null || avatar == null)
                return false;

            bool wearablesAreEqual = avatar.GetWereables().All(otheravatar.GetWereables().Contains) && avatar.GetWereables().Count == otheravatar.GetWereables().Count;

            return avatar.GetBodyShape().Equals(otheravatar.GetBodyShape()) &&
                   avatar.GetSkinColor().Equals(otheravatar.GetSkinColor()) &&
                   avatar.GetHairColor().Equals(otheravatar.GetHairColor()) &&
                   avatar.GetEyeColor().Equals(otheravatar.GetEyeColor()) &&
                   wearablesAreEqual;
        }
    }
}