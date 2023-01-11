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
            [SerializeField] private string name;
            [SerializeField] private string contractAddress;
            [SerializeField] private string price;

            public string Name => name;
            public string ContractAddress => contractAddress;
            public string Price => price;
        }

        [SerializeField] private List<NameEntry> names;

        public IReadOnlyList<NameEntry> Names => names;
    }
}
