using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCLServices.WearablesCatalogService
{
    [Obsolete("This service will be replaced by LambdasWearablesCatalogService in the future.")]
    public class WebInterfaceWearablesCatalogService : MonoBehaviour, IWearablesCatalogService
    {
        public static WebInterfaceWearablesCatalogService Instance { get; private set; }

        private void Awake() =>
            Instance = this;

        public void Initialize() { }

        public void Dispose() =>
            Destroy(gameObject);

        public UniTask<WearableItem> GetWearablesById(string wearableId, int pageNumber, int pageSize, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask<WearableItem[]> GetWearablesByCollection(string collectionId, int pageNumber, int pageSize, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask<WearableItem[]> GetWearablesByOwner(string userId, int pageNumber, int pageSize, CancellationToken ct) =>
            throw new NotImplementedException();
    }
}
