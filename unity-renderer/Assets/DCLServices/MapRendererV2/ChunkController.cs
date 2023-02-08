using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class ChunkController : IChunkController
{
    private const int PIXELS_PER_UNIT = 50;
    private const string CHUNKS_API = "https://api.decentraland.org/v1/map.png";
    private const string CHUNK_PREFAB = "MyChunk";

    private SpriteRenderer spriteRenderer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="coordsCenter">0,0 will be considered at bottom left</param>
    /// <param name="parent"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async UniTask<IChunkController> CreateChunk(Vector3 chunkLocalPosition, int chunkSize, int parcelSize, Vector2Int coordsCenter, Transform parent,
        CancellationToken ct)
    {
        var chunk = new ChunkController(chunkLocalPosition, chunkSize, coordsCenter, parent);
        await chunk.LoadImage(chunkSize, parcelSize, coordsCenter);
        return chunk;
    }

    public ChunkController(Vector3 chunkLocalPosition, int chunkSize, Vector2Int coordsCenter, Transform parent)
    {
        spriteRenderer = Object.Instantiate(Resources.Load<SpriteRenderer>(CHUNK_PREFAB), parent);
#if UNITY_EDITOR
        spriteRenderer.gameObject.name = $"Chunk {coordsCenter.x},{coordsCenter.y}";
#endif
        var transform = spriteRenderer.transform;

        transform.localScale = Vector3.one * PIXELS_PER_UNIT;
        transform.localPosition = chunkLocalPosition;
    }

    public async UniTask LoadImage(int chunkSize, int parcelSize, Vector2Int mapPosition)
    {
        string url = $"{CHUNKS_API}?center={mapPosition.x},{mapPosition.y}&width={chunkSize}&height={chunkSize}&size={parcelSize}";
        Debug.Log(url);
        Texture2D result = null;

        var webRequest = UnityWebRequestTexture.GetTexture(url, true);
        await webRequest.SendWebRequest();
        var texture = DownloadHandlerTexture.GetContent(webRequest);
        texture.wrapMode = TextureWrapMode.Clamp;
        Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);

        spriteRenderer.sprite = newSprite;
    }

    public void Dispose()
    {
        //TODO destroy gameobject
    }
}
