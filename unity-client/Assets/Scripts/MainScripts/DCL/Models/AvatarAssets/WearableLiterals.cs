using System.Collections.Generic;
using System.Linq;

public static class WearableLiterals
{
    public static class Misc
    {
        public const string HEAD = "head";
    }

    public static class Tags
    {
        public const string BASE_WEARABLE = "base-wearable";
        public const string EXCLUSIVE = "exclusive";
    }

    public static class Categories
    {
        public const string BODY_SHAPE = "body_shape";
        public const string UPPER_BODY = "upper_body";
        public const string LOWER_BODY = "lower_body";
        public const string FEET = "feet";
        public const string EYES = "eyes";
        public const string EYEBROWS = "eyebrows";
        public const string MOUTH = "mouth";
        public const string FACIAL = "facial";
        public const string HAIR = "hair";
    }

    public static class BodyShapes
    {
        public const string FEMALE = "dcl://base-avatars/BaseFemale";
        public const string MALE = "dcl://base-avatars/BaseMale";
    }

    public static class ItemRarity
    {
        public const string RARE = "rare";
        public const string EPIC = "epic";
        public const string LEGENDARY = "legendary";
        public const string MYTHIC = "mythic";
        public const string UNIQUE = "unique";
    }

    public static class DefaultWearables
    {
        private static readonly Dictionary<(string, string), string> defaultWearables = new Dictionary<(string, string), string>()
        {
            { (BodyShapes.MALE, Categories.EYES), "dcl://base-avatars/eyes_00" },
            { (BodyShapes.MALE, Categories.EYEBROWS), "dcl://base-avatars/eyebrows_00" },
            { (BodyShapes.MALE, Categories.MOUTH), "dcl://base-avatars/mouth_00" },
            { (BodyShapes.MALE, Categories.HAIR), "dcl://base-avatars/casual_hair_01" },
            { (BodyShapes.MALE, Categories.FACIAL), "dcl://base-avatars/beard"},
            { (BodyShapes.MALE, Categories.UPPER_BODY), "dcl://base-avatars/green_hoodie" },
            { (BodyShapes.MALE, Categories.LOWER_BODY), "dcl://base-avatars/brown_pants" },
            { (BodyShapes.MALE, Categories.FEET), "dcl://base-avatars/sneakers" },

            { (BodyShapes.FEMALE, Categories.EYES), "dcl://base-avatars/f_eyes_00" },
            { (BodyShapes.FEMALE, Categories.EYEBROWS), "dcl://base-avatars/f_eyebrows_00" },
            { (BodyShapes.FEMALE, Categories.MOUTH), "dcl://base-avatars/f_mouth_00" },
            { (BodyShapes.FEMALE, Categories.HAIR), "dcl://base-avatars/standard_hair" },
            { (BodyShapes.FEMALE, Categories.UPPER_BODY), "dcl://base-avatars/f_sweater" },
            { (BodyShapes.FEMALE, Categories.LOWER_BODY), "dcl://base-avatars/f_jeans" },
            { (BodyShapes.FEMALE, Categories.FEET), "dcl://base-avatars/bun_shoes" },
        };

        public static string[] GetDefaultWearables(string bodyShapeId) => defaultWearables.Where(x => x.Key.Item1 == bodyShapeId).Select(x => x.Value).ToArray();

        public static string GetDefaultWearable(string bodyShapeId, string category)
        {
            if (!defaultWearables.ContainsKey((bodyShapeId, category)))
                return null;

            return defaultWearables[(bodyShapeId, category)];
        }
    }
}
