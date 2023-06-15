﻿using System;
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
            public string name;
            public string contractAddress;
            public string price;

            public string Name => name;
            public string ContractAddress => contractAddress;
            public string Price => price;

            public NftInfo GetNftInfo() =>
                new()
                {
                    Id = contractAddress,
                    Category = "name",
                };
        }

        public List<NameEntry> elements;

        public IReadOnlyList<NameEntry> Elements => elements;
    }
}
