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
                   avatar.SkinColor.Equals(otheravatar.SkinColor) &&
                   avatar.HairColor.Equals(otheravatar.HairColor) &&
                   avatar.EyeColor.Equals(otheravatar.EyeColor) &&
                   wearablesAreEqual;
        }
    }
}