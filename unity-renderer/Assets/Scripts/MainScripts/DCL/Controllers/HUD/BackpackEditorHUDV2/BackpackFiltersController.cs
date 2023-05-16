using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Threading;
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
        private readonly IWearablesCatalogService wearablesCatalogService;
        private bool collectionsAlreadyLoaded;
        private HashSet<string> selectedCollections = new();
        private NftCollectionType collectionType = NftCollectionType.OnChain | NftCollectionType.Base;
        private CancellationTokenSource loadThirdPartyCollectionsCancellationToken = new ();

        public BackpackFiltersController(
            IBackpackFiltersComponentView view,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.view = view;
            this.wearablesCatalogService = wearablesCatalogService;

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

            async UniTaskVoid FetchTPWAndLoadDropdown(CancellationToken cancellationToken)
            {
                try
                {
                    WearableCollectionsAPIData.Collection[] collections = await wearablesCatalogService.GetThirdPartyCollectionsAsync(cancellationToken);
                    WearableCollectionsAPIData.Collection defaultCollection = new () { urn = DECENTRALAND_COLLECTION_ID, name = "Decentraland" };
                    view.LoadCollectionDropdown(collections, defaultCollection);
                    collectionsAlreadyLoaded = true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            loadThirdPartyCollectionsCancellationToken = loadThirdPartyCollectionsCancellationToken.SafeRestart();
            FetchTPWAndLoadDropdown(loadThirdPartyCollectionsCancellationToken.Token).Forget();
        }

        private void SetOnlyCollectibles(bool isOn)
        {
            if (isOn)
                collectionType &= ~NftCollectionType.Base;
            else
                collectionType |= NftCollectionType.Base;

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
