using DCLServices.Lambdas;
using System;
using System.Collections.Generic;

namespace DCLServices.WearablesCatalogService
{
    [Serializable]
    public class WearableWithoutDefinitionResponse
    {
        public List<WearableItem> wearables;
    }

    [Serializable]
    public class WearableWithDefinitionResponse : PaginatedResponse
    {
        public List<WearableElementV1Dto> elements;

        public WearableWithDefinitionResponse(List<WearableElementV1Dto> elements,
            int pageNum, int pageSize, int totalAmount)
        {
            this.elements = elements;
            this.pageNum = pageNum;
            this.pageSize = pageSize;
            this.totalAmount = totalAmount;
        }

        public WearableWithDefinitionResponse() { }
    }

    [Serializable]
    [Obsolete("Deprecated. Use WearableElementV2Dto instead")]
    public class WearableElementV1Dto
    {
        public string urn;
        public long maxTransferredAt;
        public WearableItem definition;
    }

    [Serializable]
    public class WearableWithEntityResponseDto : PaginatedResponse
    {
        [Serializable]
        public class ElementDto
        {
            [Serializable]
            public class IndividualDataDto
            {
                public string id;
                public string tokenId;
                public string transferredAt;
                public string price;
            }

            [Serializable]
            public class EntityDto
            {
                [Serializable]
                public class ContentDto
                {
                    public string file;
                    public string hash;
                }

                [Serializable]
                public class MetadataDto
                {
                    [Serializable]
                    public class Representation
                    {
                        public string[] bodyShapes;
                        public string mainFile;
                        public string[] contents;
                        public string[] overrideHides;
                        public string[] overrideReplaces;
                    }

                    [Serializable]
                    public class DataDto
                    {
                        public Representation[] representations;
                        public string category;
                        public string[] tags;
                        public string[] replaces;
                        public string[] hides;
                    }

                    public DataDto data;
                    public string id;

                    public i18n[] i18n;
                    public string thumbnail;

                    public string rarity;
                    public string description;
                }

                public MetadataDto metadata;
                public ContentDto[] content;

                public string GetContentHashByFileName(string fileName)
                {
                    foreach (ContentDto dto in content)
                        if (dto.file == fileName)
                            return dto.hash;

                    return null;
                }
            }

            public string urn;
            public IndividualDataDto[] individualData;
            public EntityDto entity;
            public string rarity;

            public long GetMostRecentTransferTimestamp()
            {
                if (individualData == null) return 0;

                long max = 0;

                foreach (IndividualDataDto dto in individualData)
                {
                    if (!long.TryParse(dto.transferredAt, out long transferredAt))
                        continue;

                    if (transferredAt > max)
                        max = transferredAt;
                }

                return max;
            }

            public WearableItem ToWearableItem(string contentBaseUrl, string bundlesBaseUrl)
            {
                EntityDto.MetadataDto metadata = entity.metadata;

                WearableItem wearable = new WearableItem
                {
                    data = new WearableItem.Data
                    {
                        representations = new WearableItem.Representation[metadata.data.representations.Length],
                        category = metadata.data.category,
                        hides = metadata.data.hides,
                        replaces = metadata.data.replaces,
                        tags = metadata.data.tags,
                    },
                    baseUrl = contentBaseUrl,
                    baseUrlBundles = bundlesBaseUrl,
                    emoteDataV0 = null,
                    description = metadata.description,
                    i18n = metadata.i18n,
                    id = metadata.id,
                    rarity = metadata.rarity ?? rarity,
                    thumbnail = entity.GetContentHashByFileName(metadata.thumbnail),
                    MostRecentTransferredDate = DateTimeOffset.FromUnixTimeSeconds(GetMostRecentTransferTimestamp())
                                                              .DateTime,
                };

                for (var i = 0; i < metadata.data.representations.Length; i++)
                {
                    EntityDto.MetadataDto.Representation representation = metadata.data.representations[i];

                    wearable.data.representations[i] = new WearableItem.Representation
                    {
                        bodyShapes = representation.bodyShapes,
                        mainFile = representation.mainFile,
                        overrideHides = representation.overrideHides,
                        overrideReplaces = representation.overrideReplaces,
                        contents = new WearableItem.MappingPair[representation.contents.Length],
                    };

                    for (var z = 0; z < representation.contents.Length; z++)
                    {
                        string fileName = representation.contents[z];
                        string hash = entity.GetContentHashByFileName(fileName);

                        wearable.data.representations[i].contents[z] = new WearableItem.MappingPair
                        {
                            url = $"{contentBaseUrl}/{hash}",
                            hash = hash,
                            key = fileName,
                        };
                    }
                }

                return wearable;
            }
        }

        public List<ElementDto> elements;

        public WearableWithEntityResponseDto(List<ElementDto> elements,
            int pageNum, int pageSize, int totalAmount)
        {
            this.elements = elements;
            this.pageNum = pageNum;
            this.pageSize = pageSize;
            this.totalAmount = totalAmount;
        }

        public WearableWithEntityResponseDto() { }
    }
}
