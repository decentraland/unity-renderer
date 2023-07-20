using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.Lambdas.LandsService
{
    [Serializable]
    public class LandsResponse : PaginatedResponse
    {
        [Serializable]
        public class LandEntry
        {
            [JsonProperty] private string name;
            [JsonProperty] private string contractAddress;
            [JsonProperty] private string category;
            [JsonProperty] private string x;
            [JsonProperty] private string y;
            [JsonProperty] private string price;
            [JsonProperty] private string image;

            public string Name => name;
            public string ContractAddress => contractAddress;
            public string Category => category;
            public string X => x;
            public string Y => y;
            public string Price => price;
            public string Image => image;

            public NftInfo GetNftInfo() =>
                new()
                {
                    Id = contractAddress,
                    Category = category,
                };
        }

        [JsonProperty] private List<LandEntry> elements;

        public IReadOnlyList<LandEntry> Elements => elements;
    }
}
