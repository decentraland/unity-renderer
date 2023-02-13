using DCLServices.Lambdas;
using System;
using System.Collections.Generic;

namespace DCLServices.WearablesCatalogService
{
    [Serializable]
    public class WearableResponse : PaginatedResponse
    {
        public List<WearableItem> wearables;
    }
}
