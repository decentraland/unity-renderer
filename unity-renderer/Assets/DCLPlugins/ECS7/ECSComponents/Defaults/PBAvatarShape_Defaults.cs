using System;
using Google.Protobuf.Collections;

namespace DCL.ECSComponents
{
    public static class PBAvatarShape_Defaults
    {
        private static readonly Color3 colorWhite = new Color3()
        {
            R = 1, G = 1, B = 1
        };
        
        public static long GetExpressionTriggerTimestamp(this PBAvatarShape self)
        {
            return self.HasExpressionTriggerTimestamp ? self.ExpressionTriggerTimestamp : DateTime.Now.ToFileTimeUtc();
        }

        public static string GetBodyShape(this PBAvatarShape self)
        {
            return self.HasBodyShape ? self.BodyShape : "urn:decentraland:off-chain:base-avatars:BaseMale";
        }

        public static Color3 GetEyeColor(this PBAvatarShape self)
        {
            return self.EyeColor ?? new Color3(colorWhite);
        }
        
        public static Color3 GetHairColor(this PBAvatarShape self)
        {
            return self.EyeColor ?? new Color3(colorWhite);
        }
        
        public static Color3 GetSkinColor(this PBAvatarShape self)
        {
            return self.EyeColor ?? new Color3(colorWhite);
        }
        
        public static string GetName(this PBAvatarShape self)
        {
            return self.HasName ? self.Name : "NPC";
        }

        public static RepeatedField<string> GetWereables(this PBAvatarShape self)
        {
            return new RepeatedField<string>();
        }
    }
}