using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.Lambdas.WearablesCatalogService
{
    [Serializable]
    public class WearableResponse : PaginatedResponse
    {
        [Serializable]
        public class WearableIndividualData
        {
            [SerializeField] private string id;
            [SerializeField] private string transferredAt;
            [SerializeField] private string price;
        }

        [Serializable]
        public class WearableEntry
        {
            [SerializeField] private string urn;
            [SerializeField] private string image;
            [SerializeField] private string name;
            [SerializeField] private string description;
            [SerializeField] private string rarity;
            [SerializeField] private WearableIndividualData[] individualData;

            public string Urn => urn;
            public string Image => image;
            public string Name => name;
            public string Description => description;
            public string Rarity => rarity;
            public WearableIndividualData[] IndividualData => individualData;
        }

        [SerializeField] private List<WearableEntry> wearables;

        public IReadOnlyList<WearableEntry> Wearables => wearables;
    }
}
