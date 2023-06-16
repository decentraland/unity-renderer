using DCL;
using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// TODO: We need to separate this entity into WearableItem and EmoteItem they both can inherit a EntityItem and just have different metadata
[Serializable]
public class WearableItem
{
    private const string THIRD_PARTY_COLLECTIONS_PATH = "collections-thirdparty";
    public static readonly List<string> CATEGORIES_PRIORITY = new ()
    {
        "skin", "upper_body", "lower_body", "feet", "helmet", "hat", "top_head", "mask", "eyewear", "earring", "tiara", "hair", "eyebrows", "eyes", "mouth", "facial_hair", "body_shape"
    };

    public static readonly Dictionary<string, string> CATEGORIES_READABLE_MAPPING = new ()
    {
        { "skin", "Skin" },
        { "upper_body", "Upper body" },
        { "lower_body", "Lower body" },
        { "feet", "Feet" },
        { "helmet", "Helmet" },
        { "hat", "Hat" },
        { "top_head", "Top Head" },
        { "mask", "Mask" },
        { "eyewear", "Eyewear" },
        { "earring", "Earring" },
        { "tiara", "Tiara" },
        { "eyes", "Eyes" },
        { "mouth", "Mouth" },
        { "hair", "Hair" },
        { "eyebrows", "Eyebrows" },
        { "body_shape", "Body shape" },
        { "facial_hair", "Facial hair" },
    };

    [Serializable]
    public class MappingPair
    {
        public string key;
        public string hash;
        public string url;
    }

    [Serializable]
    public class Representation
    {
        public string[] bodyShapes;
        public string mainFile;
        public MappingPair[] contents;
        public string[] overrideHides;
        public string[] overrideReplaces;
    }

    [Serializable]
    public class Data
    {
        public Representation[] representations;
        public string category;
        public string[] tags;
        public string[] replaces;
        public string[] hides;
        public bool loop;
    }

    public Data data;
    public EmoteDataV0 emoteDataV0;
    public string id; // urn
    public string entityId;

    public string baseUrl;
    public string baseUrlBundles;

    public i18n[] i18n;
    public string thumbnail;

    public DateTime MostRecentTransferredDate { get; set; }

    private string thirdPartyCollectionId;
    public string ThirdPartyCollectionId
    {
        get
        {
            if (!string.IsNullOrEmpty(thirdPartyCollectionId))
                return thirdPartyCollectionId;
            if (!id.Contains(THIRD_PARTY_COLLECTIONS_PATH))
                return "";
            var paths = id.Split(':');
            var thirdPartyIndex = Array.IndexOf(paths, THIRD_PARTY_COLLECTIONS_PATH);
            thirdPartyCollectionId = string.Join(":", paths, 0, thirdPartyIndex + 2);
            return thirdPartyCollectionId;
        }
    }

    public bool IsFromThirdPartyCollection => !string.IsNullOrEmpty(ThirdPartyCollectionId);

    public Sprite thumbnailSprite;

    //This fields are temporary, once Kernel is finished we must move them to wherever they are placed
    public string rarity;
    public string description;
    public int issuedId;

    private readonly Dictionary<string, string> cachedI18n = new Dictionary<string, string>();
    private readonly Dictionary<string, ContentProvider> cachedContentProviers =
        new Dictionary<string, ContentProvider>();

    private readonly string[] skinImplicitCategories =
    {
        WearableLiterals.Categories.EYES,
        WearableLiterals.Categories.MOUTH,
        WearableLiterals.Categories.EYEBROWS,
        WearableLiterals.Categories.HAIR,
        WearableLiterals.Categories.UPPER_BODY,
        WearableLiterals.Categories.LOWER_BODY,
        WearableLiterals.Categories.FEET,
        WearableLiterals.Misc.HEAD,
        WearableLiterals.Categories.FACIAL_HAIR
    };

    public bool TryGetRepresentation(string bodyshapeId, out Representation representation)
    {
        representation = GetRepresentation(bodyshapeId);
        return representation != null;
    }

    public Representation GetRepresentation(string bodyShapeType)
    {
        if (data?.representations == null)
            return null;

        for (int i = 0; i < data.representations.Length; i++)
        {
            if (data.representations[i].bodyShapes.Contains(bodyShapeType))
            {
                return data.representations[i];
            }
        }

        return null;
    }

    public ContentProvider GetContentProvider(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        if (representation == null)
            return null;

        if (!cachedContentProviers.ContainsKey(bodyShapeType))
        {
            var contentProvider = CreateContentProvider(baseUrl, representation.contents);
            contentProvider.BakeHashes();
            cachedContentProviers.Add(bodyShapeType, contentProvider);
        }

        return cachedContentProviers[bodyShapeType];
    }

    protected virtual ContentProvider CreateContentProvider(string baseUrl, MappingPair[] contents)
    {
        return new ContentProvider
        {
            baseUrl = baseUrl,
            assetBundlesBaseUrl = baseUrlBundles,
            contents = contents.Select(mapping => new ContentServerUtils.MappingPair()
                                   { file = mapping.key, hash = mapping.hash })
                               .ToList()
        };
    }

    public bool SupportsBodyShape(string bodyShapeType)
    {
        if (data?.representations == null)
            return false;

        for (int i = 0; i < data.representations.Length; i++)
        {
            if (data.representations[i].bodyShapes.Contains(bodyShapeType))
            {
                return true;
            }
        }

        return false;
    }

    public string[] GetReplacesList(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        if (representation?.overrideReplaces == null || representation.overrideReplaces.Length == 0)
            return data.replaces;

        return representation.overrideReplaces;
    }

    public string[] GetHidesList(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        string[] hides;

        if (representation?.overrideHides == null || representation.overrideHides.Length == 0)
            hides = data.hides;
        else
            hides = representation.overrideHides;

        if (IsSkin())
        {
            hides = hides == null
                ? skinImplicitCategories
                : hides.Concat(skinImplicitCategories).Distinct().ToArray();
        }

        var replaces = GetReplacesList(bodyShapeType);

        if (hides != null && replaces != null)
        {
            //merge hides and replaces removing duplicates and own category
            var combinedArray = new string[hides.Length + replaces.Length];
            Array.Copy(hides, combinedArray, hides.Length);
            Array.Copy(replaces, 0, combinedArray, hides.Length, replaces.Length);
            return combinedArray.Where(w => w != data.category).ToArray();
        }

        return hides;
    }

    public void SanitizeHidesLists()
    {
        //remove bodyshape from hides list
        if (data.hides != null)
            data.hides = data.hides.Except(new [] { WearableLiterals.Categories.BODY_SHAPE }).ToArray();
        for (int i = 0; i < data.representations.Length; i++)
        {
            Representation representation = data.representations[i];
            if (representation.overrideHides != null)
                representation.overrideHides = representation.overrideHides.Except(new [] { WearableLiterals.Categories.BODY_SHAPE }).ToArray();

        }
    }

    public bool DoesHide(string category, string bodyShape) => GetHidesList(bodyShape).Any(s => s == category);

    public bool IsCollectible()
    {
        if (id == null)
            return false;

        return !id.StartsWith("urn:decentraland:off-chain:base-avatars:");
    }

    public bool IsSkin() => data.category == WearableLiterals.Categories.SKIN;

    public bool IsSmart()
    {
        if (data?.representations == null)
            return false;

        for (var i = 0; i < data.representations.Length; i++)
        {
            var representation = data.representations[i];
            var containsGameJs = representation.contents?.Any(pair => pair.key.EndsWith("game.js")) ?? false;
            if (containsGameJs)
                return true;
        }

        return false;
    }

    public string GetName(string langCode = "en")
    {
        if (!cachedI18n.ContainsKey(langCode))
        {
            cachedI18n.Add(langCode, i18n.FirstOrDefault(x => x.code == langCode)?.text);
        }

        return cachedI18n[langCode];
    }

    public int GetIssuedCountFromRarity(string rarity)
    {
        switch (rarity)
        {
            case WearableLiterals.ItemRarity.RARE:
                return 5000;
            case WearableLiterals.ItemRarity.EPIC:
                return 1000;
            case WearableLiterals.ItemRarity.LEGENDARY:
                return 100;
            case WearableLiterals.ItemRarity.MYTHIC:
                return 10;
            case WearableLiterals.ItemRarity.UNIQUE:
                return 1;
        }

        return int.MaxValue;
    }

    public string ComposeThumbnailUrl() { return baseUrl + thumbnail; }

    public static HashSet<string> ComposeHiddenCategories(string bodyShapeId, List<WearableItem> wearables)
    {
        HashSet<string> result = new HashSet<string>();
        //Last wearable added has priority over the rest
        for (int index = 0; index < wearables.Count; index++)
        {
            WearableItem wearableItem = wearables[index];
            if (result.Contains(wearableItem.data.category)) //Skip hidden elements to avoid two elements hiding each other
                continue;

            string[] wearableHidesList = wearableItem.GetHidesList(bodyShapeId);
            if (wearableHidesList != null)
            {
                result.UnionWith(wearableHidesList);
            }
        }

        return result;
    }

    public static HashSet<string> ComposeHiddenCategoriesOrdered(string bodyShapeId, HashSet<string> forceRender, List<WearableItem> wearables)
    {
        var result = new HashSet<string>();
        var wearablesByCategory = wearables.ToDictionary(w => w.data.category);
        var previouslyHidden = new Dictionary<string, HashSet<string>>();

        foreach (var priorityCategory in CATEGORIES_PRIORITY)
        {
            previouslyHidden[priorityCategory] = new HashSet<string>();

            if (!wearablesByCategory.TryGetValue(priorityCategory, out var wearable) || wearable.GetHidesList(bodyShapeId) == null)
                continue;

            foreach (var categoryToHide in wearable.GetHidesList(bodyShapeId))
            {
                if (previouslyHidden.TryGetValue(categoryToHide, out var hiddenCategories) && hiddenCategories.Contains(priorityCategory))
                    continue;

                previouslyHidden[priorityCategory].Add(categoryToHide);

                if (forceRender != null && forceRender.Contains(categoryToHide))
                    continue;

                result.Add(categoryToHide);
            }
        }

        return result;
    }

    //Workaround to know the net of a wearable.
    //Once wearables are allowed to be moved from Ethereum to Polygon this method wont be reliable anymore
    //To retrieve this properly first we need the catalyst to send the net of each wearable, not just the ID
    public bool IsInL2()
    {
        if (id.StartsWith("urn:decentraland:matic") || id.StartsWith("urn:decentraland:mumbai"))
            return true;
        return false;
    }

    public bool IsEmote() { return emoteDataV0 != null; }

    public NftInfo GetNftInfo() =>
        new ()
        {
            Id = id,
            Category = IsEmote() ? "emote" : data?.category,
        };

    public virtual bool ShowInBackpack() { return true; }

    public override string ToString() { return id; }
}

[Serializable]
public class EmoteItem : WearableItem { }

[Serializable]
public class WearablesRequestResponse
{
    public WearableItem[] wearables;
    public string context;
}

[Serializable]
public class WearablesRequestFailed
{
    public string error;
    public string context;
}

[Serializable]
public class WearableContent
{
    public string file;
    public string hash;
}

[Serializable]
public class i18n
{
    public string code;
    public string text;
}
