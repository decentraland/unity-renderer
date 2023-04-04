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
    }

    [Serializable]
    public class WearableDefinition
    {
        public WearableItem definition;
    }
}
