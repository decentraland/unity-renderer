using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class WearablesAPIData
{
    private const string WEARABLES_CONTENT_BASE_URL = "https://peer.decentraland.org/content/contents/";

    [Serializable]
    public class WearableResponseRootData
    {
        [Serializable]
        public class Content
        {
            public string key;
            public string hash;
        }

        [Serializable]
        public class Metadata
        {
            [Serializable]
            public class Data
            {
                [Serializable]
                public class Representation
                {
                    public string[] bodyShapes;
                    public string mainFile;
                    public string[] overrideHides;
                    public string[] overrideReplaces;
                    public string[] contents;
                }

                public string[] replaces;
                public string[] hides;
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
        }

        public Content[] content;
        public Metadata metadata;
    }

    [Serializable]
    public class PaginationData
    {
        public int offset;
        public int limit;
        public bool moreData;
        public string next;
    }

    public List<WearableResponseRootData> deployments;
    public PaginationData pagination;

    public List<WearableItem> GetWearableItemsList()
    {
        if (deployments == null || deployments.Count == 0)
            return null;

        List<WearableItem> result = new List<WearableItem>();

        foreach (var wearableData in deployments)
        {
            // Populate new WearableItem with fetched data
            WearableItem wearable = new WearableItem()
            {
                id = wearableData.metadata.id,
                baseUrl = WEARABLES_CONTENT_BASE_URL,
                thumbnail = wearableData.metadata.thumbnail,
                rarity = wearableData.metadata.rarity,
                description = wearableData.metadata.description,
                i18n = wearableData.metadata.i18n,
                data = new WearableItem.Data()
                {
                    category = wearableData.metadata.data.category,
                    tags = wearableData.metadata.data.tags,
                    hides = wearableData.metadata.data.hides,
                    replaces = wearableData.metadata.data.replaces
                }
            };

            List<WearableItem.Representation> wearableRepresentations = new List<WearableItem.Representation>();
            foreach (var wearableDataRepresentation in wearableData.metadata.data.representations)
            {
                WearableItem.Representation wearableRepresentation = new WearableItem.Representation()
                {
                    bodyShapes = wearableDataRepresentation.bodyShapes,
                    overrideHides = wearableDataRepresentation.overrideHides,
                    overrideReplaces = wearableDataRepresentation.overrideReplaces,
                    mainFile = wearableDataRepresentation.mainFile
                };

                List<WearableItem.MappingPair> contentMappingPairs = new List<WearableItem.MappingPair>();
                foreach (var contentFileName in wearableDataRepresentation.contents)
                {
                    var contentData = wearableData.content.FirstOrDefault((x) => x.key == contentFileName);

                    if (contentData == null)
                    {
                        Debug.LogError($"Missing related content mapping pair info from fetched Wearable {wearableData.metadata.id}");
                        continue;
                    }

                    contentMappingPairs.Add(new WearableItem.MappingPair()
                    {
                        key = contentData.key,
                        hash = contentData.hash
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