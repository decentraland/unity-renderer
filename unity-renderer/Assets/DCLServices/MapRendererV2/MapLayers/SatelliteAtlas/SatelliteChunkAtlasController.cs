using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.SatelliteAtlas
{
    internal class SatelliteChunkAtlasController : MapLayerControllerBase, IAtlasController
    {
        public delegate UniTask<IChunkController> ChunkBuilder(Vector3 chunkLocalPosition, Vector2Int coordsCenter, Transform parent, CancellationToken ct);

        public const int CHUNKS_CREATED_PER_BATCH = 10;

        private readonly int chunkSize;
        private readonly int parcelsInsideChunk;
        private readonly ChunkBuilder chunkBuilder;
        private readonly List<IChunkController> chunks;

        private int parcelSize => coordsUtils.ParcelSize;

        public SatelliteChunkAtlasController(Transform parent, int chunkSize,
            ICoordsUtils coordsUtils, IMapCullingController cullingController, ChunkBuilder chunkBuilder) //, Func<UniTask<IChunkController>> chunkBuilder)
            : base(parent, coordsUtils, cullingController)
        {
            this.chunkSize = chunkSize;
            this.chunkBuilder = chunkBuilder;

            // var worldSize = ((Vector2)coordsUtils.WorldMaxCoords - coordsUtils.WorldMinCoords) * parcelSize;
            var chunkAmounts = new Vector2Int(8, 8); //new Vector2Int(Mathf.CeilToInt(worldSize.x / this.chunkSize), Mathf.CeilToInt(worldSize.y / this.chunkSize));
            chunks = new List<IChunkController>(chunkAmounts.x * chunkAmounts.y);
            parcelsInsideChunk = 38; //Mathf.Max(1, chunkSize / parcelSize);
            Debug.Log($"parcelsInsideChunk {parcelsInsideChunk}");
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ctsDisposing.Token, ct).Token;

            var chunksCreating = new List<UniTask<IChunkController>>(CHUNKS_CREATED_PER_BATCH);

            for (var i = 0; i < 8; i++)
            {
                float x = -2655.5f + (798.72f * i);
                    // (-152 * parcelSize) + (i * parcelsInsideChunk * parcelSize) - halfParcelSize;

                for (var j = 0; j < 8; j++)
                {
                    float y = 2637.24f - (798.72f * j);

                    if (chunksCreating.Count >= CHUNKS_CREATED_PER_BATCH)
                    {
                        chunks.AddRange(await UniTask.WhenAll(chunksCreating));
                        chunksCreating.Clear();
                    }

                    var localPosition = new Vector3(x, y, 0);

                    UniTask<IChunkController> instance = chunkBuilder.Invoke(chunkLocalPosition: localPosition, new Vector2Int(i, j), instantiationParent, linkedCt);
                    chunksCreating.Add(instance);
                }
            }

            if (chunksCreating.Count >= 0)
            {
                chunks.AddRange(await UniTask.WhenAll(chunksCreating));
                chunksCreating.Clear();
            }
        }

        UniTask IMapLayerController.Enable(CancellationToken cancellationToken)
        {
            Debug.Log("Enable satellite");
            instantiationParent.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        UniTask IMapLayerController.Disable(CancellationToken cancellationToken)
        {
            Debug.Log("Disable satellite");
            instantiationParent.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        protected override void DisposeImpl()
        {
            foreach (IChunkController chunk in chunks)
                chunk.Dispose();

            chunks.Clear();
        }
    }
}
