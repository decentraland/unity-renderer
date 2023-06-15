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
            public string name;
            public string contractAddress;
            public string category;
            public string x;
            public string y;
            public string price;
            public string image;

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

        public List<LandEntry> elements;

        public IReadOnlyList<LandEntry> Elements => elements;
    }
}
