using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCLServices.WearablesCatalogService
{
    [Serializable]
    public class WearableCollectionResponseFromBuilder
    {
        [Serializable]
        public class Pagination
        {
            public int total;
            public int limit;
            public int pages;
            public int page;
            public BuilderWearable[] results;
        }

        public Pagination data;
    }

    [Serializable]
    public class WearableItemResponseFromBuilder
    {
        public BuilderWearable data;
    }

    [Serializable]
    public class BuilderWearable
    {
        [Serializable]
        public class BuilderWearableData
        {
            [Serializable]
            public class Representation
            {
                public string[] bodyShapes;
                public string mainFile;
                public string[] contents;
                public string[] overrideHides;
                public string[] overrideReplaces;

                public WearableItem.Representation ToWearableRepresentation(string contentUrl, Dictionary<string, string> hashes)
                {
                    return new WearableItem.Representation
                    {
                        bodyShapes = bodyShapes,
                        mainFile = mainFile,
                        overrideHides = overrideHides,
                        overrideReplaces = overrideReplaces,
                        contents = contents.Select(s => new WearableItem.MappingPair
                                            {
                                                key = s,
                                                hash = hashes[s],
                                                url = $"{contentUrl}/{hashes[s]}",
                                            })
                                           .ToArray(),
                    };
                }
            }

            public string category;
            public bool loop;
            public string[] replaces;
            public string[] hides;
            public string[] tags;
            public Representation[] representations;

            public WearableItem.Data ToWearableData(string contentUrl, Dictionary<string, string> hashes)
            {
                return new WearableItem.Data
                {
                    category = category,
                    hides = hides,
                    loop = loop,
                    replaces = replaces,
                    tags = tags,
                    representations = representations.Select(r => r.ToWearableRepresentation(contentUrl, hashes)).ToArray(),

                    // TODO: builder api does not include this information
                    removesDefaultHiding = Array.Empty<string>(),
                };
            }
        }

        public string id;
        public string name;
        public string description;
        public string rarity;
        public string type;
        public BuilderWearableData data;
        public Dictionary<string, string> contents;
        public string thumbnail;

        public WearableItem ToWearableItem(string contentUrl, string assetBundleUrl)
        {
            return new WearableItem
            {
                id = id,
                baseUrl = contentUrl,
                description = description,
                thumbnail = contents[thumbnail],
                rarity = rarity,
                i18n = new[]
                {
                    new i18n
                    {
                        code = "en",
                        text = name,
                    },
                },
                baseUrlBundles = assetBundleUrl,
                data = data.ToWearableData(contentUrl, contents),
                emoteDataV0 = type == "emote" ? new EmoteDataV0 { loop = data.loop } : null,
            };
        }
    }
}
