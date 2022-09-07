using System;
using System.Collections.Generic;
using DCL.Emotes;
using UnityEngine;

[Serializable]
public class WearablesAPIData
{
    [Serializable]
    public class Wearable
    {
        [Serializable]
        public class Data
        {
            [Serializable]
            public class Representation
            {
                [Serializable]
                public class Content
                {
                    public string key;
                    public string url;
                }

                public string[] bodyShapes;
                public string mainFile;
                public string[] overrideReplaces;
                public string[] overrideHides;
                public Content[] contents;
            }

            public string[] tags;
            public string category;
            public Representation[] representations;
        }

        public string id;
        public string description;
        public string thumbnail;
        public string rarity;
        public Data data;
        public i18n[] i18n;
        public EmoteDataV0 emoteDataV0 = null;
        public long createdAt;
        public long updatedAt;
    }

    [Serializable]
    public class PaginationData
    {
        public int limit;
        public string next = null;
    }

    // TODO: dinamically use ICatalyst.contentUrl, content server is not a const
    private const string WEARABLES_CONTENT_BASE_URL = "https://peer.decentraland.org/content/contents/";

    public List<Wearable> wearables;
    public PaginationData pagination;
    public List<WearableItem> GetWearableItems()
    {
        if (wearables == null || wearables.Count == 0)
            return null;

        List<WearableItem> result = new List<WearableItem>();

        foreach (var wearableData in wearables)
        {
            // Populate new WearableItem with fetched data
            WearableItem wearable = new WearableItem()
            {
                id = wearableData.id,
                baseUrl = WEARABLES_CONTENT_BASE_URL,
                thumbnail = wearableData.thumbnail,
                rarity = wearableData.rarity,
                description = wearableData.description,
                i18n = wearableData.i18n,
                emoteDataV0 = wearableData.emoteDataV0,
                data = new WearableItem.Data()
                {
                    category = wearableData.data.category,
                    tags = wearableData.data.tags
                }
            };

            List<WearableItem.Representation> wearableRepresentations = new List<WearableItem.Representation>();
            foreach (var wearableDataRepresentation in wearableData.data.representations)
            {
                WearableItem.Representation wearableRepresentation = new WearableItem.Representation()
                {
                    bodyShapes = wearableDataRepresentation.bodyShapes,
                    overrideHides = wearableDataRepresentation.overrideHides,
                    overrideReplaces = wearableDataRepresentation.overrideReplaces,
                    mainFile = wearableDataRepresentation.mainFile
                };

                List<WearableItem.MappingPair> contentMappingPairs = new List<WearableItem.MappingPair>();
                foreach (var content in wearableDataRepresentation.contents)
                {
                    if (string.IsNullOrEmpty(content.url))
                    {
                        Debug.Log($"WearablesAPIData - Couldn't get hash from mappings for asset '{content.key}', it's content.url is null!");
                        
                        continue;
                    }
                    
                    contentMappingPairs.Add(new WearableItem.MappingPair()
                    {
                        key = content.key,
                        hash = content.url.Substring(content.url.LastIndexOf("/") + 1)
                    });
                }

                wearableRepresentation.contents = contentMappingPairs.ToArray();
                wearableRepresentations.Add(wearableRepresentation);
            }

            wearable.data.representations = wearableRepresentations.ToArray();
            result.Add(wearable);
        }

        return result;
    }
}

[Serializable]
public class WearableCollectionsAPIData
{
    [Serializable]
    public class Collection
    {
        public string urn;
        public string name;
    }

    public Collection[] data;
}
