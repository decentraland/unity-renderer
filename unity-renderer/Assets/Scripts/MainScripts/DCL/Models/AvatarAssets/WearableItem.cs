using DCL;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WearableItem
{
    [Serializable]
    public class MappingPair
    {
        public string key;
        public string hash;
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
    }

    public Data data;
    public string id;

    public string baseUrl;
    public string baseUrlBundles;

    public i18n[] i18n;
    public string thumbnail;

    //This fields are temporary, once Kernel is finished we must move them to wherever they are placed
    public string rarity;
    public string description;
    public int issuedId;

    private readonly Dictionary<string, string> cachedI18n = new Dictionary<string, string>();

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

    private readonly Dictionary<string, ContentProvider> cachedContentProviers = new Dictionary<string, ContentProvider>();

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
            contents = contents.Select(mapping => new ContentServerUtils.MappingPair() { file = mapping.key, hash = mapping.hash }).ToList()
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

        if (representation?.overrideHides == null || representation.overrideHides.Length == 0)
            return data.hides;

        return representation.overrideHides;
    }

    public bool IsCollectible() { return !string.IsNullOrEmpty(rarity); }

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

    public static HashSet<string> CompoundHidesList(string bodyShapeId, List<WearableItem> wearables)
    {
        HashSet<string> result = new HashSet<string>();
        //Last wearable added has priority over the rest
        for (int i = wearables.Count - 1; i >= 0; i--)
        {
            var wearableItem = wearables[i];

            if (result.Contains(wearableItem.data.category)) //Skip hidden elements to avoid two elements hiding each other
                continue;

            var wearableHidesList = wearableItem.GetHidesList(bodyShapeId);
            if (wearableHidesList != null)
            {
                result.UnionWith(wearableHidesList);
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
}

[System.Serializable]
public class WearablesRequestResponse
{
    public WearableItem[] wearables;
    public string context;
}

[System.Serializable]
public class WearablesRequestFailed
{
    public string error;
    public string context;
}

[System.Serializable]
public class WearableContent
{
    public string file;
    public string hash;
}

[System.Serializable]
public class i18n
{
    public string code;
    public string text;
}