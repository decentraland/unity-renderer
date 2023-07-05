using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class WearableLiterals
{
    public static class Tags
    {
        public const string BASE_WEARABLE = "base-wearable";
        public const string EXCLUSIVE = "exclusive";
    }

    public static class Categories
    {
        public static readonly IReadOnlyCollection<string> REQUIRED_CATEGORIES = new HashSet<string>
        {
            EYES,
            MOUTH
        };

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
        public const string EYEWEAR = "eyewear";
        public const string TIARA = "tiara";
        public const string EARRING = "earring";
        public const string HAT = "hat";
        public const string TOP_HEAD = "top_head";
        public const string HELMET = "helmet";
        public const string MASK = "mask";
        public const string HANDS = "hands";
        public const string HANDS_WEAR = "hands_wear";
        public const string HEAD = "head";
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

    public static class DefaultEmotes
    {
        public const string OUTFIT_SHOES = "Outfit_Shoes_v0";
        public const string OUTFIT_LOWER = "Outfit_Lower_v0";
        public const string OUTFIT_UPPER = "Outfit_Upper_v0";
        public const string OUTFIT_ACCESSORIES = "Outfit_Accessories_v0";
    }
}
