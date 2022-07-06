using System.Linq;

namespace DCL.ECSComponents
{
    public static class AvatarUtils
    {
        public static bool HaveSameWearablesAndColors(PBAvatarShape avatar, PBAvatarShape otheravatar)
        {
            if (otheravatar == null || avatar == null)
                return false;

            bool wearablesAreEqual = avatar.Wearables.All(otheravatar.Wearables.Contains) && avatar.Wearables.Count == otheravatar.Wearables.Count;

            return avatar.BodyShape.Equals(otheravatar.BodyShape) &&
                   avatar.SkinColor.Equals(otheravatar.SkinColor) &&
                   avatar.HairColor.Equals(otheravatar.HairColor) &&
                   avatar.EyeColor.Equals(otheravatar.EyeColor) &&
                   wearablesAreEqual;
        }

        public static bool HaveSameExpressions(PBAvatarShape avatar, PBAvatarShape otheravatar)
        {
            return avatar.ExpressionTriggerId == otheravatar.ExpressionTriggerId &&
                   avatar.ExpressionTriggerTimestamp == otheravatar.ExpressionTriggerTimestamp &&
                   avatar.StickerTriggerTimestamp == otheravatar.StickerTriggerTimestamp;
        }
    }
}