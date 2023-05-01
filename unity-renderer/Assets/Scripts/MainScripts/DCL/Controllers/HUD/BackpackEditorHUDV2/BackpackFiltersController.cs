using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackFiltersController
    {
        private const string DECENTRALAND_COLLECTION_ID = "decentraland";

        public event Action<HashSet<string>> OnThirdPartyCollectionChanged;
        public event Action<(NftOrderByOperation type, bool directionAscendent)> OnSortByChanged;
        public event Action<string> OnSearchTextChanged;
        public event Action<NftCollectionType> OnCollectionTypeChanged;

        private readonly IBackpackFiltersComponentView view;
        private bool collectionsAlreadyLoaded;
        private HashSet<string> selectedCollections = new();
        private NftCollectionType collectionType = NftCollectionType.OnChain;

        public BackpackFiltersController(IBackpackFiltersComponentView view)
        {
            this.view = view;

            view.OnOnlyCollectiblesChanged += SetOnlyCollectibles;
            view.OnCollectionChanged += SetCollections;
            view.OnSortByChanged += SetSorting;
            view.OnSearchTextChanged += SetSearchText;
        }

        public void Dispose()
        {
            view.OnOnlyCollectiblesChanged -= SetOnlyCollectibles;
            view.OnCollectionChanged -= SetCollections;
            view.OnSortByChanged -= SetSorting;
            view.OnSearchTextChanged -= SetSearchText;

            view.Dispose();
        }

        public void LoadCollections()
        {
            if (collectionsAlreadyLoaded)
                return;

            WearablesFetchingHelper.GetThirdPartyCollections()
                                   .Then(collections =>
                                    {
                                        WearableCollectionsAPIData.Collection defaultCollection = new () { urn = DECENTRALAND_COLLECTION_ID, name = "Decentraland" };
                                        view.LoadCollectionDropdown(collections, defaultCollection);
                                        collectionsAlreadyLoaded = true;
                                    })
                                   .Catch((error) => Debug.LogError(error));
        }

        private void SetOnlyCollectibles(bool isOn)
        {
            if (isOn)
            {
                collectionType |= NftCollectionType.OnChain;
                collectionType &= ~NftCollectionType.Base;
            }
            else
                collectionType |= NftCollectionType.OnChain | NftCollectionType.Base;

            OnCollectionTypeChanged?.Invoke(collectionType);
        }

        private void SetCollections(HashSet<string> newSelectedCollections)
        {
            if (newSelectedCollections.Contains(DECENTRALAND_COLLECTION_ID))
            {
                collectionType |= NftCollectionType.OnChain;
                newSelectedCollections.Remove(DECENTRALAND_COLLECTION_ID);
            }
            else
                collectionType &= ~NftCollectionType.OnChain;

            selectedCollections = newSelectedCollections;

            if (newSelectedCollections.Count > 0)
                collectionType |= NftCollectionType.ThirdParty;
            else
                collectionType &= ~NftCollectionType.ThirdParty;

            OnCollectionTypeChanged?.Invoke(collectionType);
            OnThirdPartyCollectionChanged?.Invoke(selectedCollections);
        }

        private void SetSorting((NftOrderByOperation type, bool directionAscendent) newSorting) =>
            OnSortByChanged?.Invoke(newSorting);

        private void SetSearchText(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
