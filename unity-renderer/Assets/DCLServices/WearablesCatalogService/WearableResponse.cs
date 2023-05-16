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
        public List<WearableDefinition> elements;

        public WearableWithDefinitionResponse(List<WearableDefinition> elements,
            int pageNum, int pageSize, int totalAmount)
        {
            this.elements = elements;
            this.pageNum = pageNum;
            this.pageSize = pageSize;
            this.totalAmount = totalAmount;
        }

        public WearableWithDefinitionResponse()
        {
        }
    }

    [Serializable]
    public class WearableDefinition
    {
        public string urn;
        public long maxTransferredAt;
        public WearableItem definition;
    }

    [Serializable]
    public class WearableElementDto
    {
        public string urn;
        public WearableIndividualDataDto[] individualData;
        public WearableEntityDto entity;

        public long GetMostRecentTransferTimestamp()
        {
            long max = 0;

            foreach (WearableIndividualDataDto dto in individualData)
            {
                var transferredAt = long.Parse(dto.transferredAt);

                if (transferredAt > max)
                    max = transferredAt;
            }

            return max;
        }
    }

    [Serializable]
    public class WearableEntityDto
    {
        public WearableItem metadata;
    }

    [Serializable]
    public class WearableIndividualDataDto
    {
        public string id;
        public string tokenId;
        public string transferredAt;
        public string price;
    }

    [Serializable]
    public class WearableWithEntityResponseDto : PaginatedResponse
    {
        public List<WearableElementDto> elements;

        public WearableWithEntityResponseDto(List<WearableElementDto> elements,
            int pageNum, int pageSize, int totalAmount)
        {
            this.elements = elements;
            this.pageNum = pageNum;
            this.pageSize = pageSize;
            this.totalAmount = totalAmount;
        }

        public WearableWithEntityResponseDto()
        {
        }
    }
}
