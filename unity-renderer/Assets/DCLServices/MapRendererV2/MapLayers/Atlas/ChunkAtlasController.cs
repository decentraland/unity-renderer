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
            int chunkSize,
            int parcelSize,
            int drawOrder,
            Vector2Int coordsCenter,
            Transform parent,
            SpriteRenderer prefab,
            CancellationToken ct);

        private const int CHUNKS_CREATED_PER_FRAME = 10;

        public static readonly Vector2Int WORLD_MIN_COORDS = new (-150, -150);
        public static readonly Vector2Int WORLD_MAX_COORDS = new (175, 175); // DCL map is not squared, there are some extra parcels in the top right
        public static readonly int TOTAL_ROWS = WORLD_MAX_COORDS.x - WORLD_MIN_COORDS.x; //We can use a single dimension due to the world/chunks being squared

        private readonly SpriteRenderer prefab;
        private readonly int chunkSize;
        private readonly int parcelsInsideChunk;
        private readonly ChunkBuilder chunkBuilder;
        private readonly List<IChunkController> chunks;

        private int parcelSize => coordsUtils.ParcelSize;

        public ChunkAtlasController(Transform parent, SpriteRenderer prefab, int drawOrder, int chunkSize,
            ICoordsUtils coordsUtils, IMapCullingController cullingController, ChunkBuilder chunkBuilder)
            : base(parent, coordsUtils, cullingController, drawOrder)
        {
            this.prefab = prefab;
            this.chunkSize = chunkSize;
            this.chunkBuilder = chunkBuilder;

            var worldSize = TOTAL_ROWS * parcelSize;
            var chunkRows = Mathf.CeilToInt((float)worldSize / this.chunkSize);
            chunks = new List<IChunkController>(chunkRows * chunkRows);
            parcelsInsideChunk = chunkSize / parcelSize;
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ctsDisposing.Token, ct).Token;

            ClearCurrentChunks();
            float halfParcelSize = parcelSize * 0.5f;

            List<UniTask<IChunkController>> chunksCreating = new List<UniTask<IChunkController>>(CHUNKS_CREATED_PER_FRAME);

            for (int i = WORLD_MIN_COORDS.x; i <= WORLD_MAX_COORDS.x; i += parcelsInsideChunk)
            {
                for (int j = WORLD_MIN_COORDS.y; j <= WORLD_MAX_COORDS.y; j += parcelsInsideChunk)
                {
                    if (chunksCreating.Count >= CHUNKS_CREATED_PER_FRAME)
                    {
                        chunks.AddRange(await UniTask.WhenAll(chunksCreating));
                        chunksCreating.Clear();
                    }

                    Vector2Int coordsCenter = new Vector2Int(i, j);

                    // Subtract half parcel size to displace the pivot, this allow easier PositionToCoords calculations.
                    Vector3 localPosition = new Vector3((parcelSize * i) - halfParcelSize, (parcelSize * j) - halfParcelSize, 0);

                    var instance = chunkBuilder.Invoke(localPosition, chunkSize, coordsUtils.ParcelSize, drawOrder, coordsCenter, instantiationParent, prefab, linkedCt);
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
