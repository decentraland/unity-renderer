using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.SatelliteAtlas
{
    internal class SatelliteChunkAtlasController : MapLayerControllerBase, IAtlasController
    {
        public delegate UniTask<IChunkController> ChunkBuilder(Vector3 chunkLocalPosition, Vector2Int coordsCenter, Transform parent, CancellationToken ct);

        private const int CHUNKS_CREATED_PER_BATCH = 5;

        private readonly int gridSize;
        private readonly int parcelsInsideChunk;

        private readonly ChunkBuilder chunkBuilder;
        private readonly List<IChunkController> chunks;

        public SatelliteChunkAtlasController(Transform parent, int gridSize, int parcelsInsideChunk, ICoordsUtils coordsUtils, IMapCullingController cullingController, ChunkBuilder chunkBuilder)
            : base(parent, coordsUtils, cullingController)
        {
            this.gridSize = gridSize;
            this.parcelsInsideChunk = parcelsInsideChunk;
            this.chunkBuilder = chunkBuilder;

            var chunkAmounts = new Vector2Int(gridSize, gridSize);
            chunks = new List<IChunkController>(chunkAmounts.x * chunkAmounts.y);
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            int chunkSpriteSize = parcelsInsideChunk * coordsUtils.ParcelSize;
            Vector3 offset = SatelliteMapOffset();

            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ctsDisposing.Token, ct).Token;

            var chunksCreating = new List<UniTask<IChunkController>>(CHUNKS_CREATED_PER_BATCH);

            for (var i = 0; i < gridSize; i++)
            {
                float x = offset.x + (chunkSpriteSize * i);

                for (var j = 0; j < gridSize; j++)
                {
                    float y = offset.y - (chunkSpriteSize * j);

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

        private Vector3 SatelliteMapOffset()
        {
            // World minimum plus half size of the chunk to get position (in parcels units) for the center of first chunk
            Vector2Int topLeftCornerChunkCenter = coordsUtils.WorldMinCoords + new Vector2Int(parcelsInsideChunk / 2, parcelsInsideChunk / 2);
            // offset by (3,2) parcels because Satellite image has border parcels outside of the world
            topLeftCornerChunkCenter = new Vector2Int(topLeftCornerChunkCenter.x - 3, Math.Abs(topLeftCornerChunkCenter.y - 2));

            return coordsUtils.CoordsToPosition(topLeftCornerChunkCenter);
        }

        UniTask IMapLayerController.Enable(CancellationToken cancellationToken)
        {
            instantiationParent.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        UniTask IMapLayerController.Disable(CancellationToken cancellationToken)
        {
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
