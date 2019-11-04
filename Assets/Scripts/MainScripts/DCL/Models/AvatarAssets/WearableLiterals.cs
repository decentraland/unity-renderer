using System.Collections.Generic;

public static class WearableLiterals
{
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

    public static class DefaultWearables
    {
        private static readonly Dictionary<(string, string), string> defaultWearables = new Dictionary<(string, string), string>()
        {
            { (BodyShapes.MALE, Categories.EYES), "dcl://base-avatars/eyes_00" },
            { (BodyShapes.MALE, Categories.EYEBROWS), "dcl://base-avatars/eyebrows_00" },
            { (BodyShapes.MALE, Categories.MOUTH), "dcl://base-avatars/mouth_00" },

            { (BodyShapes.FEMALE, Categories.EYES), "dcl://base-avatars/f_eyes_00" },
            { (BodyShapes.FEMALE, Categories.EYEBROWS), "dcl://base-avatars/f_eyebrows_00" },
            { (BodyShapes.FEMALE, Categories.MOUTH), "dcl://base-avatars/f_mouth_00" },
        };

        public static string GetDefaultWearable(string bodyShapeId, string category)
        {
            if (!defaultWearables.ContainsKey((bodyShapeId, category)))
                return null;

            return defaultWearables[(bodyShapeId, category)];
        }
    }
}