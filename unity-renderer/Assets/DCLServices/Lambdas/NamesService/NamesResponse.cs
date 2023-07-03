using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.Lambdas.NamesService
{
    [Serializable]
    public class NamesResponse : PaginatedResponse
    {
        [Serializable]
        public class NameEntry
        {
            [JsonProperty] private string name;
            [JsonProperty] private string contractAddress;
            [JsonProperty] private string price;

            public string Name => name;
            public string ContractAddress => contractAddress;
            public string Price => price;

            public NameEntry()
            {
            }

            public NameEntry(string name, string contractAddress, string price)
            {
                this.name = name;
                this.contractAddress = contractAddress;
                this.price = price;
            }

            public NftInfo GetNftInfo() =>
                new()
                {
                    Id = contractAddress,
                    Category = "name",
                };
        }

        [JsonProperty] private List<NameEntry> elements;

        public IReadOnlyList<NameEntry> Elements => elements;
    }
}
