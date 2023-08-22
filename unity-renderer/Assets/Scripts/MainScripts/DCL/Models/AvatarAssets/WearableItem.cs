using DCL;
using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static WearableLiterals;

// TODO: We need to separate this entity into WearableItem and EmoteItem they both can inherit a EntityItem and just have different metadata
[Serializable]
public class WearableItem
{
    private const string THIRD_PARTY_COLLECTIONS_PATH = "collections-thirdparty";
    public static readonly IList<string> CATEGORIES_PRIORITY = new List<string>
    {
        Categories.SKIN,
        Categories.UPPER_BODY,
        Categories.HANDS_WEAR,
        Categories.LOWER_BODY,
        Categories.FEET,
        Categories.HELMET,
        Categories.HAT,
        Categories.TOP_HEAD,
        Categories.MASK,
        Categories.EYEWEAR,
        Categories.EARRING,
        Categories.TIARA,
        Categories.HAIR,
        Categories.EYEBROWS,
        Categories.EYES,
        Categories.MOUTH,
        Categories.FACIAL_HAIR,
        Categories.BODY_SHAPE,
    };

    public static readonly Dictionary<string, string> CATEGORIES_READABLE_MAPPING = new ()
    {
        { Categories.SKIN, "Skin" },
        { Categories.UPPER_BODY, "Upper body" },
        { Categories.LOWER_BODY, "Lower body" },
        { Categories.FEET, "Feet" },
        { Categories.HELMET, "Helmet" },
        { Categories.HAT, "Hat" },
        { Categories.TOP_HEAD, "Top Head" },
        { Categories.MASK, "Mask" },
        { Categories.EYEWEAR, "Eyewear" },
        { Categories.EARRING, "Earring" },
        { Categories.TIARA, "Tiara" },
        { Categories.EYES, "Eyes" },
        { Categories.MOUTH, "Mouth" },
        { Categories.HAIR, "Hair" },
        { Categories.EYEBROWS, "Eyebrows" },
        { Categories.BODY_SHAPE, "Body shape" },
        { Categories.FACIAL_HAIR, "Facial hair" },
        { Categories.HANDS_WEAR, "Handwear" },
    };

    public static readonly string[] SKIN_IMPLICIT_CATEGORIES =
    {
        Categories.EYES,
        Categories.MOUTH,
        Categories.EYEBROWS,
        Categories.HAIR,
        Categories.UPPER_BODY,
        Categories.LOWER_BODY,
        Categories.FEET,
        Categories.HANDS,
        Categories.HANDS_WEAR,
        Categories.HEAD,
        Categories.FACIAL_HAIR
    };

    public static readonly string[] UPPER_BODY_DEFAULT_HIDES =
    {
        Categories.HANDS,
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
        public string[] removesDefaultHiding;
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

    private readonly Dictionary<string, string> cachedI18n = new ();
    private readonly Dictionary<string, ContentProvider> cachedContentProviers = new ();

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
            if (data.representations[i].bodyShapes.Contains(bodyShapeType)) { return data.representations[i]; }
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
            if (data.representations[i].bodyShapes.Contains(bodyShapeType)) { return true; }
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

        HashSet<string> hides = new HashSet<string>();

        if (representation?.overrideHides == null || representation.overrideHides.Length == 0)
            hides.UnionWith(data.hides ?? Enumerable.Empty<string>());
        else
            hides.UnionWith(representation.overrideHides);

        if (IsSkin())
            hides.UnionWith(SKIN_IMPLICIT_CATEGORIES);

        // we apply this rule to hide the hands by default if the wearable is an upper body or hides the upper body
        bool isOrHidesUpperBody = hides.Contains(Categories.UPPER_BODY) || data.category == Categories.UPPER_BODY;
        // the rule is ignored if the wearable contains the removal of this default rule (newer upper bodies since the release of hands)
        bool removesHandDefault = data.removesDefaultHiding?.Contains(Categories.HANDS) ?? false;
        // why we do this? because old upper bodies contains the base hand mesh, and they might clip with the new handwear items
        if (isOrHidesUpperBody && !removesHandDefault)
            hides.UnionWith(UPPER_BODY_DEFAULT_HIDES);

        string[] replaces = GetReplacesList(bodyShapeType);

        if (replaces != null)
            hides.UnionWith(replaces);

        // Safeguard so no wearable can hide itself
        hides.Remove(data.category);

        return hides.ToArray();
    }

    public void SanitizeHidesLists()
    {
        //remove bodyshape from hides list
        if (data.hides != null)
            data.hides = data.hides.Except(new[] { Categories.BODY_SHAPE }).ToArray();

        for (int i = 0; i < data.representations.Length; i++)
        {
            Representation representation = data.representations[i];

            if (representation.overrideHides != null)
                representation.overrideHides = representation.overrideHides.Except(new[] { Categories.BODY_SHAPE }).ToArray();
        }
    }

    public bool DoesHide(string category, string bodyShape) =>
        GetHidesList(bodyShape).Any(s => s == category);

    public bool IsCollectible()
    {
        if (id == null)
            return false;

        return !id.StartsWith("urn:decentraland:off-chain:base-avatars:");
    }

    public bool IsSkin() =>
        data.category == Categories.SKIN;

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
        if (!cachedI18n.ContainsKey(langCode)) { cachedI18n.Add(langCode, i18n.FirstOrDefault(x => x.code == langCode)?.text); }

        return cachedI18n[langCode];
    }

    public int GetIssuedCountFromRarity(string rarity)
    {
        switch (rarity)
        {
            case ItemRarity.RARE:
                return 5000;
            case ItemRarity.EPIC:
                return 1000;
            case ItemRarity.LEGENDARY:
                return 100;
            case ItemRarity.MYTHIC:
                return 10;
            case ItemRarity.UNIQUE:
                return 1;
        }

        return int.MaxValue;
    }

    public string ComposeThumbnailUrl()
    {
        return baseUrl + thumbnail;
    }

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

            if (wearableHidesList != null) { result.UnionWith(wearableHidesList); }
        }

        return result;
    }

    public static HashSet<string> ComposeHiddenCategoriesOrdered(string bodyShapeId, HashSet<string> forceRender, List<WearableItem> wearables)
    {
        var result = new HashSet<string>();
        var wearablesByCategory = wearables.ToDictionary(w => w.data.category);
        var previouslyHidden = new Dictionary<string, HashSet<string>>();

        foreach (string priorityCategory in CATEGORIES_PRIORITY)
        {
            previouslyHidden[priorityCategory] = new HashSet<string>();

            if (!wearablesByCategory.TryGetValue(priorityCategory, out var wearable) || wearable.GetHidesList(bodyShapeId) == null)
                continue;

            foreach (string categoryToHide in wearable.GetHidesList(bodyShapeId))
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

    public bool IsEmote() =>
        emoteDataV0 != null;

    public NftInfo GetNftInfo() =>
        new ()
        {
            Id = id,
            Category = IsEmote() ? "emote" : data?.category,
        };

    public virtual bool ShowInBackpack() =>
        true;

    public override string ToString() =>
        id;

    public string GetMarketplaceLink()
    {
        if (!IsCollectible())
            return "";

        const string MARKETPLACE = "https://market.decentraland.org/contracts/{0}/items/{1}";
        var split = id.Split(":");

        if (split.Length < 2)
            return "";

        // If this is not correct, we could retrieve the marketplace link by checking TheGraph, but that's super slow
        if (!split[^2].StartsWith("0x") || !int.TryParse(split[^1], out int _))
            return "";

        return string.Format(MARKETPLACE, split[^2], split[^1]);
    }
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
