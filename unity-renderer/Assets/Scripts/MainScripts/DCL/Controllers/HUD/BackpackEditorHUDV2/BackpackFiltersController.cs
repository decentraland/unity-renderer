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
        private HashSet<string> loadedCollections = new ();
        private NftCollectionType collectionType = NftCollectionType.OnChain | NftCollectionType.Base;
        private CancellationTokenSource loadThirdPartyCollectionsCancellationToken = new ();

        public BackpackFiltersController(
            IBackpackFiltersComponentView view,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.view = view;
            this.wearablesCatalogService = wearablesCatalogService;

            view.OnOnlyCollectiblesChanged += SetOnlyCollectibles;
            view.OnCollectionChanged += SetInternalStateOfCollectionsAndTriggerStateChange;
            view.OnSortByChanged += TriggerSorting;
            view.OnSearchTextChanged += TriggerTextSearch;
        }

        public void Dispose()
        {
            view.OnOnlyCollectiblesChanged -= SetOnlyCollectibles;
            view.OnCollectionChanged -= SetInternalStateOfCollectionsAndTriggerStateChange;
            view.OnSortByChanged -= TriggerSorting;
            view.OnSearchTextChanged -= TriggerTextSearch;

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

                    loadedCollections.Clear();
                    loadedCollections.Add(defaultCollection.urn);
                    if (collections != null)
                    {
                        foreach (var collection in collections)
                            loadedCollections.Add(collection.urn);
                    }

                    view.LoadCollectionDropdown(collections, defaultCollection);
                    collectionsAlreadyLoaded = true;
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            loadThirdPartyCollectionsCancellationToken = loadThirdPartyCollectionsCancellationToken.SafeRestart();
            FetchTPWAndLoadDropdown(loadThirdPartyCollectionsCancellationToken.Token).Forget();
        }

        public void ClearTextSearch(bool notify = true) =>
            view.SetSearchText(null, notify);

        public void SetTextSearch(string text, bool notify = true) =>
            view.SetSearchText(text, notify);

        public void SelectCollections(NftCollectionType collectionTypeMask,
            ICollection<string> thirdPartyCollectionIdsFilter = null,
            bool notify = true)
        {
            HashSet<string> newSelectedCollections = new ();

            if ((collectionTypeMask & NftCollectionType.OnChain) != 0)
                newSelectedCollections.Add(DECENTRALAND_COLLECTION_ID);

            if ((collectionTypeMask & NftCollectionType.ThirdParty) != 0 && thirdPartyCollectionIdsFilter != null)
                foreach (string tpw in thirdPartyCollectionIdsFilter)
                    newSelectedCollections.Add(tpw);

            bool isOnlyCollectiblesOn = (collectionTypeMask & NftCollectionType.Base) == 0;
            view.SetOnlyCollectiblesToggleIsOn(isOnlyCollectiblesOn, notify);
            view.SelectDropdownCollections(newSelectedCollections, notify);

            collectionType = collectionTypeMask;
            selectedCollections = newSelectedCollections;
        }

        public void SetSorting(NftOrderByOperation type, bool directionAscending, bool notify) =>
            view.SetSorting(type, directionAscending, notify);

        private void SetOnlyCollectibles(bool isOn)
        {
            if (isOn)
                collectionType &= ~NftCollectionType.Base;
            else
                collectionType |= NftCollectionType.Base;

            OnCollectionTypeChanged?.Invoke(collectionType);
        }

        private void SetInternalStateOfCollectionsAndTriggerStateChange(HashSet<string> newSelectedCollections) =>
            SetInternalStateOfCollectionsAndTriggerStateChange(newSelectedCollections, true);

        private void SetInternalStateOfCollectionsAndTriggerStateChange(HashSet<string> newSelectedCollections, bool notify)
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

            if (notify)
            {
                OnCollectionTypeChanged?.Invoke(collectionType);
                OnThirdPartyCollectionChanged?.Invoke(selectedCollections);
            }
        }

        private void TriggerSorting((NftOrderByOperation type, bool directionAscendent) newSorting) =>
            OnSortByChanged?.Invoke(newSorting);

        private void TriggerTextSearch(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
