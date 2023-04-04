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
            [SerializeField] private string name;
            [SerializeField] private string contractAddress;
            [SerializeField] private string category;
            [SerializeField] private string x;
            [SerializeField] private string y;
            [SerializeField] private string price;
            [SerializeField] private string image;

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

        [SerializeField] private List<LandEntry> elements;

        public IReadOnlyList<LandEntry> Elements => elements;
    }
}
