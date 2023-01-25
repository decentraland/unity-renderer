using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.WearablesCatalogService
{
    [Obsolete("This service will be replaced by LambdasWearablesCatalogService in the future.")]
    public class WebInterfaceWearablesCatalogService : MonoBehaviour, IWearablesCatalogService
    {
        public static WebInterfaceWearablesCatalogService Instance { get; private set; }

        public BaseDictionary<string, WearableItem> WearablesCatalog { get; } = new ();

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize() { }

        public void Dispose()
        {
            Destroy(this);
        }

        public async UniTask<WearableItem> RequestWearableAsync(string wearableId, CancellationToken ct) =>
            throw new NotImplementedException();

        public async UniTask<WearableItem[]> RequestOwnedWearablesAsync(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            throw new NotImplementedException();

        public async UniTask<WearableItem[]> RequestBaseWearablesAsync(CancellationToken ct) =>
            throw new NotImplementedException();

        public async UniTask<WearableItem[]> RequestThirdPartyWearablesByCollectionAsync(string userId, string collectionId, CancellationToken ct) =>
            throw new NotImplementedException();

        public void AddWearablesToCatalog(WearableItem[] wearableItems)
        {
            throw new NotImplementedException();
        }

        public void RemoveWearablesFromCatalog(IEnumerable<string> wearableIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveWearablesInUse(IEnumerable<string> wearablesInUseToRemove)
        {
            throw new NotImplementedException();
        }

        public void RemoveWearablesInUse(List<string> wearablesInUseToRemove)
        {
            throw new NotImplementedException();
        }

        public void EmbedWearables(IEnumerable<WearableItem> wearables)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
