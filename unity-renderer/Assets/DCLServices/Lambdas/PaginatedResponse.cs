using Newtonsoft.Json;
using System;

namespace DCLServices.Lambdas
{
    [Serializable]
    public abstract class PaginatedResponse
    {
        [JsonProperty] protected internal int pageNum;
        [JsonProperty] protected internal int pageSize;
        [JsonProperty] protected internal int totalAmount;

        public int PageNum => pageNum;
        public int PageSize => pageSize;
        public int TotalAmount => totalAmount;
    }
}
