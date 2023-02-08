using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkAtlasController : IAtlasController
{
    public delegate UniTask<IChunkController> ChunkBuilder(Vector3 chunkLocalPosition, int chunkSize, int parcelSize, Vector2Int coordsCenter, Transform parent,
        CancellationToken ct);

    private const int CHUNKS_CREATED_PER_FRAME = 10;
    public static readonly Vector2Int WORLD_MIN_COORDS = new (-150, -150);
    public static readonly Vector2Int WORLD_MAX_COORDS = new (175, 175); // DCL map is not squared, there are some extra parcels in the top right
    public static readonly int TOTAL_ROWS = WORLD_MAX_COORDS.x - WORLD_MIN_COORDS.x; //We can use a single dimension due to the world/chunks being squared

    private readonly Transform parent;
    private readonly int chunkSize;
    private readonly int parcelSize;
    private readonly int parcelsInsideChunk;
    private readonly ChunkBuilder chunkBuilder;
    private readonly List<IChunkController> chunks;

    private CancellationTokenSource ctsDisposing = new CancellationTokenSource();

    public ChunkAtlasController(Transform parent, int chunkSize, int parcelSize, ChunkBuilder chunkBuilder)
    {
        this.parent = parent;
        this.chunkSize = chunkSize;
        this.parcelSize = parcelSize;
        this.chunkBuilder = chunkBuilder;

        var worldSize = TOTAL_ROWS * this.parcelSize;
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
                Vector3 localPosition = new Vector3((parcelSize * i) - halfParcelSize, (parcelSize * j) - halfParcelSize, 0); //Substract half parcel size to displace the pivot, this allow easier PositionToCoords calculations.
                chunksCreating.Add(chunkBuilder.Invoke(localPosition, chunkSize, parcelSize, coordsCenter, parent, linkedCt));
            }
        }

        if (chunksCreating.Count >= 0)
        {
            chunks.AddRange(await UniTask.WhenAll(chunksCreating));
            chunksCreating.Clear();
        }
    }

    public Vector2Int PositionToCoords(Vector3 pos)
    {
        return new Vector2Int(Mathf.CeilToInt(pos.x / parcelSize), Mathf.CeilToInt(pos.y / parcelSize));
    }

    private void ClearCurrentChunks()
    {
        foreach (IChunkController chunk in chunks) { chunk.Dispose(); }

        chunks.Clear();
    }

    public void Dispose()
    {
        ctsDisposing.Cancel();
        ctsDisposing.Dispose();
        ClearCurrentChunks();
    }
}
