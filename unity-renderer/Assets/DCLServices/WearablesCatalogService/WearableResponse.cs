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
}
