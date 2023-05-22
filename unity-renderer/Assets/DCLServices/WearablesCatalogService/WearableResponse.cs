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

            public long GetMostRecentTransferTimestamp()
            {
                if (individualData == null) return 0;

                long max = 0;

                foreach (IndividualDataDto dto in individualData)
                {
                    var transferredAt = long.Parse(dto.transferredAt);

                    if (transferredAt > max)
                        max = transferredAt;
                }

                return max;
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
