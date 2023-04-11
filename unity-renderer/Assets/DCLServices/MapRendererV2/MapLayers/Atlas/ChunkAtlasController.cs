using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Atlas
{
    internal class ChunkAtlasController : MapLayerControllerBase, IAtlasController
    {
        public delegate UniTask<IChunkController> ChunkBuilder(Vector3 chunkLocalPosition,
            Vector2Int coordsCenter,
            Transform parent,
            CancellationToken ct);

        public const int CHUNKS_CREATED_PER_BATCH = 10;

        private readonly SpriteRenderer prefab;
        private readonly int chunkSize;
        private readonly int parcelsInsideChunk;
        private readonly ChunkBuilder chunkBuilder;
        private readonly List<IChunkController> chunks;

        private int parcelSize => coordsUtils.ParcelSize;

        public ChunkAtlasController(Transform parent, SpriteRenderer prefab, int chunkSize,
            ICoordsUtils coordsUtils, IMapCullingController cullingController, ChunkBuilder chunkBuilder)
            : base(parent, coordsUtils, cullingController)
        {
            this.prefab = prefab;
            this.chunkSize = chunkSize;
            this.chunkBuilder = chunkBuilder;

            var worldSize = ((Vector2)coordsUtils.WorldMaxCoords - coordsUtils.WorldMinCoords) * parcelSize;
            var chunkAmounts = new Vector2Int(Mathf.CeilToInt(worldSize.x / this.chunkSize), Mathf.CeilToInt(worldSize.y / this.chunkSize));
            chunks = new List<IChunkController>(chunkAmounts.x * chunkAmounts.y);
            parcelsInsideChunk = Mathf.Max(1, chunkSize / parcelSize);
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ctsDisposing.Token, ct).Token;

            ClearCurrentChunks();
            float halfParcelSize = parcelSize * 0.5f;

            List<UniTask<IChunkController>> chunksCreating = new List<UniTask<IChunkController>>(CHUNKS_CREATED_PER_BATCH);

            for (int i = coordsUtils.WorldMinCoords.x; i <= coordsUtils.WorldMaxCoords.x; i += parcelsInsideChunk)
            {
                for (int j = coordsUtils.WorldMinCoords.y; j <= coordsUtils.WorldMaxCoords.y; j += parcelsInsideChunk)
                {
                    if (chunksCreating.Count >= CHUNKS_CREATED_PER_BATCH)
                    {
                        chunks.AddRange(await UniTask.WhenAll(chunksCreating));
                        chunksCreating.Clear();
                    }

                    Vector2Int coordsCenter = new Vector2Int(i, j);

                    // Subtract half parcel size to displace the pivot, this allow easier PositionToCoords calculations.
                    Vector3 localPosition = new Vector3((parcelSize * i) - halfParcelSize, (parcelSize * j) - halfParcelSize, 0);

                    var instance = chunkBuilder.Invoke(localPosition, coordsCenter, instantiationParent, linkedCt);
                    chunksCreating.Add(instance);
                }
            }

            if (chunksCreating.Count >= 0)
            {
                chunks.AddRange(await UniTask.WhenAll(chunksCreating));
                chunksCreating.Clear();
            }
        }

        private void ClearCurrentChunks()
        {
            foreach (IChunkController chunk in chunks)
                chunk.Dispose();

            chunks.Clear();
        }

        protected override void DisposeImpl()
        {
            ClearCurrentChunks();
        }

        // Atlas is always enabled and can't be turned off/on
        UniTask IMapLayerController.Enable(CancellationToken cancellationToken) =>
            UniTask.CompletedTask;

        UniTask IMapLayerController.Disable(CancellationToken cancellationToken) =>
            UniTask.CompletedTask;
    }
}
