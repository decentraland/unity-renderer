using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static readonly ReadOnlyCollection<string> REQUIRED_CATEGORIES = new ReadOnlyCollection<string>(new List<string> { UPPER_BODY, LOWER_BODY, EYES, EYEBROWS, MOUTH });

        public const string BODY_SHAPE = "body_shape";
        public const string UPPER_BODY = "upper_body";
        public const string LOWER_BODY = "lower_body";
        public const string FEET = "feet";
        public const string EYES = "eyes";
        public const string EYEBROWS = "eyebrows";
        public const string MOUTH = "mouth";
        public const string FACIAL = "facial";
        public const string HAIR = "hair";
        public const string SKIN = "skin";
        public const string FACIAL_HAIR = "facial_hair";

        //TODO: Implement an IReadOnlyCollection for HashSet to make them immutable
        public static readonly HashSet<string> ALL  = new HashSet<string>
        {
            UPPER_BODY,
            LOWER_BODY,
            EYEBROWS,
            FACIAL,
            MOUTH,
            FEET,
            EYES,
            SKIN
        };
    }

    public static class BodyShapes
    {
        public const string FEMALE = "urn:decentraland:off-chain:base-avatars:BaseFemale";
        public const string MALE = "urn:decentraland:off-chain:base-avatars:BaseMale";
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
        public static readonly IReadOnlyDictionary<(string, string), string> defaultWearables = new Dictionary<(string, string), string>
        {
            { (BodyShapes.MALE, Categories.EYES), "urn:decentraland:off-chain:base-avatars:eyes_00" },
            { (BodyShapes.MALE, Categories.EYEBROWS), "urn:decentraland:off-chain:base-avatars:eyebrows_00" },
            { (BodyShapes.MALE, Categories.MOUTH), "urn:decentraland:off-chain:base-avatars:mouth_00" },
            { (BodyShapes.MALE, Categories.HAIR), "urn:decentraland:off-chain:base-avatars:casual_hair_01" },
            { (BodyShapes.MALE, Categories.FACIAL), "urn:decentraland:off-chain:base-avatars:beard" },
            { (BodyShapes.MALE, Categories.UPPER_BODY), "urn:decentraland:off-chain:base-avatars:green_hoodie" },
            { (BodyShapes.MALE, Categories.LOWER_BODY), "urn:decentraland:off-chain:base-avatars:brown_pants" },
            { (BodyShapes.MALE, Categories.FEET), "urn:decentraland:off-chain:base-avatars:sneakers" },

            { (BodyShapes.FEMALE, Categories.EYES), "urn:decentraland:off-chain:base-avatars:f_eyes_00" },
            { (BodyShapes.FEMALE, Categories.EYEBROWS), "urn:decentraland:off-chain:base-avatars:f_eyebrows_00" },
            { (BodyShapes.FEMALE, Categories.MOUTH), "urn:decentraland:off-chain:base-avatars:f_mouth_00" },
            { (BodyShapes.FEMALE, Categories.HAIR), "urn:decentraland:off-chain:base-avatars:standard_hair" },
            { (BodyShapes.FEMALE, Categories.UPPER_BODY), "urn:decentraland:off-chain:base-avatars:f_sweater" },
            { (BodyShapes.FEMALE, Categories.LOWER_BODY), "urn:decentraland:off-chain:base-avatars:f_jeans" },
            { (BodyShapes.FEMALE, Categories.FEET), "urn:decentraland:off-chain:base-avatars:bun_shoes" },
        };

        public static string[] GetDefaultWearables() => defaultWearables.Values.Distinct().ToArray();
        public static string[] GetDefaultWearables(string bodyShapeId) => defaultWearables.Where(x => x.Key.Item1 == bodyShapeId).Select(x => x.Value).ToArray();

        public static string GetDefaultWearable(string bodyShapeId, string category)
        {
            if (!defaultWearables.ContainsKey((bodyShapeId, category)))
                return null;

            return defaultWearables[(bodyShapeId, category)];
        }
    }
}