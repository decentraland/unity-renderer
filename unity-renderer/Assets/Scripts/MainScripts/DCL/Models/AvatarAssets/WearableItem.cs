using DCL;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WearableItem : Item
{
    [Serializable]
    public class Representation
    {
        public string[] bodyShapes;
        public string mainFile;
        public ContentServerUtils.MappingPair[] contents;
        public string[] overrideHides;
        public string[] overrideReplaces;
    }

    public Representation[] representations;
    public string category;
    public string[] tags;

    public string baseUrl;
    public string baseUrlBundles;

    public i18n[] i18n;
    public string thumbnail;
    public string[] hides;
    public string[] replaces;

    //This fields are temporary, once Kernel is finished we must move them to wherever they are placed
    public string rarity;
    public string description;
    public int issuedId;


    private readonly Dictionary<string, string> cachedI18n = new Dictionary<string, string>();


    public Representation GetRepresentation(string bodyShapeType)
    {
        if (representations == null) return null;

        for (int i = 0; i < representations.Length; i++)
        {
            if (representations[i].bodyShapes.Contains(bodyShapeType))
            {
                return representations[i];
            }
        }

        return null;
    }

    private readonly Dictionary<string, ContentProvider> cachedContentProviers = new Dictionary<string, ContentProvider>();

    public ContentProvider GetContentProvider(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        if (representation == null) return null;

        if (!cachedContentProviers.ContainsKey(bodyShapeType))
        {
            var contentProvider = CreateContentProvider(baseUrl, representation.contents.ToList());
            contentProvider.BakeHashes();
            cachedContentProviers.Add(bodyShapeType, contentProvider);
        }

        return cachedContentProviers[bodyShapeType];
    }

    protected virtual ContentProvider CreateContentProvider(string baseUrl, List<ContentServerUtils.MappingPair> contents)
    {
        return new ContentProvider
        {
            baseUrl = baseUrl,
            contents = contents
        };
    }

    public bool SupportsBodyShape(string bodyShapeType)
    {
        if (representations == null) return false;

        for (int i = 0; i < representations.Length; i++)
        {
            if (representations[i].bodyShapes.Contains(bodyShapeType))
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
            return replaces;

        return representation.overrideReplaces;
    }

    public string[] GetHidesList(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        if (representation?.overrideHides == null || representation.overrideHides.Length == 0)
            return hides;

        return representation.overrideHides;
    }

    public bool IsCollectible()
    {
        return !string.IsNullOrEmpty(rarity);
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

    public string ComposeThumbnailUrl()
    {
        return baseUrl + thumbnail;
    }

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
